# (Unofficial) RockSnifferGui v0.4.0 [[Download](https://github.com/hunterpankey/RockSnifferGui/releases/download/0.4.0/RockSnifferGui-0.4.0.zip)]
An easy to use application to display your Rocksmith data.

## Overview
So [Rocksmith](https://rocksmith.ubisoft.com/rocksmith/en-us/home/) is excellent, and [RockSniffer](https://github.com/kokolihapihvi/RockSniffer) is an excellent add-on to get data out of the game for whatever reason you might want to. If you've been using RockSniffer, there's a decent chance you're a streamer trying to show an overlay to your viewers. Well, this is for the non-streamers out there. (Not exclusively or anything. Streamers should feel free to use this if it works for your visual setup.) This is a simple Windows application that will display the main song details and statistics on-screen, updating automatically, just like the RockSniffer output text files do, but in graphical form.

### Main Window
The main window displays the current song being played, with album art, artist, song title, album, and release year laid out approximately how the game, or a music application would disply it. It also includes the live note data from the song being played, with a time progress bar, red/green display for total notes hit and missed, the current hit/miss streak, the highest streak so far, and the overall accuracy.

<img src="https://github.com/hunterpankey/RockSnifferGui/blob/master/Screenshots/0.2.0-main%20window.png" width=500 alt="Unofficial RockSniffer GUI - Main Screen - Showing Radiohead - Paranoid Android playing" />

### Play History Window
URSG tracks statistics for every song you play! You can view basic information for every time you've played a song by clicking the "Play History" button on the main screen. This updates live every time a song is finished, so it's possible to leave it open during a play session and keep an eye on everything. Column headings are clickable to sort by that column.

<img src="https://github.com/hunterpankey/RockSnifferGui/blob/master/Screenshots/0.2.0-play%20history.png" width=800 alt="Unofficial RockSniffer GUI - Play History Screen - Showing stats for several songs played" />

New in v0.4.0, you can double click on a played song to show just that song, so you can see your stats for that song and see your improvement! Hit escape, backspace, Alt+left, or click the back button in the toolbar above to return to the "all songs" view.

<img src="https://github.com/hunterpankey/RockSnifferGui/blob/master/Screenshots/0.4.0-play%20history%20song%20details.png" width=800 alt="Unofficial RockSniffer GUI - Play History Screen - Showing stats for one selected song" />

### Overlay
Click the Toggle Overlay button on the main window toolbar to bring up the overlay window. This is a borderless/chromeless window suitable for showing on top of the game window for heads-up, live data. There's a nice glow effect around the window so it's not just a bare rectangle on screen. It uses the same displays as the main window, only side-by-side so it fits in a corner without covering a ton of the game window. Obviously, if you want to capture that in OBS or whatever and scale it up or down, feel free. (If that's possible. I know nothing about OBS, so you're on your own, Al Capone.)

~~URSG doesn't yet remember the window locations between executions, so you'll have to move it in place each time it's shown. That's obviously a necessary feature, but, one step at a time, right?~~ This is updated for v0.4.0. URSG will now remember where the main window and overlay were from last time and put them there again. To move the window, just click and drag anywhere in the overlay and drop it wherever you want it.

I initially had to set Rocksmith to windowed mode, but it seems to work in full screen mode with this build. Still, if it doesn't want to stay on top of the game window, hit F11 (on Windows, at least), or go into the game options' graphics settings and set it to windowed mode.

<img src="https://github.com/hunterpankey/RockSnifferGui/blob/master/Screenshots/0.3.0-overlay-cake-720.png" width=800 alt="Unofficial RockSniffer GUI - Overlay Screen - Showing live play of a Cake song" />

### Song Accuracy Graphs
Thanks to the fine folks at [Live Charts](https://lvcharts.net/), URSG now has charts to show you graphically how you're doing on any song you play. Click the Song Graphs button on the main window or select it from the view menu to show the graphs window. Check or uncheck the boxes in the legend to the right to show or hide individual songs. Use the "Select all" and "Select none" buttons above to control the view!

The values displayed are scaled to the total number of notes in your plays of the songs, so if you did really well on an easier difficulty of the song sections, but a little worse on a harder difficulty, you'll still see improvement because the graph rates more difficult plays higher than easier ones. The values probably don't match the song mastery percentage in-game, but it's still a decent way to see your progress.

<img src="https://github.com/hunterpankey/RockSnifferGui/blob/master/Screenshots/0.4.0-accuracy graph.png" width=800 alt="Unofficial RockSniffer GUI - Song Accuracy Graph screen - Showing accuracy stats for three selected songs" />

### Song Sections
The main and overlay windows now show a list of song sections by name along with the current section highlighted in the center. The display automatically updates as you play the song, moving the previous section off to the left and moving the new section in from the right.

<img src="https://github.com/hunterpankey/RockSnifferGui/blob/master/Screenshots/0.4.0-song%20sections.png" width=800 alt="Unofficial RockSniffer GUI - Overlay Screen - Showing current active section during song play" />

## Download
I don't know why Github makes this difficult, but to download, you have to go to the "[Releases](https://github.com/hunterpankey/RockSnifferGui/releases)" section, then expand the "Assets" section, and then finally download the "RockSnifferGui-0.4.0.zip" file to download. Or...

### [Download (Unofficial) RockSniffer GUI v0.4.0](https://github.com/hunterpankey/RockSnifferGui/releases/download/0.4.0/RockSnifferGui-0.4.0.zip)

Extract it into its own folder and run RockSnifferGui.exe to launch it. It will scan for any extra songs you might have and build the initial database of tracks. It will also create default configuration files and initialize the play history database to track song play instances. All files will be stored in the same folder as the application.

## Drawbacks/Limitations
This doesn't do any of the text files and album cover image file output that you're used to with RockSniffer. All it does is display the info in its little windows. It'll keep all the cached information it knows about your CDLCs and stuff like that as well.

Also, there's not really much to configure yet. Actually, there's nothing to configure, as far as I can see. That will likely be forthcoming, but it's sure not in there now.

## To Do
1. I started on tracking song statistics over the long term. ~~I think it would be cool to look at a graph of every time you've played that song to see how you did from instance to instance. Maybe you'd see that steady improvement that Ubi advertises. But that's nowhere near complete yet.~~ This is new for version 0.4.0. Click the "Song Graphs" button on the main window toolbar to check it out!
2. Configurability. It would be nice to have some color configuration and be able to save window locations so it opens back up in the same place it was closed from.

## To Be Clear
I'm not affiliated with the RockSniffer project and don't really know anyone associated with it (yet?) I just wanted to have something I could use for myself to look at simple statistics and thought some other folks might like it, too.
