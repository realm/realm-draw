# Realm Draw Demo

Realm Draw is a simple drawing app designed to show off the collaborative features of the [Realm Mobile Platform](https://realm.io/news/introducing-realm-mobile-platform/).

Any number of users may draw on a single shared canvas in any given moment, with contributions from other devices appearing on the canvas in real-time.

This version is the iOS version, as demonstrated in the Realm Mobile Platform demonstration video

## Installation Instructions

1. [Download the Realm Mobile Platform](https://realm.io/docs/realm-mobile-platform/get-started/) Developer Edition.
2. Run a local instance of the Realm Mobile Platform.
3. Create a new user, with the email `demo@realm.io` and the password `password`.
4. Open the Android Project located at `realm-draw/RealmDraw` with Android Studio, and build the Draw app and deploy it to an Android device.
5. When Realm Draw starts you will be automatically be logged in as demo@realm.io and be able to start drawing. The Realm Object server address you enter can be local or it can be an instance running on any of our other supported Linux platforms which may also be downloaded from [Realm](https://realm.io). In either case you should ensure your firewall allows access to ports 9080 and 27800 as these are needed by the application in order to communicate wth the Realm Object Server.


## Screenshots

![Nexus 6P](AndroidPhoneScreenshot.png?raw=true "Nexus 6p")

Nexus 6P Phone


![Pixel C](AndroidTabletScreenshot.png?raw=true "Pixel C")

Pixel C Tablet
