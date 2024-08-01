using Il2Cpp;
using Il2CppInterop.Runtime;
using Il2CppSystem.Threading.Tasks;
using MelonLoader;
using NRPFarmod;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityStandardAssets.Vehicles.Car;
using UnityEngine.Playables;
using static Il2Cpp.addressablesManager;
using System.Diagnostics;

//.\Mods\src\

[assembly: MelonInfo(typeof(NRPFarModLib), "NRPFarModLib", "1.0.2", "Farliam & iLollek")]
[assembly: MelonGame("PLANET JEM SOFTWARE", "NIGHT-RUNNERS PROLOGUE")]
[assembly: MelonGame("PLANET JEM SOFTWARE", "NIGHT-RUNNERS PROLOGUE PATREON")]

namespace NRPFarmod {

    #region Constructor / Initialization
    internal record AudioTupleData(string Title, string Path);

    public class NRPFarModLib : MelonMod {

        public Rect windowRect = new Rect(20, 20, 250, 110);
        private bool IsVisible = false;
        private bool KeyDownHandler = false;
        private GUIStyle infoFont = new();
        private GUIStyle hotkeyFont = new();

        private List<AudioTupleData> CustomSongs = new();
        private List<AudioClip> LoadedAudioClips = new();
        
        private int CurrentSelectedSong = 0;

        string currentsong = "No Music Playing";

        private bool isPlaylistModeEnabled = false;

        public override void OnInitializeMelon() {
            string path = Path.Combine(Environment.CurrentDirectory, @"Mods\src\");
            if(!Directory.Exists(path)) Directory.CreateDirectory(path);
            foreach(var song in Directory.GetFiles(path, "*.wav")) {
                int nameStart = song.LastIndexOf('\\');
                if(nameStart != -1) {
                    string title = song[(nameStart+1)..].Replace(".wav","");
                    CustomSongs.Add(new AudioTupleData(title, song));
                }
            }
            infoFont.normal.textColor = Color.green;
            infoFont.fontSize = 15;
            hotkeyFont.fontSize = 10;
            hotkeyFont.normal.textColor = Color.white;
        }

        public override void OnGUI() {
            if (IsVisible) {
                windowRect = GUI.Window(0, windowRect, (GUI.WindowFunction)DrawWindow, "NRPFarMod by Farliam & iLollek - 2024");
            } else {
                GUI.Label(new Rect(Screen.width - 425, 10, 425, 50), "NRPFarMod - Custom Song Loader - Made by Farliam & iLollek", infoFont);
                GUI.Label(new Rect(Screen.width - 425, 25, 425, 50), $"Now Playing: {currentsong}", infoFont);
            }
        }

        public void DrawWindow(int windowID) {

            CurrentSelectedSong = (int)GUI.HorizontalSlider(new Rect(10, 50, 200, 15), (float)CurrentSelectedSong, 0f, (float)CustomSongs.Count - 1);
          
            var currentSong = CustomSongs.ElementAtOrDefault(CurrentSelectedSong);

            if (currentSong != null)
                GUI.Label(new Rect(10, 30, 200, 30), $"Song: {currentSong.Title}");

            if(GUI.Button(new Rect(10, 70, 200, 20),"Play")) {
                MelonLogger.Msg($"SelectedSong: {CurrentSelectedSong}");
                if(currentSong != null) MelonCoroutines.Start(LoadAudioClipHTTP(currentSong.Path));
            }
            GUI.Label(new Rect(10, 91, 200, 20), "Hotkeys: O/P", hotkeyFont);
            
            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }

        #endregion

        #region OnUpdate (Controls)

        public override void OnUpdate() {
            if (Input.GetKeyDown(KeyCode.Insert) ||
                Input.GetKeyDown(KeyCode.O) ||
                Input.GetKeyDown(KeyCode.P) ||
                Input.GetKeyDown(KeyCode.N) || 
                Input.GetKeyDown(KeyCode.M)) {
                KeyDownHandler = true;
            }
            if(Input.GetKeyUp(KeyCode.Insert) && KeyDownHandler) {
                IsVisible = !IsVisible;
                KeyDownHandler = false;
            }
            if (Input.GetKeyUp(KeyCode.O) && KeyDownHandler) {
                if (isPlaylistModeEnabled == false) {
                    DownSong();
                }
                else
                {
                    MelonLogger.Msg("Unable to use Overwrite-Mode: Playlist-Mode is enabled!");
                }
                KeyDownHandler = false;
            }
            if (Input.GetKeyUp(KeyCode.P) && KeyDownHandler) {
                if (isPlaylistModeEnabled == false)
                {
                    UpSong();
                }
                else
                {
                    MelonLogger.Msg("Unable to use Overwrite-Mode: Playlist-Mode is enabled!");
                }
                KeyDownHandler = false;
            }
            if (Input.GetKeyUp(KeyCode.M) && KeyDownHandler)
            {
                StartPlaylistMode();
                KeyDownHandler = false;
            }
            if (Input.GetKeyUp(KeyCode.N))
            {
                MelonLogger.Msg($"Manually Removing Unused Songs to free RAM...");
                RemoveUnusedSongs();
                KeyDownHandler = false;
            }
        }

        #endregion

        #region Main Mod Functions (Core Functions)
        private void UpSong() {
            CurrentSelectedSong += 1;
            if (CurrentSelectedSong > CustomSongs.Count - 1) CurrentSelectedSong = 0;
            MelonCoroutines.Start(LoadAudioClipHTTP(CustomSongs[CurrentSelectedSong].Path));
            MelonLogger.Msg($"SelectedSong: {CurrentSelectedSong}");
        }

        private void DownSong() {
            CurrentSelectedSong -= 1;
            if(CurrentSelectedSong < 0) CurrentSelectedSong = CustomSongs.Count - 1;
            MelonCoroutines.Start(LoadAudioClipHTTP(CustomSongs[CurrentSelectedSong].Path));
            MelonLogger.Msg($"SelectedSong: {CurrentSelectedSong}");
        }

        private void RemoveUnusedSongs()
        {
            long memoryBefore = GC.GetTotalMemory(false);

            // Iterate through LoadedAudioClips, check if the name of the clip is
            // equal to nowPlaying, if not, delete, if yes, keep in.
            MelonLogger.Msg($"Deleting Unused Music from RAM: {LoadedAudioClips.Count.ToString()}");
            var obj = UnityEngine.Object.FindObjectOfType<GodConstant>();
            MelonLogger.Msg($"MusicSource.name: {obj.musicSource.clip.name}");

            foreach (AudioClip clip in LoadedAudioClips)
            {
                if (clip.name == obj.musicSource.clip.name)
                {
                    // The Clip Name where the Cursor is currently is the same as the one that's currently playing In-Game. 
                    // Do not Delete in this Period.
                    MelonLogger.Msg($"RemoveClipsFromMemory: Not deleting {clip.name} as it is currently playing.");
                }
                else
                {
                    try
                    {
                        clip.UnloadAudioData();
                        Resources.UnloadAsset(clip);
                        AudioClip.Destroy(clip);
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        MelonLogger.Msg($"RemoveClipsFromMemory: Removed {clip.name}");
                    }
                    catch (Exception e)
                    {
                        MelonLogger.Msg($"Unable to delete {clip.name}: {e}");
                    }
                }
            }

            int amountofunusedaudioclips = LoadedAudioClips.Count(clip => clip.name != obj.musicSource.clip.name);

            LoadedAudioClips.RemoveAll(clip => clip.name != obj.musicSource.clip.name);


            MelonLogger.Msg($"Removed {amountofunusedaudioclips.ToString()} unused Audioclips. More RAM should be available now.");
            GC.Collect();
            GC.WaitForPendingFinalizers();

            // Measure memory after cleanup
            long memoryAfter = GC.GetTotalMemory(false);

            // Calculate the amount of memory freed in MB
            long memoryFreed = memoryBefore - memoryAfter;
            double memoryFreedMB = memoryFreed / 1024.0 / 1024.0;

            MelonLogger.Msg($"Removed {amountofunusedaudioclips} unused AudioClips. Freed approximately {memoryFreedMB:F2} MB of RAM.");
        }
        private void StartPlaylistMode()
        {
            try
            {
                // Start a background coroutine to continuously check the song
                MelonCoroutines.Start(CheckAndUpdateSong());
            }
            catch (Exception ex) { MelonLogger.Msg($"PlaylistMode CRASH! :( ({ex.Message})"); }
        }

        private bool CheckIfMusicPlaying()
        {
            try
            {
                var obj = UnityEngine.Object.FindObjectOfType<GodConstant>();
                var test = obj.musicSource.clip.name;
                MelonLogger.Msg($"CheckIfMusicPlaying: Music is playing right now. Able to Overwrite!");
                return true;
            }
            catch (Exception e)
            {
                MelonLogger.Msg($"CheckIfMusicPlaying: No Music playing right now. Exception: {e}");
                return false;
            }
        }

        private IEnumerator CheckAndUpdateSong()
        {
            if (isPlaylistModeEnabled == true)
            {
                MelonLogger.Msg("Playlist-Mode is already Enabled!");
                yield break;
            }

            if (CheckIfMusicPlaying() == true)
            {
                isPlaylistModeEnabled = true;
                MelonLogger.Msg("Started PlaylistSimulator WatcherThread.");
            }
            else
            {
                MelonLogger.Error($"Unable to start PlaylistSimulator WatcherThread. Please try again when there's Music Playing.");
                yield break;
            }
            

            string previousSong = null;

            while (true)
            {
                var obj = UnityEngine.Object.FindObjectOfType<GodConstant>();
                var nowplayingsong = obj.nowPlaying;
                string currentSong = RCC_Settings.getSongName(nowplayingsong.song_ID);

                if (currentSong != previousSong)
                {
                    if (currentSong != string.Empty ) { RemoveUnusedSongs(); }
                    MelonLogger.Msg($"Current playing song: {obj.musicSource.clip.name}");
                    try
                    {
                        currentsong = obj.musicSource.clip.name;
                        obj.UI_Data.ui_nowPlayingText.text = currentsong;
                    }
                    catch (Exception e)
                    {
                        MelonLogger.Error($"PlaylistSimulator: {e}");
                    }
                    // 20% chance to select a random song
                    if (UnityEngine.Random.value < 0.2f && currentSong != string.Empty)
                    {
                        MelonLogger.Msg("Playing something from the User's Playlist now!");
                        int randomIndex = UnityEngine.Random.Range(0, CustomSongs.Count);
                        string randomSongName = Path.GetFileNameWithoutExtension(CustomSongs[randomIndex].Path);

                        RCC_Settings.SoundtrackSong my_custom_song = new RCC_Settings.SoundtrackSong();
                        obj.nowPlaying = my_custom_song;

                        try
                        {
                            currentsong = obj.musicSource.clip.name;
                            obj.UI_Data.ui_nowPlayingText.text = currentsong;
                        }
                        catch (Exception e)
                        {
                            // Do nothing. There's just not any music playing.
                        }

                        MelonCoroutines.Start(LoadAudioClipHTTP(CustomSongs[randomIndex].Path));
                    }

                    previousSong = currentSong;
                }

                // Wait for a short period before checking again
                yield return new WaitForSeconds(1f); // Adjust the interval as needed
            }
        }

        #endregion

        #region HelperFunctions
        private void ListAllGameObjects() {
            Scene currentScene = SceneManager.GetActiveScene();
            GameObject[] rootObjects = currentScene.GetRootGameObjects();
            MelonLogger.Msg($"Anzahl der Wurzel-GameObjects: {rootObjects.Length}");
            foreach (GameObject obj in rootObjects) {
                PrintGameObject(obj, 0);
            }
        }

        private void PrintGameObject(GameObject obj, int indent) {
            string indentStr = new string(' ', indent * 2);
            MelonLogger.Msg($"{indentStr}- {obj.name}");
        }

        #endregion

        #region LoadAudio Components

        private IEnumerator LoadAudioClipHTTP(string path) {
            MelonLogger.Msg($"LoadAudioClipHTTP->{path}");
            UnityWebRequest AudioFiles = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.WAV);
            yield return AudioFiles.SendWebRequest();
            if (AudioFiles.isNetworkError) {
                MelonLogger.Msg(AudioFiles.error);
            } else {
                var obj = UnityEngine.Object.FindObjectOfType<GodConstant>();
                if (obj != null) {
                    AudioClip clip = DownloadHandlerAudioClip.GetContent(AudioFiles);
                    clip.name = Path.GetFileNameWithoutExtension(path); // Replace this with the audioname later!
                    obj.musicSource.clip = clip;
                    obj.musicSource.Play();
                    MelonLogger.Msg($"Playing AudioClip: {clip.name}");
                    LoadedAudioClips.Add(clip);
                    AudioFiles.Dispose();
                }
            }
        }

        private IEnumerator LoadAudioStreamIntoBuffer(string path) {
            // Unused (Deprecated)
            MelonLogger.Msg("Starting-> LoadAudioStreamIntoBuffer");
            var obj = UnityEngine.Object.FindObjectOfType<GodConstant>();
            if (obj != null) {
                MelonLogger.Msg($"Current: {obj.musicSource.clip.name}");
            } else {
                yield break;
            }

            if (!File.Exists(path)) {
                MelonLogger.Msg("File not found: " + path);
                yield break;
            }

            byte[] fileData = File.ReadAllBytes(path);

            WAV wav = new WAV(fileData);
            AudioClip audioClip = AudioClip.Create("audioClip", wav.SampleCount, 1, wav.Frequency, false);
            audioClip.SetData(wav.LeftChannel, 0);

            obj.musicSource.clip = audioClip;
            obj.musicSource.Play();

            MelonLogger.Msg($"Current: {obj.musicSource.clip.name}");
        }

        #endregion

    }
}
