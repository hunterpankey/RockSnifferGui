# (Unofficial) RockSnifferGui v0.2.0 [[Download]](https://github.com/hunterpankey/RockSnifferGui/releases/download/0.2.0/RockSnifferGui-0.2.0.zip)
A simple application to display RockSniffer data instead of having to read and display the text files from vanilla RockSniffer.

## Overview
So [Rocksmith](https://rocksmith.ubisoft.com/rocksmith/en-us/home/) is excellent, and [RockSniffer](https://github.com/kokolihapihvi/RockSniffer) is an excellent add-on to get data out of the game for whatever reason you might want to. If you've been using RockSniffer, there's a decent chance you're a streamer trying to show an overlay to your viewers. Well, this is for the non-streamers out there. (Not exclusively or anything. Streamers should feel free to use this if it works for your visual setup.) This is a simple Windows application that will display the main song details and statistics on-screen, updating automatically, just like the RockSniffer output text files do, but in graphical form.

### Main Window
The main window displays the current song being played, with album art, artist, song title, album, and release year laid out approximately how the game, or a music application would disply it. It also includes the live note data from the song being played, with a time progress bar, red/green display for total notes hit and missed, the current hit/miss streak, the highest streak so far, and the overall accuracy.

<img src="https://github.com/hunterpankey/RockSnifferGui/blob/master/Screenshots/0.2.0-main%20window.png" width=500 alt="Unofficial RockSniffer GUI v0.2.0 Main Screen - Showing Radiohead - Paranoid Android playing" />

### Play History Window
URSG tracks statistics for every song you play! You can view basic information for every time you've played a song by clicking the "Play History" button on the main screen. This updates live every time a song is finished, so it's possible to leave it open during a play session and keep an eye on everything. Column headings are clickable to sort by that column.

<img src="https://github.com/hunterpankey/RockSnifferGui/blob/master/Screenshots/0.2.0-play%20history.png" width=800 alt="Unofficial RockSniffer GUI v0.2.0 Play History Screen - Showing stats for several songs played" />

## Download
I don't know why Github makes this difficult, but to download, you have to go to the "[Releases](https://github.com/hunterpankey/RockSnifferGui/releases)" section, then expand the "Assets" section, and then finally download the "RockSnifferGui-0.2.0.zip" file to download. Or...

### [Download (Unofficial) RockSniffer GUI v0.2.0](https://github.com/hunterpankey/RockSnifferGui/releases/download/0.2.0/RockSnifferGui-0.2.0.zip)

Extract it into its own folder and run RockSnifferGui.exe to launch it. It will scan for any extra songs you might have and build the initial database of tracks. It will also create default configuration files and initialize the play history database to track song play instances. All files will be stored in the same folder as the application.

## Drawbacks/Limitations
This doesn't do any of the text files and album cover image file output that you're used to with RockSniffer. All it does is display the info in its little window. It'll keep all the cached information it knows about your CDLCs and stuff like that as well.

Also, there's not really much to configure yet. Actually, there's nothing to configure, as far as I can see. That will likely be forthcoming, but it's sure not in there now.

## To Do
1. I started on tracking song statistics over the long term. I think it would be cool to look at a graph of every time you've played that song to see how you did from instance to instance. Maybe you'd see that steady improvement that Ubi advertises. But that's nowhere near complete yet.
2. Move stats information to a prettier overlay that's suitable for dropping in your OBS (or whatever) setup and showing to viewers. It has basically the right layout for overlay displaying at this point, but a proper overlay should hide the window chrome and ensure that it's small enough not to be intrusive.

## To Be Clear
I'm not affiliated with the RockSniffer project and don't really know anyone associated with it (yet?) I just wanted to have something I could use for myself to look at simple statistics and thought some other folks might like it, too.
