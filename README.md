# Realm Draw
> A Collaborative Drawing App, where multiple users can draw to a shared canvas at the same time.

![Screenshot](screenshot.jpg)

[![CI Status](http://img.shields.io/travis/realm-demos/realm-draw.svg?style=flat)](http://api.travis-ci.org/realm-demos/realm-draw.svg)
[![GitHub license](https://img.shields.io/badge/license-Apache-blue.svg)](https://raw.githubusercontent.com/realm/realm-draw/LICENSE)

The demo application seen in the Realm Platform [launch video](https://realm.io/news/introducing-realm-mobile-platform/), Realm Draw is a real-time collaborative drawing program available for Android, iOS and Xamarin. Any number of users may draw on a single shared canvas at any given moment with the strokes appearing on all devices in real time.

# Features

- [x] Allows multiple users to contribute drawings to a shared canvas in real-time.
- [x] Canvas can be reset by shaking the device briefly.
- [x] Can connect to any running instance of Realm Object Server.

# Requirements

### iOS
- iOS 9.0 and above
- Xcode 8.3

### Android
- Android 4.0.3

### Xamarin
- Android 4.0.3
- iOS 9.3

### ROS
- Realm Platform / Realm Object Server 2.0 and above


### Realm Platform

This application demonstrates features of the [Realm Platform](https://realm.io/products/realm-platform/) and needs to have a working instance of the Realm Object Server version 2.x to make data available between instances of the Draw app. The Realm Object Server can be installed via npm as a node application for macOS or Linux. Please see the [installation instructions](https://realm.io/docs/get-started/installation/developer-edition/). If you already got Node.js installed, it's a one-liner.


### Realm Studio
Another useful tool is [Realm Studio](https://realm.io/products/realm-studio/) which is available for macOS, Linux, Windows and allows developers to inspect and manage Realms and the Realm Object Server. Realm Studio is recommended for all developers and can be downloaded from the [Realm web site](https://realm.io/products/realm-studio/).


# Building and Running

Follow the README files in the platform-specific subdirectory of this repository:

* [Android](Android)
* [iOS](Apple)
* [Xamarin](Xamarin)


# Connecting to the Realm Object Server
When launching the app each time, you'll be presented with a login form in order to connect to the right Realm Object Server instance.

For the server URL field, you may simply enter `localhost` if you're running out of the iOS Simulator with the Realm Object Server at the same time. For iOS devices running on the same network as that Mac, you can alt-click on the Wi-Fi icon in the macOS status bar to get the Mac's local IP address. You can then manually enter this into the server URL field on the iOS device.

You'll be required to create an admin account the first time you run an instance of the Realm Object Server. You may use this account when logging into Realm Draw by entering in the same username/password pair. Alternatively, you may also register new user accounts from the form by tapping the 'Register a new account' button.

# Known Issues

The Android and iOS versions of the app will inter-operate; the Xamarin version currently can only share drawings with other copies of itself (compiled for either iOS or Android). This is due to a difference in the way the Xamarin platform handles its drawing canvas compared to the native iOS and Android code bases.

# Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for more details!

This project adheres to the [MongoDB Code of Conduct](https://www.mongodb.com/community-code-of-conduct).
By participating, you are expected to uphold this code. Please report
unacceptable behavior to [community-conduct@mongodb.com](mailto:community-conduct@mongodb.com).

# License

Distributed under the Apache license. See ``LICENSE`` for more information.

<img style="width: 0px; height: 0px;" src="https://3eaz4mshcd.execute-api.us-east-1.amazonaws.com/prod?s=https://github.com/realm/realm-draw#README.md (deprecated)">
