# NRPFarMod - Welcome!

Below you will find Information about the NRPFarMod. Happy listening!

# Quick Start - How to Install & Use
In this Section you will learn how to Install the Mod and how to use it.
## Install
To Install, you need to have [MelonLoader](https://melonloader.xyz) Installed on your NIGHT-RUNNERS Instance!

1: Install NIGHT-RUNNERS: Prologue (Either from Steam or Itch.io)
2: Install MelonLoader to your NIGHT-RUNNERS instance. Run the MelonLoader installer and select the NIGHT-RUNNERS executable (.exe) file.
3: Start NIGHT-RUNNERS and verify that the game can start normally (attempt to go into your garage at least).
4: Close NIGHT-RUNNERS
5: Download the latest NRPFarMod.dll File. You can find it in this Repositories [Releases](https://github.com/iLollek/NRPFarMod/releases) Tab.
6: MelonLoader will create several new folders in your NIGHT-RUNNERS game folder. Locate the "Mods" folder.
7: Copy or Move the "NRPFarMod.dll" File into the "Mods" Folder.
8: Inside the "Mods" Folder, create a Folder named "src" (Verify that the folder is named "src", like here, in all lowercase letters! This is important!)
9: Copy or Move your Music Files - They have to be the WAVE Audioformat (.wav) - Into the "src" Folder.
10: Start NIGHT-RUNNERS. If everything worked correctly, you should see green text in the top right corner of your screen in NIGHT-RUNNERS.
## Usage
Currently, the mod supports two main ways of loading your own music:
- Overwrite Mode (directly overwrites the currently playing music with one that you imported and selected)
- Playlist Mode (starts a thread that checks if the song changes. If a song change is detected, it has a small chance of playing something from your own songs, giving the illusion that it's added to the in-game "playlist")

### Overwrite Mode 
When you press INSERT on your keyboard, a small menu on the top left will open up. This menu shows the currently selected track and the hotkeys, O and P. You can slide the slider with your mouse to select a song, then press Play to immediately overwrite the song that is currently playing in-game with the selected one. Important: There has to be a song already playing for this to work correctly.
You can also immediately call this overwrite without having to open the GUI of the mod! For example, while driving, you can easily change your song by using the O/P hotkeys.

### Playlist Mode
Playlist Mode works differently. It's designed to give the illusion that the songs are added to the game's playlist. To start the PlaylistWatcher thread, press "M" on your keyboard. In the MelonLoader log, it will now say something like this:
`[NRPFarMod] Started PlaylistWatcher Thread!`
This means that now NPRFarMod is going to monitor the music in the game. As soon as it detects that a song has changed - either automatically, because it ended, or because the player used the phone to skip a song - there is a small chance (currently it's 20%) that NRPFarMod will, instead of playing the next normal song (from the game's OST), play one of your custom songs.
