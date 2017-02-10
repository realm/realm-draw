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
using System.Threading.Tasks;
using DrawXShared;
using Foundation;
using Realms.Sync;
using Realms.Sync.Exceptions;
using SkiaSharp.Views.iOS;
using UIKit;

namespace DrawX.IOS
{
    // Most of the ViewController logic is factored out here so we can subclass
    // with a local copy in case we want a debug build which bypasses nuget as a Realm source
    public class ViewControllerShared : UIViewController
    {
        private RealmDraw _drawer;
        private CoreGraphics.CGRect _prevBounds;
        private float _devicePixelMul;  // usually 2.0 except on weird iPhone 6+

        public ViewControllerShared(IntPtr handle) : base(handle)
        {
            _devicePixelMul = (float)UIScreen.MainScreen.Scale;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            DrawXSettingsManager.InitLocalSettings();
            var user = User.Current;
            if (user != null)
            {
                SetupDrawer(() => Task.FromResult(user));
            }
        }

        private Task<bool> SetupDrawer(Func<Task<User>> getUserFunc)
        {
            _drawer?.Dispose();

            // scale bounds to match the pixel dimensions of the SkiaSurface
            _drawer = new RealmDraw(
                _devicePixelMul * (float)View.Bounds.Width,
                _devicePixelMul * (float)View.Bounds.Height);
            _prevBounds = View.Bounds;
            _drawer.CredentialsEditor = () =>
            {
                InvokeOnMainThread(EditCredentials);
            };

            _drawer.RefreshOnRealmUpdate = () =>
            {
                View?.SetNeedsDisplay();  // just refresh on notification, OnPaintSample below triggers DrawTouches
            };

            _drawer.ReportError = (bool isError, string msg) =>
            {
                var tcs = new TaskCompletionSource<object>();
                var alertController = UIAlertController.Create(isError ? "Realm Error" : "Warning", msg, UIAlertControllerStyle.Alert);
                alertController.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, _ => tcs.TrySetResult(null)));
                (PresentedViewController ?? this).PresentViewController(alertController, true, null);
                return tcs.Task;
            };

            return _drawer.LoginUserAsync(getUserFunc);
        }

        public async override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            var user = User.Current;
            if (View.Bounds != _prevBounds && user != null)
            {
                await SetupDrawer(() => Task.FromResult(user));
            }
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            if (_drawer?.Realm == null)
            {
                EditCredentials();
            }
        }

        protected void OnPaintSample(object sender, SKPaintSurfaceEventArgs e)
        {
            _drawer?.DrawTouches(e.Surface.Canvas);
        }

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            base.TouchesBegan(touches, evt);
            var touch = touches.AnyObject as UITouch;
            if (touch != null)
            {
                var point = touch.LocationInView(View);
                _drawer?.StartDrawing((float)point.X * _devicePixelMul, (float)point.Y * _devicePixelMul);
                View.SetNeedsDisplay();
            }
        }

        public override void TouchesMoved(NSSet touches, UIEvent evt)
        {
            base.TouchesMoved(touches, evt);
            var touch = touches.AnyObject as UITouch;
            if (touch != null)
            {
                var point = touch.LocationInView(View);
                _drawer?.AddPoint((float)point.X * _devicePixelMul, (float)point.Y * _devicePixelMul);
                View.SetNeedsDisplay();
            }
        }

        public override void TouchesCancelled(NSSet touches, UIEvent evt)
        {
            base.TouchesCancelled(touches, evt);
            var touch = touches.AnyObject as UITouch;
            if (touch != null)
            {
                _drawer?.CancelDrawing();
            }
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);
            var touch = touches.AnyObject as UITouch;
            if (touch != null)
            {
                var point = touch.LocationInView(View);
                _drawer?.StopDrawing((float)point.X * _devicePixelMul, (float)point.Y * _devicePixelMul);
            }

            View.SetNeedsDisplay();
        }

        public override void MotionBegan(UIEventSubtype motion, UIEvent evt)
        {
            if (motion == UIEventSubtype.MotionShake)
            {
                var alert = UIAlertController.Create(
                    "Erase Canvas?",
                    "This will clear the shared Realm database and erase the canvas. Are you sure you wish to proceed?",
                    UIAlertControllerStyle.Alert);
                
                // unlike other gesture actions, don't call View.SetNeedsDisplay but let major Realm change prompt redisplay
                alert.AddAction(UIAlertAction.Create("Erase", UIAlertActionStyle.Destructive, action => _drawer?.ErasePaths()));
                alert.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));
                if (alert.PopoverPresentationController != null)
                {
                    alert.PopoverPresentationController.SourceView = View;
                }

                PresentViewController(alert, animated: true, completionHandler: null);
            }
        }

        // invoked as callback from pressing a control area in drawing surface, or at startup
        private void EditCredentials()
        {
            var sb = UIStoryboard.FromName("LoginScreen", null);
            var loginVC = sb.InstantiateViewController("Login") as LoginViewController;
            loginVC.PerformLoginAsync = async (credentials) =>
            {
                var success = await SetupDrawer(() => User.LoginAsync(credentials, new Uri($"http://{DrawXSettingsManager.Settings.ServerIP}")));

                if (success)
                {
                    loginVC.DismissViewController(true, null);
                }
            };

            PresentViewController(loginVC, true, null);
        }
    }
}
