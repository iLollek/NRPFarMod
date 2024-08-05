using Il2Cpp;
using MelonLoader;
using NAudio.Wave;
using UnityEngine.Networking;
using UnityEngine;
using System.Collections;
using MelonLoader.TinyJSON;
using System.Text.Json;
using NRPFarmod.CustomUnityScripts;
using static Il2Cpp.GodConstant;
using UnityEngine.Playables;

namespace NRPFarmod.ContentManager {


    /// <summary>
    /// Saves Information about a Song
    /// </summary>
    public sealed class AudioClipInfo {
        /// <summary>
        /// Full path to file
        /// </summary>
        public string FullPath { get; set; } = string.Empty;
        /// <summary>
        /// Text that is shown on the Display
        /// </summary>
        public string UI_Display { get; set; } = string.Empty;
        /// <summary>
        /// Additional Volume Factor
        /// </summary>
        public float Volume { get; set; } = 0f;
        /// <summary>
        /// Additional Pitch Factor
        /// </summary>
        public float Pitch { get; set; } = 1f;
        /// <summary>
        /// Additional Speed Factor
        /// </summary>
        public float ReverbZoneMix { get; set; } = 1f;
    }

    /// <summary>
    /// New song created
    /// </summary>
    public class NewAudioClipPlaying : EventArgs {
        public readonly UnityEngine.AudioClip clip;
        public NewAudioClipPlaying(UnityEngine.AudioClip clip) {
            this.clip = clip ?? new UnityEngine.AudioClip();
        }
    }


    public class ContentManager<T> : IDisposable where T : UnityEngine.Object {

        #region Privat
        /// <summary>
        /// Random Numbers
        /// </summary>
        private readonly System.Random rnd;
        /// <summary>
        /// Index in Dictionary
        /// </summary>
        protected int currentIndex = 0;
        /// <summary>
        /// ContentManager
        /// </summary>
        protected readonly ManagedContent<T> managedContent;
        /// <summary>
        /// Saves Music and Index
        /// </summary>
        protected IDictionary<int, AudioClipInfo>? MusicData;
        /// <summary>
        /// Clean Audiofiles
        /// </summary>
        protected readonly string TempPath;
        /// <summary>
        /// All Files
        /// </summary>
        protected readonly string ContentFolder;
        #endregion

        #region Property
        /// <summary>
        /// Getter/Setter for Info
        /// </summary>
        public AudioClipInfo CurrentClipInfo {
            get {
                return MusicData![currentIndex];
            }
            set {
                if (value != null) {
                    MusicData![currentIndex] = value;
                }
            }
        }
        /// <summary>
        /// Memory dedication
        /// </summary>
        public IReadOnlyList<double> Memory { get => managedContent.MemoryInformation; }
        #endregion

        #region Events
        /// <summary>
        /// Triggered when a new song has been loaded
        /// </summary>
        public event EventHandler<NewAudioClipPlaying>? NewAudiClip;
        /// <summary>
        /// Raise Event
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnNewAudioClip(NewAudioClipPlaying args) {
            EventHandler<NewAudioClipPlaying>? handler = NewAudiClip;
            handler?.Invoke(this, args);
        }

        #endregion

        #region Konstruktor & Initialize
        /// <summary>
        /// Resets the settings
        /// </summary>
        public void ResetCurrentSongInfo() {
            try {
                if (MusicData == null) return;
                var file = Path.Combine(ContentFolder, "Settings.json");
                if (File.Exists(file)) {
                    string content = File.ReadAllText(file);
                    var set = JsonSerializer.Deserialize<AudioClipInfo[]>(content);
                    if (set != null) {
                        MusicData[currentIndex] = set[currentIndex];
                        MelonLogger.Msg($"Reset Song Settings: {set[currentIndex].UI_Display}");
                    }
                }
            }catch(Exception ex) {
                MelonLogger.Error(ex);
            }

        }
        /// <summary>
        /// Saves the current song data
        /// </summary>
        private void SafeSettings() {
            var file = Path.Combine(ContentFolder, "Settings.json");
            string myContent = JsonSerializer.Serialize(MusicData!.Select(x => x.Value).ToArray(), new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(file, myContent);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ContentManager() {
            TempPath = Path.Combine(Environment.CurrentDirectory, $"Mods\\src\\used");
            if (!Directory.Exists(TempPath)) Directory.CreateDirectory(TempPath);
            ContentFolder = Path.Combine(Environment.CurrentDirectory, "Mods\\src\\");
            managedContent = new ManagedContent<T>();
            rnd = new System.Random(Guid.NewGuid().GetHashCode());
        }
        /// <summary>
        /// Begin conversion of files
        /// </summary>
        public virtual void OnInitialize() {
            //Konvertieren und verschieben
            ConvertMp3ToWave(ContentFolder);
            //Musik aus alle Ordner laden und in den Ordner einfügen
            LoadSettingFile();
        }

        /// <summary>
        /// Handles the Json Settings
        /// </summary>
        private void LoadSettingFile() {

            var curMusikData = LoadWaveFiles(ContentFolder)
                .Concat(LoadWaveFiles(TempPath))
                .Select((file, index) => new { Index = index, File = file })
                .ToDictionary(x => x.Index, x => x.File);

            var file = Path.Combine(ContentFolder, "Settings.json");

            var resultDict = new Dictionary<int, AudioClipInfo>();

            if (File.Exists(file)) {
                string content = File.ReadAllText(file);
                var set = JsonSerializer.Deserialize<AudioClipInfo[]>(content);
                if (set != null) {
                    foreach (var clip in set) {
                        resultDict.Add(resultDict.Count, clip);
                        MelonLogger.Msg($"Load SongInfo: {clip.UI_Display}");
                        if (curMusikData.Any(x => x.Value.FullPath == clip.FullPath)) {
                            int index = curMusikData.First(x => x.Value.FullPath == clip.FullPath).Key;
                            curMusikData.Remove(index);
                        }
                    }
                    foreach (var newClip in curMusikData) {
                        resultDict.Add(resultDict.Count, newClip.Value);
                        MelonLogger.Msg($"New SongInfo: {newClip.Value.UI_Display}");
                    }
                    MusicData = resultDict;
                }
            } else {
                MusicData = curMusikData;
            }

            string myContent = JsonSerializer.Serialize(MusicData!.Select(x => x.Value).ToArray(), new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(file, myContent);
        }

        /// <summary>
        /// Collect Information about the WAVE files
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public virtual IList<AudioClipInfo> LoadWaveFiles(string folderPath) {
            var waveFiles = Directory.GetFiles(folderPath, "*.wav");
            if (waveFiles != null) {
                return waveFiles.Select(x => new AudioClipInfo() {
                    FullPath = x,
                    Pitch = 1f,
                    Volume = 0f,
                    ReverbZoneMix = 1f,
                    UI_Display = Path.GetFileName(x)
                }).ToList();
            }
            return new List<AudioClipInfo>();
        }
        /// <summary>
        /// Converts the Mp3 files and moves them to the TempPath
        /// </summary>
        /// <param name="folderPath"></param>
        public virtual void ConvertMp3ToWave(string folderPath) {
            var mp3Files = Directory.GetFiles(folderPath, "*.mp3");
            foreach (var mp3 in mp3Files) {
                try {
                    var newFileName = Path.Combine(TempPath, Path.GetFileName(mp3).Replace(".mp3", ".wav"));
                    if (File.Exists(newFileName)) {
                        MelonLogger.Msg($"Skip Song: {Path.GetFileName(mp3)}");
                        continue;
                    }
                    MelonLogger.Msg($"Start Convert {Path.GetFileName(mp3)}");
                    using (var mp3Reader = new Mp3FileReader(mp3)) {
                        var waveFormat = new WaveFormat(44100, 16, 2);
                        using (var waveStream = new WaveFormatConversionStream(waveFormat, mp3Reader)) {
                            WaveFileWriter.CreateWaveFile(newFileName, waveStream);
                        }
                    }
                } catch (Exception ex) {
                    MelonLogger.Error(ex);
                }
            }
        }

        #endregion

        #region Navigation
        /// <summary>
        /// Loads the next Song
        /// </summary>
        public virtual void LoadNextSong(AudioSource player) {
            if (MusicData == null) return;
            currentIndex = currentIndex < MusicData?.Count - 1 ? currentIndex + 1 : 0;
            ContentLoader(new Action<object>((obj) => {
                managedContent.SetContet((T)obj);
                PlayCurrentSong(player);
            }));
        }
        /// <summary>
        /// Loads the previous Song
        /// </summary>
        public virtual void LoadPrevSong(AudioSource player) {
            if (MusicData == null) return;
            currentIndex = currentIndex == 0 ? MusicData.Count - 1 : currentIndex - 1;
            ContentLoader(new Action<object>((obj) => {
                managedContent.SetContet((T)obj);
                PlayCurrentSong(player);
            }));
        }
        /// <summary>
        /// Loads a random Song
        /// </summary>
        public virtual void LoadRandomSong(AudioSource player) {
            if (MusicData == null) return;
            currentIndex = rnd.Next(0, MusicData.Count - 1);
            ContentLoader(new Action<object>((obj) => {
                managedContent.SetContet((T)obj);
                PlayCurrentSong(player);
            }));
        }
        /// <summary>
        /// Plays the current Song
        /// </summary>
        /// <param name="player"></param>
        protected virtual void PlayCurrentSong(AudioSource player) {
            if (managedContent.Value is AudioClip clip) {
                OnNewAudioClip(new NewAudioClipPlaying(clip));
                GodConstant.Instance.musicVol_Target = 0;
                player.pitch = 1;
                player.reverbZoneMix = 1;
                player.clip = managedContent.Value as AudioClip;
                player.Play();
                var sounddata = MusicData![currentIndex];
                GodConstant.Instance.musicVol_Target = sounddata.Volume;
                player.pitch = sounddata.Pitch;
                player.reverbZoneMix = sounddata.ReverbZoneMix;
                AudioClipTrigger.SetNowPlaying(MusicData?[currentIndex].UI_Display ?? "Outsch :3");
            } else {
                MelonLogger.Error($"Type {typeof(T)} not supported");
            }
        }
        /// <summary>
        /// Custom Contentloader
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public virtual void ContentLoader(Action<object>? PostCall) {
            if (MusicData?.Count is <= 0) throw new NullReferenceException(nameof(MusicData));
            if (currentIndex < 0 || currentIndex >= MusicData!.Count) throw new IndexOutOfRangeException(nameof(currentIndex));
            var song = MusicData[currentIndex];
            MelonCoroutines.Start(LoadAudioClipHTTP(song.FullPath, new Action<object>((input) => {
                PostCall?.Invoke((T)input);
            })));
        }
        /// <summary>
        /// Loads the Audioclip
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private IEnumerator LoadAudioClipHTTP(string path, Action<object> CallBack) {
            MelonLogger.Msg($"LoadAudioClipHTTP->{Path.GetFileName(path)}");
            UnityWebRequest AudioFiles = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.WAV);
            yield return AudioFiles.SendWebRequest();
            if (AudioFiles.isNetworkError) {
                MelonLogger.Msg(AudioFiles.error);
            } else {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(AudioFiles);
                clip.name = Path.GetFileNameWithoutExtension(path);
                AudioFiles.Dispose();
                CallBack?.Invoke(clip);
            }
        }
        #endregion

        #region Cleanup
        /// <summary>
        /// Cleanup
        /// </summary>
        public void Dispose() {
            SafeSettings();
            managedContent.Dispose();
        }
        #endregion


    }
}
