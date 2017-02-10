////////////////////////////////////////////////////////////////////////////
//
// Copyright 2016 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Realms;
using Realms.Exceptions;
using Realms.Sync;
using Realms.Sync.Exceptions;
using SkiaSharp;

namespace DrawXShared
{
    /***
     Class that does almost everything for the demo that can be shared.
     It combines drawing with logging in and connecting to the server.

     DrawXSettingsManager provides a singleton to wrap settings and their local Realm.

    Login
    -----
    We first try the Realm built-in crediential sture in User.Current.
    Otherwise, credentials come from DrawXSettings, which remembers our last choice of server and username/password, as well as other settings.
    It is the responsibility of external GUI classes to get credentials entered and delay
    starting RealmDraw until connection is made.

    Drawing
    -------
    There are three components to drawing:
    - the background images for "controls"
    - previously drawn completed, paths which will no longer grow,
    - the currently growing paths (one per app updating the shared Realm).

    However, with the paths, if we only worry about drawing _added_ or _changed_ paths
    then we don't care if they are completed or not, just that they are dirty.

    Caching and Responsiveness
    --------------------------
    Ideally, we want to have almost all the drawn content cached in a bitmap for redisplay, 
    and only draw new line segments for each added point. It is relatively easy to optimise for
    local draw updates because we know when touched that we are drawing a single added segment.

    There are two distinct patterns which can cause a poor demo with synchronised Draw.
    - A long, winding path being updated - there may be enough lag that we have to add more than 
      one point to it locally but we don't want to redraw the whole thing from scratch.      
    - Many small, single "dab" strokes being drawn, say from someone tapping a display, 
      which mean we have, at least, a TouchesBegan and TouchesEnded and probably AddPoint in between.
    
    We simply monitor changes - any *added* or *changed* paths get redrawn **entirely**.

    Most importantly, to get the fastest possible response as the user moves their finger,
    we draw the local line immediately as a continuation of the path they started drawing 
    earlier. (See _currentlyDrawing)
    
    */
    public class RealmDraw
    {
        private const float NORMALISE_TO = 4000.0f;
        private const float PENCIL_MARGIN = 4.0f;
        private const float INVALID_LAST_COORD = -1.0f;
        private float _lastX = INVALID_LAST_COORD;
        private float _lastY = INVALID_LAST_COORD;

        #region Synchronised data
        private Realm _realm;
        private IQueryable<DrawPath> _allPaths;  // we observe all and filter based on changes
        #endregion

        #region GUI Callbacks
        internal Action RefreshOnRealmUpdate { get; set; }

        internal Action CredentialsEditor { get; set; }

        internal Action<bool, string> ReportError { get; set; }  // params isError, msg
        #endregion

        #region DrawingState
        private bool _isDrawing = false;
        private bool _ignoringTouches = false;
        private DrawPath _drawPath;
        private SKPath _currentlyDrawing;  // caches for responsive drawing on this device
        private float _canvasWidth, _canvasHeight;
        private IList<DrawPath> _pathsToDraw = null;  // set in notification callback
        #endregion

        #region CachedCanvas
        private int _canvasSaveCount;  // from SaveLayer
        private bool _hasSavedBitmap = false;  // separate flag so we don't rely on any given value in _canvasSaveCount
        private bool _redrawPathsAtNextDraw = true;
        #endregion

        #region Touch Areas
        private SKRect _loginIconRect;
        private SKRect _loginIconTouchRect;
        //// setup in DrawBackground
        private float _pencilWidth;
        private float _pencilsTop;
        private int _numPencils;
        private List<SKBitmap> _pencilBitmaps;
        private SKBitmap _loginIconBitmap;
        #endregion

        #region LoginState
        private bool _waitingForLogin = false;
        #endregion

        #region Settings
        private DrawXSettings Settings => DrawXSettingsManager.Settings;

        private int _currentColorIndex = -1;  //// store as int for quick check if pencil we draw is current color

        private SwatchColor CurrentColor
        {
            get
            {
                if (_currentColorIndex == -1)
                {
                    InitCurrentColor();
                }

                return SwatchColor.Colors[_currentColorIndex];
            }

            set
            {
                if (_currentColorIndex == -1 || !value.Name.Equals(SwatchColor.Colors[_currentColorIndex]))
                {
                    DrawXSettingsManager.Write(() => Settings.LastColorUsed = value.Name);
                    _currentColorIndex = SwatchColor.Colors.IndexOf(value);
                }
            }
        }

        private void InitCurrentColor()
        {
            var currentByName = SwatchColor.ColorsByName[Settings.LastColorUsed];
            _currentColorIndex = SwatchColor.Colors.IndexOf(currentByName);
        }
        #endregion Settings

        public RealmDraw(float inWidth, float inHeight)
        {
            _canvasWidth = inWidth;
            _canvasHeight = inHeight;

            // simple local open            
            // _realm = Realm.GetInstance("DrawX.realm");

            _pencilBitmaps = new List<SKBitmap>(SwatchColor.Colors.Count);
            foreach (var swatch in SwatchColor.Colors)
            {
                _pencilBitmaps.Add(EmbeddedMedia.BitmapNamed(swatch.Name + ".png"));
            }

            _loginIconBitmap = EmbeddedMedia.BitmapNamed("CloudIcon.png");
            ReportError = (isError, msg) => System.Diagnostics.Debug.WriteLine(msg); // default expect override by apps
        }

        internal void InvalidateCachedPaths()
        {
            _redrawPathsAtNextDraw = true;
            _hasSavedBitmap = false;
            _currentlyDrawing = null;
        }

        internal async void LoginToServerAsync(User user = null)
        {
            // in case have lingering subscriptions, clear by clearing the results to which we subscribe
            _allPaths = null;

            _waitingForLogin = true;

            // assuming we are calling Login again after being logged in already, which has been guarded
            // to see that there IS a change in server/user, we need to logout
            try
            {
                foreach(var activeUser in User.AllLoggedIn)
                {
                    activeUser.LogOut();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Unexpected exception getting User.AllLoggedIn {e}");
            }

            var s = Settings;
            //// TODO allow entering Create User flag on credentials to pass in here instead of false
            Credentials credentials = user == null ? Credentials.UsernamePassword(s.Username, s.Password, false) : null;
            try
            {
                if (user == null)
                {
                    user = await User.LoginAsync(credentials, new Uri($"http://{s.ServerIP}"));
                    Debug.WriteLine($"Got user logged in with refresh token {user.RefreshToken}");
                }
                var loginConf = new SyncConfiguration(user, new Uri($"realm://{s.ServerIP}/~/Draw"));
                _realm = Realm.GetInstance(loginConf);
                SetupPathChangeMonitoring();
            }
            catch (AuthenticationException)
            {
                ReportError(false, $"Unknown Username \n{s.Username} and Password \n{s.Password} combination");
            }
            catch (System.Net.Sockets.SocketException sockEx)
            {
                ReportError(true, $"Network error: {sockEx}");
            }
            catch (WebException webEx)
            {
                if (webEx.Status == System.Net.WebExceptionStatus.ConnectFailure)
                {
                    ReportError(true, $"Unable to connect to {s.ServerIP} - check address {webEx.Message}");
                }
                else
                {
                    ReportError(true, $"Error trying to login to {s.ServerIP} with {s.Username}/{s.Password} {webEx.Message}");
                }
            }
            catch (Exception e)
            {
                if (user == null)
                {
                    ReportError(true, $"Error trying to login to {s.ServerIP} with {s.Username}/{s.Password} {e.GetType().FullName} {e.Message}");
                }
                else
                {
                    ReportError(true, $"Credentials accepted at {s.ServerIP} but then failed to open Realm: {e.GetType().FullName} {e.Message}");
                }
            }

            if (user != null)
            {
                // cleanup the graphics
                InvalidateCachedPaths();
                RefreshOnRealmUpdate();
            }

            _waitingForLogin = false;
        }

        private void SetupPathChangeMonitoring()
        {
            _allPaths = _realm.All<DrawPath>() as IQueryable<DrawPath>;
            _allPaths.SubscribeForNotifications((sender, changes, error) =>
            {
                // WARNING ChangeSet indices are only valid inside this callback
                if (changes == null)  // initial call
                {
                    RefreshOnRealmUpdate();  // force initial draw on login
                    return;
                }
                //// we assume if at least one path deleted, drastic stuff happened, probably erase all
                if (_allPaths.Count() == 0 || changes.DeletedIndices.Length > 0)
                {
                    InvalidateCachedPaths();  // someone erased their tablet
                    RefreshOnRealmUpdate();
                    return;
                }

                var numInserted = changes.InsertedIndices.Length;
                var numChanged = changes.ModifiedIndices.Length;
                if ((numInserted == 0 && numChanged == 1 && _allPaths.ElementAt(changes.ModifiedIndices[0]) == _drawPath) ||
                    (numInserted == 1 && numChanged == 0 && _allPaths.ElementAt(changes.InsertedIndices[0]) == _drawPath))
                {
                    // current path is drawn by immediate action, not by a callback
                    return;
                }

                if (_pathsToDraw == null)
                {
                    _pathsToDraw = new List<DrawPath>();  // otherwise previous draw got interrupted.
                }

                foreach (var index in changes.InsertedIndices)
                {
                    _pathsToDraw.Add(_allPaths.ElementAt(index));
                }

                foreach (var index in changes.ModifiedIndices)
                {
                    _pathsToDraw.Add(_allPaths.ElementAt(index));
                }

                RefreshOnRealmUpdate();
            });
        }

        private void ScalePointsToStore(ref float w, ref float h)
        {
            w *= NORMALISE_TO / _canvasWidth;
            h *= NORMALISE_TO / _canvasHeight;
        }

        private void ScalePointsToDraw(ref float w, ref float h)
        {
            w *= _canvasWidth / NORMALISE_TO;
            h *= _canvasHeight / NORMALISE_TO;
        }

        private bool TouchInControlArea(float inX, float inY)
        {
            if (_realm == null || _loginIconTouchRect.Contains(inX, inY))  // treat entire screen as control area
            {
                InvalidateCachedPaths();
                CredentialsEditor.Invoke();  // TODO only invalidate if changed server??
                return true;
            }

            if (inY < _pencilsTop)
            {
                return false;
            }

            // see opposite calc in DrawBackground
            var pencilIndex = (int)(inX / (_pencilWidth + PENCIL_MARGIN));
            var selectecColor = SwatchColor.Colors[pencilIndex];
            if (!selectecColor.Name.Equals(CurrentColor.Name))
            {
                CurrentColor = selectecColor;  // will update saved settings
            }

            InvalidateCachedPaths();
            return true;  // if in this area even if didn't actually change
        }

        private void DrawPencils(SKCanvas canvas, SKPaint paint)
        {
            // draw pencils, assigning the fields used for touch detection
            _numPencils = SwatchColor.ColorsByName.Count;
            if (_currentColorIndex == -1)
            {
                InitCurrentColor();
            }

            var marginAlloc = (_numPencils + 1) * PENCIL_MARGIN;
            _pencilWidth = (canvas.ClipBounds.Width - marginAlloc) / _numPencils;  // see opposite calc in TouchInControlArea
            var pencilHeight = _pencilWidth * 334.0f / 112.0f;  // scale as per originals
            var runningLeft = PENCIL_MARGIN;
            var pencilsBottom = canvas.ClipBounds.Height;
            _pencilsTop = pencilsBottom - pencilHeight;
            int _pencilIndex = 0;
            foreach (var swatchBM in _pencilBitmaps)
            {
                var pencilRect = new SKRect(runningLeft, _pencilsTop, runningLeft + _pencilWidth, pencilsBottom);
                if (_pencilIndex++ == _currentColorIndex)
                {
                    var offsetY = -Math.Max(20.0f, pencilHeight / 4.0f);
                    pencilRect.Offset(0.0f, offsetY);  // show selected color
                }

                canvas.DrawBitmap(swatchBM, pencilRect, paint);
                runningLeft += PENCIL_MARGIN + _pencilWidth;
            }
        }

        private void DrawLoginIcon(SKCanvas canvas, SKPaint paint)
        {
            if (_loginIconRect.Width <= 0.1f)
            {
                const float ICON_WIDTH = 84.0f;
                const float ICON_HEIGHT = 54.0f;
#if __IOS__
                const float TOP_BAR_OFFSET = 48.0f;
#else
                const float TOP_BAR_OFFSET = 8.0f;
#endif
                _loginIconRect = new SKRect(8.0f, TOP_BAR_OFFSET, 8.0f + ICON_WIDTH, TOP_BAR_OFFSET + ICON_HEIGHT);
                _loginIconTouchRect = new SKRect(0.0f, 0.0f,
                                                 Math.Max(_loginIconRect.Right + 4.0f, 44.0f),
                                                 Math.Max(_loginIconRect.Bottom + 4.0f, 44.0f));
            }

            canvas.DrawBitmap(_loginIconBitmap, _loginIconRect, paint);
        }

        private void DrawAPath(SKCanvas canvas, SKPaint paint, DrawPath drawPath)
        {
            using (var path = new SKPath())
            {
                // nasty hack because the Cocoa version defaults to Black
                var pathColorName = (drawPath.Color == "Black") ? "Charcoal" : drawPath.Color;
                var pathColor = SwatchColor.ColorsByName[pathColorName].Color;
                paint.Color = pathColor;
                var isFirst = true;
                foreach (var point in drawPath.Points)
                {
                    // for compatibility with iOS Realm, stores floats, normalised to NORMALISE_TO
                    var fx = (float)point.X;
                    var fy = (float)point.Y;
                    ScalePointsToDraw(ref fx, ref fy);
                    if (isFirst)
                    {
                        isFirst = false;
                        path.MoveTo(fx, fy);
                    }
                    else
                    {
                        path.LineTo(fx, fy);
                    }
                }

                canvas.DrawPath(path, paint);
            }
        }

        private void InitPaint(SKPaint paint)
        {
            paint.Style = SKPaintStyle.Stroke;
            paint.StrokeWidth = 10;  // TODO scale width depending on device width
            paint.IsAntialias = true;
            paint.StrokeCap = SKStrokeCap.Round;
            paint.StrokeJoin = SKStrokeJoin.Round;
        }

        // replaces the CanvasView.drawRect of the original
        // draw routine called when screen refreshed
        // Note the canvas only exists during this call
        public void DrawTouches(SKCanvas canvas)
        {
            if (_realm == null)
            {
                return;  // too early to have finished login
            }

            if (_hasSavedBitmap)
            {
                canvas.RestoreToCount(_canvasSaveCount);  // use up the offscreen bitmap regardless
            }

            using (var paint = new SKPaint())
            {
                InitPaint(paint);
                if (_redrawPathsAtNextDraw)
                {
                    canvas.Clear(SKColors.White);
                    DrawPencils(canvas, paint);
                    DrawLoginIcon(canvas, paint);
                    foreach (var drawPath in _realm.All<DrawPath>())
                    {
                        DrawAPath(canvas, paint, drawPath);
                    }

                    _pathsToDraw = null;
                }
                else
                {
                    // current paths being drawn, by other devices
                    if (_pathsToDraw != null)
                    {
                        foreach (var drawPath in _pathsToDraw)
                        {
                            DrawAPath(canvas, paint, drawPath);
                        }
                    }
                }

                if (_currentlyDrawing != null)
                {
                    paint.Color = CurrentColor.Color;
                    canvas.DrawPath(_currentlyDrawing, paint);
                }

                _canvasSaveCount = canvas.SaveLayer(paint);  // cache everything to-date
                _hasSavedBitmap = true;
            } // SKPaint
            _pathsToDraw = null;
            _redrawPathsAtNextDraw = false;
        }

        public void StartDrawing(float inX, float inY)
        {
            _currentlyDrawing = null;  // don't clear in Stop as will lose last point, clear when we know done
            _ignoringTouches = false;

            // when not logged in, entire screen acts as control area that will trigger CredentialsEditor
            if (TouchInControlArea(inX, inY))
            {
                _ignoringTouches = true;
                return;
            }


            // check if needs login, eg: if login failed and touch blank screen should cope
            if (_realm == null)
            {
                if (!_waitingForLogin)
                {
                    LoginToServerAsync();
                }

                return;  // not yet logged into server, let next touch invoke us
            }

            _lastX = inX;
            _lastY = inY;

            // start a local path for responsive drawing
            _currentlyDrawing = new SKPath();
            _currentlyDrawing.MoveTo(inX, inY);

            ScalePointsToStore(ref inX, ref inY);
            _isDrawing = true;
            _realm.Write(() =>
            {
                _drawPath = new DrawPath { Color = CurrentColor.Name };  // Realm saves name of color
                _drawPath.Points.Add(new DrawPoint { X = inX, Y = inY });
                _realm.Add(_drawPath);
            });
        }

        public void AddPoint(float inX, float inY)
        {
            if (_ignoringTouches)
            {
                return;  // probably touched in pencil area
            }

            if (_realm == null)
            {
                return;  // not yet logged into server
            }

            if (!_isDrawing)
            {
                // has finished connecting to Realm so this is actually a start
                StartDrawing(inX, inY);
                return;
            }

            _lastX = inX;
            _lastY = inY;
            _currentlyDrawing.LineTo(inX, inY);

            ScalePointsToStore(ref inX, ref inY);
            _realm.Write(() =>
            {
                _drawPath.Points.Add(new DrawPoint { X = inX, Y = inY });
            });
        }

        public void StopDrawing(float inX, float inY)
        {
            if (_ignoringTouches)
            {
                return;  // probably touched in pencil area
            }

            _isDrawing = false;
            if (_realm == null)
            {
                return;  // not yet logged into server
            }

            var movedWhilstStopping = (_lastX == inX) && (_lastY == inY);
            _lastX = INVALID_LAST_COORD;
            _lastY = INVALID_LAST_COORD;

            if (movedWhilstStopping)
            {
                _currentlyDrawing.LineTo(inX, inY);
            }

            ScalePointsToStore(ref inX, ref inY);
            _realm.Write(() =>
            {
                if (movedWhilstStopping)
                {
                    _drawPath.Points.Add(new DrawPoint { X = inX, Y = inY });
                }
                _drawPath.DrawerId = "";  // objc original uses this to detect a "finished" path
            });
        }

        public void CancelDrawing()
        {
            _isDrawing = false;
            _ignoringTouches = false;
            _lastX = INVALID_LAST_COORD;
            _lastY = INVALID_LAST_COORD;
            InvalidateCachedPaths();
            //// TODO wipe current path
        }

        public void ErasePaths()
        {
            InvalidateCachedPaths();
            _realm.Write(() =>
            {
                _realm.RemoveAll<DrawPath>();
                _realm.RemoveAll<DrawPoint>();  // we don't yet have cascading delete https://github.com/realm/realm-dotnet/issues/310
            });
        }
    }
}
