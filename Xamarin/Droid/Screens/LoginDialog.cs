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
using Realms.Sync;

namespace DrawX.Droid
{
    public class LoginDialog : DialogFragment, Android.Views.View.IOnClickListener
    {
        private EditText serverAddressEntry;
        private EditText usernameEntry;
        private EditText passwordEntry;

        public Func<Credentials, Task> PerformLoginAsync { get; set; }

        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            OnCreate(savedInstanceState);

            var inflator = Activity.LayoutInflater;
            var dialogView = inflator.Inflate(Resource.Layout.LoginLayout, null);

            if (dialogView == null)
            {
                return null;
            }

            serverAddressEntry = dialogView.FindViewById<EditText>(Resource.Id.serverIPEntry);
            usernameEntry = dialogView.FindViewById<EditText>(Resource.Id.usernameEntry);
            passwordEntry = dialogView.FindViewById<EditText>(Resource.Id.passwordEntry);
            serverAddressEntry.Text = DrawXSettingsManager.Settings.ServerIP;
            usernameEntry.Text = DrawXSettingsManager.Settings.Username;

            return new AlertDialog.Builder(Activity)
                                  .SetView(dialogView)
                                  .SetPositiveButton("Login", (IDialogInterfaceOnClickListener)null)
                                  .Create();
        }

        public override void OnStart()
        {
            base.OnStart();

            var alert = (AlertDialog)Dialog;
            var button = alert.GetButton((int)DialogButtonType.Positive);
            button.SetOnClickListener(this);
        }

        public async void OnClick(View v)
        {
            v.Enabled = false;

            try
            {
                DrawXSettingsManager.Write(() =>
                {
                    DrawXSettingsManager.Settings.ServerIP = serverAddressEntry.Text;
                    DrawXSettingsManager.Settings.Username = usernameEntry.Text;
                });

                var credentials = Credentials.UsernamePassword(usernameEntry.Text, passwordEntry.Text, false);

                await PerformLoginAsync(credentials);
            }
            finally
            {
                v.Enabled = true;
            }
        }
    }
}
