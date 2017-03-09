# Realm-Draw

![Screenshot)(screenshot.jpg)

This is the demo application seen in the Realm Mobile Platform [launch video][1], a real-time collaborative drawing program available for Android, iOS and Xamarin. Any number of users may draw on a single shared canvas in any given moment.

## Prerequisites

The Realm-Draw demo application requires the Realm Mobile Platform Developer Edition. This can be downloaded free of charge for macOS and several popular Linux distributions, and takes just a few minutes to install.

Download links and installation instructions:

* [Realm Mobile Platform for macOS][2]
* [Realm Mobile Platform for Linux][3]

## Building and Running

Follow the README files in the platform-specific subdirectory of this repository.

* [Android][4]
* [iOS][5]
* [Xamarin][6]

# Known Issues

The Android and iOS versions of the app will interoperate; the Xamarin version currently can only share drawings with other copies of itself (compiled for either iOS or Android). This is due to a difference in the way the Xamarin platform handles its drawing canvas compared to the native iOS and Android code bases.


[1]: https://realm.io/news/introducing-realm-mobile-platform/
[2]: https://realm.io/docs/get-started/installation/mac/
[3]: https://realm.io/docs/get-started/installation/linux/
[4]: https://github.com/realm-demos/realm-draw/tree/master/Android
[5]: https://github.com/realm-demos/realm-draw/tree/master/Apple
[6]: https://github.com/realm-demos/realm-draw/tree/master/Xamarin
