# RockSnifferGui
Simple GUI to display RockSniffer data instead of having to read and display the text files from vanilla RockSniffer.

## Overview
So [Rocksmith](https://rocksmith.ubisoft.com/rocksmith/en-us/home/) is excellent, and [RockSniffer](https://github.com/kokolihapihvi/RockSniffer) is an excellent add-on to get data out of the game for whatever reason you might want to. If you've been using RockSniffer, there's a decent chance you're a streamer trying to show an overlay to your viewers. Well, this is for the non-streamers out there (not exclusively or anything. Streamers should feel free to use this if it works for your visual setup.) This is a simple Windows application that will display the main song details and statistics on-screen, updating automatically, just like the RockSniffer output text files do, but in graphical form.

## Download
I don't know why Github makes this difficult, but to download, you have to go to the "[Releases](https://github.com/hunterpankey/RockSnifferGui/releases)" section, then expand the "Assets" section, and then finally download the "RockSnifferGui-0.0.2.zip" file to download. Or...

### [Download RockSnifferGui](https://github.com/hunterpankey/RockSnifferGui/releases/download/0.0.2/RockSnifferGui-0.0.2.zip)

Extract it into its own folder and run RockSnifferGui.exe to launch it. Launch the Rocksmith game first, though. It'll complain about it if you don't.

## Drawbacks/Limitations
This doesn't do any of the text files and album cover image file output that you're used to with RockSniffer. All it does is display the info in its little window. Also, once you close the program, that data's gone. It'll keep all the cached information it knows about your CDLCs and stuff like that. It just won't keep a record of your performance from session to session.

Also, there's not really much to configure yet. Actually, there's nothing to configure, as far as I can see. That will likely be forthcoming, but it's sure not in there now.

## To Do
1. I started on tracking song statistics over the long term. I think it would be cool to look at a graph of every time you've played that song to see how you did from instance to instance. Maybe you'd see that steady improvement that Ubi advertises. But that's nowhere near complete yet.
2. Move stats information to a prettier overlay that's suitable for dropping in your OBS (or whatever) setup and showing to viewers.

## To Be Clear
I'm not affiliated with the RockSniffer project and don't really know anyone associated with it (yet?) I just wanted to have something I could use for myself to look at simple statistics and thought some other folks might like it, too.
