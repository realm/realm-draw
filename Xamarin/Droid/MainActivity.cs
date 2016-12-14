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
using Android.App;
using Android.OS;
using Android.Views;
using DrawXShared;
using SkiaSharp.Views.Android;

namespace DrawX.Droid
{
    [Activity(Label = "DrawX", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : Activity
    {
        private RealmDraw _drawer;
        private SKCanvasView _canvas;
        private bool _hasShownCredentials;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            DrawXSettingsManager.InitLocalSettings();
        }

        private void SetupDrawer()
        {
            _canvas = FindViewById<SKCanvasView>(Resource.Id.canvas);
            _canvas.PaintSurface += OnPaintSample;
            _canvas.Touch += OnTouch;
            //// deferred update until can get view bounds
            _drawer = new RealmDraw(_canvas.CanvasSize.Width, _canvas.CanvasSize.Height);
            _drawer.CredentialsEditor = () =>
            {
                EditCredentials();
            };
            _drawer.RefreshOnRealmUpdate = () =>
            {
                System.Diagnostics.Debug.WriteLine("Refresh callback triggered by Realm");
                _canvas.Invalidate();
            };
        }

        protected override void OnStart()
        {
            base.OnStart();
            if (_drawer == null)
            {
                if (DrawXSettingsManager.HasCredentials())
                {
                    // assume we can login and be able to draw
                    // TODO handle initial failure to login despite saved credentials
                    SetupDrawer();
                }
            }

            if (!_hasShownCredentials)
            {
                EditCredentials();
                _hasShownCredentials = true;
            }
        }

        private void OnPaintSample(object sender, SKPaintSurfaceEventArgs e)
        {
            _drawer?.DrawTouches(e.Surface.Canvas);
        }

        private void OnTouch(object sender, View.TouchEventArgs touchEventArgs)
        {
            if (_drawer == null)
            {
                return;  // in case managed to trigger before focus event finished setup
            }
            
            float fx = touchEventArgs.Event.GetX();
            float fy = touchEventArgs.Event.GetY();
            var needsRefresh = false;
            switch (touchEventArgs.Event.Action & MotionEventActions.Mask)
            {
                case MotionEventActions.Down:
                    _drawer.StartDrawing(fx, fy);
                    needsRefresh = true;
                    break;

                case MotionEventActions.Move:
                    _drawer.AddPoint(fx, fy);
                    needsRefresh = true;
                    break;

                case MotionEventActions.Up:
                    _drawer.StopDrawing(fx, fy);
                    needsRefresh = true;
                    break;
            }

            if (needsRefresh)
            {
                _canvas.Invalidate();
            }
        }

        // use the back button in preference to trying to detect shake, which is not built in
        public override void OnBackPressed()
        {
            _drawer.ErasePaths();
            _canvas.Invalidate();
        }

        private void EditCredentials()
        {
            var dialog = new LoginDialog();
            dialog.OnCloseLogin = (bool changedServer) =>
            {
                if (changedServer || _drawer == null)
                {
                    if (DrawXSettingsManager.HasCredentials())
                    {
                        SetupDrawer();  // pointless unless contact server
                        _drawer.LoginToServerAsync();
                    }
                    //// TODO allow user to launch locally if server not available
                }

                _canvas.Invalidate();
            };

            dialog.Show(FragmentManager, "login");
        }
    }
}
