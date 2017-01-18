# Realm-Draw[a]


This is the demo application seen in the Realm Mobile Platform [launch video][1][b][c][d][e], a real-time collaborative drawing program available for Android, iOS and Xamarin. Any number of users may draw on a single shared canvas in any given moment.


## Prerequisites


The Realm-Draw demo application requires the Realm Mobile Platform Developer Edition. This can be downloaded free of charge for macOS and several popular Linux distributions, and takes just a few minutes to install.


Download links and installation instructions:


* [Realm Mobile Platform for macOS][2]
* [Realm Mobile Platform for Linux][3]


## Building and Running


Follow the README files in the platform-specific subdirectory of this repository.


# Known Issues 


The Android and iOS versions of the app will interoperate; the Xamarin version currently can only share drawings with other copies of itself (compiled for either iOS or Android). This is due to a difference in the way the Xamarin platform handles its drawing canvas compared to the native iOS and Android code bases.


* [Android][4]
* [iOS][5]
* [Xamarin][6]


[1]: https://realm.io/news/introducing-realm-mobile-platform/
[2]: https://realm.io/docs/get-started/installation/mac/
[3]: https://realm.io/docs/get-started/installation/linux/
[4]: https://github.com/realm-demos/realm-draw/tree/master/Android
[5]: https://github.com/realm-demos/realm-draw/tree/master/Xamarin/iOS
[6]: https://github.com/realm-demos/realm-draw/tree/master/Xamarin[f][g][h][i][j]




[a]This is basically the same document, just expanded a little to have a description at the front. Right now it's using the links that are actually in the Github repo for Xamarin, iOS and Android, but that might not be correct?
[b]Is this notation mean that it's a link?
[c]No, this plain text -  Markdown supports links. IMO we should embed them or if we want to be very transparent make the link text at the end themselves markdown links (i.e.., ppl can see the links that they link to....)
[d]We can change the link styles, but I see this format a lot on Hacker News and in readme files in general -- this notation becomes a link in Markdown (a "reference style" link) and these will be links when the readme is viewed on Github itself, but in plain text they read like footnotes, and should be pretty easy to follow.
[e]Okay, I'm new to this kind of documentation, so will follow both of your leads on this, but wanted to ask the question.
[f]Do we want these are separate links? GitHub ReadMe will support full-blown markdown so the links can be in-line...
[g]This is Markdown, too! Github will turn "reference style" links into full links.
[h]cool - only remaining question is purely stylistic - do we want ppl to have to click twice or once?  (once to get from footnote to the link, then the  the links to the site) or embed the links?  I am not married to any way as long as we pick on -- product/marketing agree and then we use it going forward.
[i]When the README file is displayed by Github, it'll just be one click--the footnotes will go away and the links will be actual links. You'll only see them as footnotes when you're looking at this file as plain text. It's pretty cool. (Assuming that's what you want. :) )
[j]ah.... cool.. I didn't know that. (√ learned something new & useful today)