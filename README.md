# NRPFarMod - Welcome!

Below you will find Information about the Night Runners Prologue Farliam Mod (NRPFarMod). Happy listening!
[NRPFarMod YouTube Video](https://youtu.be/BjadvGEYdkc)

# Quick Start - How to Install & Use
In this Section you will learn how to Install the Mod and how to use it.

An Installation YouTube Tutorial will follow.
## Install
To Install, you need to have [MelonLoader](https://melonloader.xyz) Installed on your NIGHT-RUNNERS Instance!

1: Install NIGHT-RUNNERS: Prologue (Either from Steam or Itch.io)

2: Install MelonLoader to your NIGHT-RUNNERS instance. Run the MelonLoader installer and select the NIGHT-RUNNERS executable (.exe) file.

3: Start NIGHT-RUNNERS and verify that the game can start normally (attempt to go into your garage at least).

4: Close NIGHT-RUNNERS

5: Download the latest NRPFarMod.dll File. You can find it in this Repositories [Releases](https://github.com/iLollek/NRPFarMod/releases) Tab.

6: MelonLoader will create several new folders in your NIGHT-RUNNERS game folder. Locate the "Mods" folder.

7: Copy or Move the "NRPFarMod.dll" File into the "Mods" Folder.

8: Inside the "Mods" Folder, there will be a new folder named "src"

9: Copy or Move your Music Files - They have to be the WAVE Audioformat (.wav) - Into the "src" Folder.

10: Start NIGHT-RUNNERS. If everything worked correctly, you should see green text in the top right corner of your screen in NIGHT-RUNNERS.

## Usage
Currently, the mod supports two main ways of loading your own music:
- Overwrite Mode (directly overwrites the currently playing music with one that you imported and selected)
- Playlist Mode (starts a thread that checks if the song changes. If a song change is detected, it has a small chance of playing something from your own songs, giving the illusion that it's added to the in-game "playlist")

### Overwrite Mode 
When you press INSERT on your keyboard, a small menu on the top left will open up. This menu shows the currently selected track and the hotkeys, `O` and `P`. You can slide the slider with your mouse to select a song, then press Play to immediately overwrite the song that is currently playing in-game with the selected one. Important: There has to be a song already playing for this to work correctly.
You can also immediately call this overwrite without having to open the GUI of the mod! For example, while driving, you can easily change your song by using the O/P hotkeys.
**Please note:** Overwrite Mode will be disabled if Playlist Mode is enabled.

### Playlist Mode
Playlist Mode works differently. It's designed to give the illusion that the songs are added to the game's playlist. To start the PlaylistWatcher thread, press `M` on your keyboard as soon as there is Music Playing In Game. In the MelonLoader log, it will now say something like this:

`[NRPFarMod] Started PlaylistSimulator WatcherThread.`

This means that now NRPFarMod is going to monitor the music in the game. As soon as it detects that a song has changed - either automatically, because it ended, or because the player used the phone to skip a song - there is a small chance (currently it's 40%) that NRPFarMod will, instead of playing the next normal song (from the game's OST), play one of your custom songs.

### Manually Removing unused Songs to free RAM

While the Playlist Simulator automatically takes care of Memory Management, you might want to free RAM manually. This is mandatory when you use **Overwrite Mode**.

You can press `N` on your Keyboard to manually execute the ``RemoveUnusedSongs()`` Method. This will remove unused, but loaded Songs from your RAM.

The game *might* freeze for half a second when this executes.

## Usage: How to import your Songs from YouTube & Spotify

It's a relatively common process: Download the Music from your Source (Spotify or YouTube) and then convert the output into WAVE (.wav) and then put that Music into your /src/ Folder.

### Spotify
Use a Tool to Download Spotify Music off of. For Playlists, you can use this: [Spotify Playlist Downloader Online](https://spotify-downloader.com/)

After that, convert your Files into WAVE (.wav). You can use [this](https://online-audio-converter.com/) Tool. Select CD Quality or DVD Quality, if available.

Finally, move your Downloaded .wav Files into the /src/ Directory where your Mod (NRPFarMod.dll) is located.

### YouTube
Use a Tool to Download YouTube Music off of. You can use [this](https://en1.savefrom.net/1-youtube-video-downloader-3vV/) Tool. (Hey! I also made a [YouTube Downloader in Python](https://ilollek.net/pyd-rem/download.html)!)

After that, convert your Files into WAVE (.wav). You can use [this](https://online-audio-converter.com/) Tool. Select CD Quality or DVD Quality, if available.

Finally, move your Downloaded .wav Files into the /src/ Directory where your Mod (NRPFarMod.dll) is located.

## Usage: Hotkeys

| Key  | Function |
| ------------- | ------------- |
| INSERT  | Open GUI (Developer) |
| O  | Previous Song (Overwrite Mode)  |
| P  | Next Song (Overwrite Mode)  |
| N  | Remove unused Songs from RAM  |
| M  | Start Playlist Mode  |

# Bugs & Issues
Found something that doesn't work? Please verify that your Issue isn't already known (In the "Known Issues" section) and that is has not been reported in the [Issues Tab](https://github.com/iLollek/NRPFarMod/issues) already.
## Reporting a Bug
First, verify if you're on the newest Release of NRPFarMod. Maybe your Issue has been fixed already.

If something doesn't work, please [create a Issue](https://github.com/iLollek/NRPFarMod/issues/new). Give a Detailed Description of your Issue and if possible, always provide Screenshots. This helps us fix your Issue faster.

## Known Issues
- The Text on the phone doesn't show, or doesn't show correctly. I can't change this without patching a method -- And I want this Mod to be minimally intrusive, so I have to wait for Jem to change how he loads ``phoneNowPlaying`` in a future Update.

# Support
If you want to Support the Development of the NRPFarMod, you can do so in many ways!

For example, by reporting Bugs or suggesting new Features, you can help this Mod become better.

By talking about the Mod on Reddit or any other Platform, to give it Publicity, so more people can use it and thus, maybe Support it.

Or, if you're able to Code, you can commit to this Repository and fix Issues with us!

You can leave a Star to this Repository or follow the Developers!

Lastly, you can also Donate Money to the Repository Owner by pressing the "Sponsor" Button at the top.

Thank you!

# Developing for this Mod

If you're thinking of supporting this Mod by Developing for it on this Repository, first up I have to say: Thank you!

There's certain guidelines and criteria I follow for this Mod, and I need to ask you to follow too if you make changes to this Mod that you want to be implemented via a Pull Request.

## Developer: Guidelines

- I want NRPFarMod to be "minimally intrusive" by Design. This means: Your Changes should make minimal, if None at all, changes to the original Code. This basically means things like: do not patch any methods, do not access, change or modify anything you don't find absolutely necessary to do to achieve your feature/bugfix, keep your code small, etc.

- Write clean Code. Your Code should be easily maintainable, extensible and readable.

- Resources are not infinite. We are not a AAA-Studio. Do good memory Management. If your added Code introduces **unecessary** framedrops or performance loss in any way, your Pull Request will be denied.

- Game first. We are the ones who are making Mods off of Jem's work, not the other way around. We have to wrap our heads around NIGHT-RUNNERS, not Jem around our Mods.

## Developer: Issues, Pull Requests etc.: Name Conventions

When you create a Issue or Pull Request (or anything that requires you to describe the Problem) there is usually a big naming convention and/or template for big repositories. I won't make one (for now), i hope that will work.
Please describe your Issue or what your added code does in a Pull request extensively and into detail. Provide Screenshots if possible.

## Developer: What to configurate before Building, Editing and Writing

Essentially, you just need to configure your System's paths to point to the files on your Computer.

You will find a few "wildcards" that can look like this: ``<path_to_nightrunners_instance>\Mods`` 

Just replace the Wildcard with what it wants. In this case, replace ``<path_to_nightrunners_instance>`` with a path pointing to your NIGHT-RUNNERS game's folder.

The PostCall.bat automatically copies your newly built modfile (.dll) to your Mods folder so you can save some time for not having to drag and drop it.

# Credits
[Farliam](https://github.com/Farliam93/) - Created the first Version of this Mod (1.0.0)

[iLollek](https://github.com/iLollek/) - NRPFarMod Developer & Repository Owner

[Jem Byrne (PLANET JEM Software)](https://linktr.ee/night_runners) - Developer of the Game that this Mod is developed for

*Using AssetUnlocker is piracy!*
