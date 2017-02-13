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
using System.Linq;
using System.Threading.Tasks;
using DrawXShared;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Realms.Sync;
using UIKit;

namespace DrawX.IOS
{
    // TODO enable Login button only if enter text in all three
    public partial class LoginViewController : UIViewController
    {
        public Func<Credentials, Task> PerformLoginAsync { get; set; }  // caller should set so can use to dismiss

        public LoginViewController(IntPtr handle) : base(handle)
        {
        }

        private void LoginWithPassword()
        {
            LoginCore(LoginButton, () => Task.FromResult(Credentials.UsernamePassword(UsernameEntry.Text, PasswordEntry.Text, false)));
        }

        private void LoginWithAD()
        {
            LoginCore(ADLoginButton, async () =>
            {
                var clientId = ADCredentials.ClientId;
                if (clientId == "your-client-id")
                {
                    throw new Exception("Please update ADCredentials.ClientId with the correct ClientId of your application.");
                }

                var redirectUri = ADCredentials.RedirectUri;
                if (redirectUri.AbsolutePath == "http://your-redirect-uri")
                {
                    throw new Exception("Please update ADCredentials.RedirectUri with the correct RedirectUri of your application.");
                }

                var authContext = new AuthenticationContext(ADCredentials.CommonAuthority);
                var response = await authContext.AcquireTokenAsync("https://graph.windows.net", ADCredentials.ClientId, ADCredentials.RedirectUri, new PlatformParameters(this));

                // TODO: uncomment when implemented
                // var credentials = Credentials.ActiveDirectory(response.AccessToken);
                return Credentials.Debug();
            });
        }

        private async Task LoginCore(UIButton sender, Func<Task<Credentials>> getCredentialsFunc)
        {
            sender.Enabled = false;
            try
            {
                DrawXSettingsManager.Write(() =>
                {
                    DrawXSettingsManager.Settings.ServerIP = ServerEntry.Text;
                    DrawXSettingsManager.Settings.Username = UsernameEntry.Text;
                });

                var credentials = await getCredentialsFunc();

                await PerformLoginAsync(credentials);
            }
            catch (Exception ex)
            {
                var tcs = new TaskCompletionSource<object>();
                var alertController = UIAlertController.Create("Unable to login", ex.Message, UIAlertControllerStyle.Alert);
                alertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, _ => tcs.TrySetResult(null)));
                PresentViewController(alertController, animated: true, completionHandler: null);
                await tcs.Task;
            }
            finally
            {
                sender.Enabled = true;
            }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            ServerEntry.Text = DrawXSettingsManager.Settings.ServerIP;
            UsernameEntry.Text = DrawXSettingsManager.Settings.Username;

            LoginButton.TouchUpInside += (sender, e) => LoginWithPassword();
            ADLoginButton.TouchUpInside += (sender, e) => LoginWithAD();

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
                LoginWithPassword();
                return false;
            };

            #endregion
        }

    }
}