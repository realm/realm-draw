# Realm Draw Demo

Realm Draw is a simple drawing app designed to show off the collaborative features of the [Realm Mobile Platform](https://realm.io/news/introducing-realm-mobile-platform/).

Any number of users may draw on a single shared canvas in any given moment, with contributions from other devices appearing on the canvas in real-time.

This version is the iOS version, as demonstrated in the Realm Mobile Platform demonstration video

## Installation Instructions

1. [Download the macOS version](https://realm.io/docs/realm-mobile-platform/get-started/) of the Realm Mobile Platform.
2. Run a local instance of the Realm Mobile Platform.
3. Create a new user, with the email `demo@realm.io` and the password `password`.
4. Update/download the required Cocoapods with `pod update` (Cocoapods installation instructions may be found on the [Cocoapods site](https://cocoapods.org))
5. Open the `RealmDraw.xcworkspace` with Xcode, and build the Draw app and deploy it to iOS device.
6. When Realm Draw starts you will be prompted to enter the server name/address, a username and password. The Realm Object server address you enter can be local (if for example, you downloaded the macOS version in Step 1, above), or it can be an instance running on any of our other supported Linux platforms which may also be downloaded from [Realm](https://realm.io).  In either case you should ensure your firewall allows access to ports 9080 and 27800 as these are needed by the application in order to communicate wth the Realm Object Server. 
