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
using Realms.Sync;
using UIKit;

namespace DrawX.IOS
{
    // TODO enable Login button only if enter text in all three
    // TODO handle return key to move between fields and trigger launch (and change storyboard settings on fields if do so)
    public partial class LoginViewController : UIViewController
    {
        public Func<Credentials, Task> PerformLoginAsync { get; set; }  // caller should set so can use to dismiss

        public LoginViewController(IntPtr handle) : base(handle)
        {
        }

        private async void DoLogin()
        {
            LoginButton.Enabled = false;
            try
            {
                DrawXSettingsManager.Write(() =>
                {
                    DrawXSettingsManager.Settings.ServerIP = ServerEntry.Text;
                    DrawXSettingsManager.Settings.Username = UsernameEntry.Text;
                });

                var credentials = Credentials.UsernamePassword(UsernameEntry.Text, PasswordEntry.Text, false);

                await PerformLoginAsync(credentials);
            }
            finally
            {
                LoginButton.Enabled = true;
            }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            ServerEntry.Text = DrawXSettingsManager.Settings.ServerIP;
            UsernameEntry.Text = DrawXSettingsManager.Settings.Username;

            LoginButton.TouchUpInside += (sender, e) =>
            {
                DoLogin();
            };

            // you can only cancel logging in if already logged in, otherwise it is meaningless
            CancelButton.Enabled = (DrawXSettingsManager.LoggedInUser != null);
                
            CancelButton.TouchUpInside += (sender, e) =>
            {
                PerformLoginAsync(null);
            };

            #region Return key behaviour on keyboard - Next unti last field then Go
            ServerEntry.ShouldReturn += (textField) =>
            {
                UsernameEntry.BecomeFirstResponder();
                return false;  // no linebreak insertion
            };

            UsernameEntry.ShouldReturn += (textField) =>
            {
                PasswordEntry.BecomeFirstResponder();
                return false;
            };

            PasswordEntry.ShouldReturn += (textField) =>
            {
                PasswordEntry.ResignFirstResponder();
                DoLogin();
                return false;
            };
            #endregion
        }

    }
}