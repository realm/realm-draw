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
using Android.App;
using Android.OS;
using Android.Views;
using DrawXShared;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Realms.Sync;
using SkiaSharp.Views.Android;

namespace DrawX.Droid
{
    [Activity(Label = "DrawX", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : Activity
    {
        private RealmDraw _drawer;
        private SKCanvasView _canvas;
        private bool _isEditingCredentials;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            DrawXSettingsManager.InitLocalSettings();

            _canvas = FindViewById<SKCanvasView>(Resource.Id.canvas);
            _canvas.ViewTreeObserver.GlobalLayout += OnViewRendered;
        }

        private Task<bool> SetupDrawer(Func<Task<User>> getUserFunc)
        {
            if (_drawer != null)
            {
                _drawer.Dispose();
                _canvas.PaintSurface -= OnPaintSample;
                _canvas.Touch -= OnTouch;
            }

            _canvas.PaintSurface += OnPaintSample;
            _canvas.Touch += OnTouch;

            _drawer = new RealmDraw(_canvas.CanvasSize.Width, _canvas.CanvasSize.Height);
            _drawer.CredentialsEditor = () =>
            {
                RunOnUiThread(EditCredentials);
            };

            _drawer.RefreshOnRealmUpdate = _canvas.Invalidate;

            _drawer.ReportError = (bool isError, string msg) =>
            {
                var tcs = new TaskCompletionSource<object>();

                RunOnUiThread(() =>
                {
                    new AlertDialog.Builder(this)
                                   .SetTitle(isError ? "Realm Error" : "Warning")
                                   .SetMessage(msg)
                                   .SetPositiveButton("OK", (senderAlert, args) =>
                                   {
                                       tcs.TrySetResult(null);
                                   })
                                   .Show();
                });

                return tcs.Task;
            };

            return _drawer.LoginUserAsync(getUserFunc);
        }

        private async void OnViewRendered(object sender, EventArgs e)
        {
            _canvas.ViewTreeObserver.GlobalLayout -= OnViewRendered;

            var user = User.Current;
            if (user != null)
            {
                await SetupDrawer(() => Task.FromResult(user));
            }

            if (_drawer?.Realm == null)
            {
                EditCredentials();
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

            var fx = touchEventArgs.Event.GetX();
            var fy = touchEventArgs.Event.GetY();
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
            if (_drawer.Realm != null)
            {
                _drawer.ErasePaths();
                _canvas.Invalidate();
            }
        }

        private void EditCredentials()
        {
            if (_isEditingCredentials)
            {
                return;
            }

            _isEditingCredentials = true;

            var dialog = new LoginDialog(this);
            dialog.PerformLoginAsync = async (credentials) =>
            {
                var success = await SetupDrawer(() => User.LoginAsync(credentials, new Uri($"http://{DrawXSettingsManager.Settings.ServerIP}")));

                if (success)
                {
                    dialog.Dismiss();
                    _isEditingCredentials = false;
                    _canvas.Invalidate();
                }
            };

            dialog.Cancelable = false;

            dialog.Show(FragmentManager, "login");
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Android.Content.Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            AuthenticationAgentContinuationHelper.SetAuthenticationAgentContinuationEventArgs(requestCode, resultCode, data);
        }
    }
}
