// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace DrawX.IOS
{
	[Register ("LoginViewController")]
	partial class LoginViewController
	{
		[Outlet]
		UIKit.UIButton ADLoginButton { get; set; }

		[Outlet]
		UIKit.UIButton LoginButton { get; set; }

		[Outlet]
		UIKit.UITextField PasswordEntry { get; set; }

		[Outlet]
		UIKit.UILabel PasswordLabel { get; set; }

		[Outlet]
		UIKit.UITextField ServerEntry { get; set; }

		[Outlet]
		UIKit.UILabel ServerLabel { get; set; }

		[Outlet]
		UIKit.UITextField UsernameEntry { get; set; }

		[Outlet]
		UIKit.UILabel UsernameLabel { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (LoginButton != null) {
				LoginButton.Dispose ();
				LoginButton = null;
			}

			if (PasswordEntry != null) {
				PasswordEntry.Dispose ();
				PasswordEntry = null;
			}

			if (PasswordLabel != null) {
				PasswordLabel.Dispose ();
				PasswordLabel = null;
			}

			if (ServerEntry != null) {
				ServerEntry.Dispose ();
				ServerEntry = null;
			}

			if (ServerLabel != null) {
				ServerLabel.Dispose ();
				ServerLabel = null;
			}

			if (UsernameEntry != null) {
				UsernameEntry.Dispose ();
				UsernameEntry = null;
			}

			if (UsernameLabel != null) {
				UsernameLabel.Dispose ();
				UsernameLabel = null;
			}

			if (ADLoginButton != null) {
				ADLoginButton.Dispose ();
				ADLoginButton = null;
			}
		}
	}
}
