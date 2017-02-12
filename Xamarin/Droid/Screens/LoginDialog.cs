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
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using DrawXShared;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Realms.Sync;

namespace DrawX.Droid
{
    public class LoginDialog : DialogFragment, View.IOnClickListener
    {
        private EditText _serverAddressEntry;
        private EditText _usernameEntry;
        private EditText _passwordEntry;

        private Button _loginButton;
        private Button _adLoginButton;
        private Activity _parentActivity;

        public Func<Credentials, Task> PerformLoginAsync { get; set; }

        public LoginDialog(Activity parentActivity)
        {
            _parentActivity = parentActivity;
        }

        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            OnCreate(savedInstanceState);

            var inflator = Activity.LayoutInflater;
            var dialogView = inflator.Inflate(Resource.Layout.LoginLayout, null);

            if (dialogView == null)
            {
                return null;
            }

            _serverAddressEntry = dialogView.FindViewById<EditText>(Resource.Id.serverIPEntry);
            _usernameEntry = dialogView.FindViewById<EditText>(Resource.Id.usernameEntry);
            _passwordEntry = dialogView.FindViewById<EditText>(Resource.Id.passwordEntry);
            _serverAddressEntry.Text = DrawXSettingsManager.Settings.ServerIP;
            _usernameEntry.Text = DrawXSettingsManager.Settings.Username;

            return new AlertDialog.Builder(Activity)
                                  .SetView(dialogView)
                                  .SetPositiveButton("Login", (IDialogInterfaceOnClickListener)null)
                                  .SetNeutralButton("Login with AD", (IDialogInterfaceOnClickListener)null)
                                  .Create();
        }

        public override void OnStart()
        {
            base.OnStart();

            var alert = (AlertDialog)Dialog;
            _loginButton = alert.GetButton((int)DialogButtonType.Positive);
            _loginButton.SetOnClickListener(this);

            _adLoginButton = alert.GetButton((int)DialogButtonType.Neutral);
            _adLoginButton.SetOnClickListener(this);
        }

        public async void OnClick(View v)
        {
            v.Enabled = false;

            try
            {
                DrawXSettingsManager.Write(() =>
                {
                    DrawXSettingsManager.Settings.ServerIP = _serverAddressEntry.Text;
                    DrawXSettingsManager.Settings.Username = _usernameEntry.Text;
                });

                Credentials credentials = null;
                if (v == _loginButton)
                {
                    credentials = Credentials.UsernamePassword(_usernameEntry.Text, _passwordEntry.Text, false);
                }
                else if (v == _adLoginButton)
                {
                    // TODO: verify that server url has been input

                    var authContext = new AuthenticationContext(ADCredentials.CommonAuthority);
                    var response = await authContext.AcquireTokenAsync("https://graph.windows.net", ADCredentials.ClientId, ADCredentials.RedirectUri, new PlatformParameters(_parentActivity));

                    // TODO: uncomment when implemented
                    // var credentials = Credentials.ActiveDirectory(response.AccessToken);
                    credentials = Credentials.Debug();
                }

                await PerformLoginAsync(credentials);
            }
            catch (Exception ex)
            {
                // TODO: handle
            }
            finally
            {
                v.Enabled = true;
            }
        }
    }
}
