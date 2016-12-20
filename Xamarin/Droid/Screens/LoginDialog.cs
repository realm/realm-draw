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
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using DrawXShared;

namespace DrawX.Droid
{
    public class LoginDialog : DialogFragment
    {
        public Action<bool> OnCloseLogin { get; set; }  // caller should set so can use to dismiss

        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            OnCreate(savedInstanceState);

            // equivalent of an Activity's SetContentView
            var builder = new AlertDialog.Builder(Activity);
            var inflator = Activity.LayoutInflater;
            var dialogView = inflator.Inflate(Resource.Layout.LoginLayout, null);

            if (dialogView == null)
            {
                return null;
            }

            var serverAddress = dialogView.FindViewById<EditText>(Resource.Id.serverIPEntry);
            var username = dialogView.FindViewById<EditText>(Resource.Id.usernameEntry);
            var password = dialogView.FindViewById<EditText>(Resource.Id.passwordEntry);
            var s = DrawXSettingsManager.Settings;
            serverAddress.Text = s.ServerIP;
            username.Text = s.Username;
            password.Text = s.Password;

            builder.SetView(dialogView);
            builder.SetPositiveButton("Login", (sender, e) =>
            {
                bool changedServer = DrawXSettingsManager.UpdateCredentials(serverAddress.Text, username.Text, password.Text);
                ((AlertDialog)sender).Dismiss();
                OnCloseLogin(changedServer);
            });
            builder.SetNegativeButton("Cancel", (sender, e) =>
            {
                ((AlertDialog)sender).Dismiss();
            });
            return builder.Create();
        }
    }
}
