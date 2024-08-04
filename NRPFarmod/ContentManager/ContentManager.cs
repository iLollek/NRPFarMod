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

namespace NRPFarmod.ContentManager {


    /// <summary>
    /// Speichert Informationen über einen Song
    /// </summary>
    public sealed class AudiClipInfo {
        /// <summary>
        /// Voller Pfad zur Datei
        /// </summary>
        public string FullPath { get; set; } = string.Empty;
        /// <summary>
        /// Text welcher im Display angezeigt wird
        /// </summary>
        public string UI_Display { get; set; } = string.Empty;
        /// <summary>
        /// Additionaler Volume Faktor
        /// </summary>
        public float Volume { get; set; } = 0f;
        /// <summary>
        /// Additionaler Pitch Faktor
        /// </summary>
        public float Pitch { get; set; } = 0f;
    }

    /// <summary>
    /// Neuer Song erstellt
    /// </summary>
    public class NewAudioClipPlaying : EventArgs {
        public readonly UnityEngine.AudioClip clip;
        public NewAudioClipPlaying(UnityEngine.AudioClip clip) {
            this.clip = clip ?? new UnityEngine.AudioClip();
        }
    }


    public class ContentManager<T> : IDisposable where T : UnityEngine.Object {

        #region Private
        /// <summary>
        /// Random Numbers
        /// </summary>
        private readonly System.Random rnd;
        /// <summary>
        /// Index im Dictionary
        /// </summary>
        protected int currentIndex = 0;
        /// <summary>
        /// ContentManager
        /// </summary>
        protected readonly ManagedContent<T> managedContent;
        /// <summary>
        /// Speichert Musik und Index
        /// </summary>
        protected IReadOnlyDictionary<int, AudiClipInfo>? MusicData;
        /// <summary>
        /// Clean Audiofiles
        /// </summary>
        protected readonly string TempPath;
        /// <summary>
        /// All Files
        /// </summary>
        protected readonly string ContentFolder;
        #endregion

        #region Events
        /// <summary>
        /// Wird ausgelöst wenn ein neuer Song geladen wurde
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
        /// Speichert die aktuellen Songdaten
        /// </summary>
        private void SafeSettings() {
            var file = Path.Combine(ContentFolder, "Settings.json");
            string myContent = JsonSerializer.Serialize(MusicData!.Select(x => x.Value).ToArray(), new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(file, myContent);
        }

        /// <summary>
        /// Konstruktor
        /// </summary>
        public ContentManager() {
            TempPath = Path.Combine(Environment.CurrentDirectory, $"Mods\\src\\used");
            if (!Directory.Exists(TempPath)) Directory.CreateDirectory(TempPath);
            ContentFolder = Path.Combine(Environment.CurrentDirectory, "Mods\\src\\");
            managedContent = new ManagedContent<T>();
            rnd = new System.Random(Guid.NewGuid().GetHashCode());
        }
        /// <summary>
        /// Starte konvertieren der Dateien
        /// </summary>
        public virtual void OnInitialize() {
            //Konvertieren und verschieben
            ConvertMp3ToWave(ContentFolder);
            //Musik aus alle Ordner laden und in den Ordner einfügen
            LoadSettingFile();
        }

        /// <summary>
        /// Behandelt die Json Settings
        /// </summary>
        private void LoadSettingFile() {

            var curMusikData = LoadWaveFiles(ContentFolder)
                .Concat(LoadWaveFiles(TempPath))
                .Select((file, index) => new { Index = index, File = file })
                .ToDictionary(x => x.Index, x => x.File);

            var file = Path.Combine(ContentFolder, "Settings.json");

            var resultDict = new Dictionary<int, AudiClipInfo>();

            if(File.Exists(file)) {
                string content = File.ReadAllText(file);
                var set = JsonSerializer.Deserialize<AudiClipInfo[]>(content);
                if (set != null) {
                    foreach(var clip in set) {
                        resultDict.Add(resultDict.Count, clip);
                        MelonLogger.Msg($"Load SongInfo: {clip.UI_Display}");
                        if(curMusikData.Any(x => x.Value.FullPath == clip.FullPath)) {
                            int index = curMusikData.First(x => x.Value.FullPath == clip.FullPath).Key;
                            curMusikData.Remove(index);
                        }
                    }
                    foreach(var newClip in curMusikData) {
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
        /// Informationen über die Wave Dateien sammeln
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public virtual IReadOnlyList<AudiClipInfo> LoadWaveFiles(string folderPath) {
            var waveFiles = Directory.GetFiles(folderPath, "*.wav");
            if (waveFiles != null) {
                return waveFiles.Select(x => new AudiClipInfo() {
                    FullPath = x,
                    Pitch = 0.0f,
                    Volume = 0.0f,
                    UI_Display = Path.GetFileName(x)
                }).ToList();
            }
            return new List<AudiClipInfo>();
        }
        /// <summary>
        /// Konvertiert die Mp3 Dateien und verschiebt sie in den TempPfad
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
        /// Ladet den nächsten Song
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
        /// Ladet den vorherigen Song
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
        /// Lädt einen Random Song
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
        /// Spielt den aktuellen Song
        /// </summary>
        /// <param name="player"></param>
        protected virtual void PlayCurrentSong(AudioSource player) {
            if (managedContent.Value is AudioClip clip) {
                OnNewAudioClip(new NewAudioClipPlaying(clip));
                player.clip = managedContent.Value as AudioClip;
                player.Play();
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
        /// Lädt den Audioclip
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private IEnumerator LoadAudioClipHTTP(string path, Action<object> CallBack) {
            MelonLogger.Msg($"LoadAudioClipHTTP->{path}");
            UnityWebRequest AudioFiles = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.WAV);
            yield return AudioFiles.SendWebRequest();
            if (AudioFiles.isNetworkError) {
                MelonLogger.Msg(AudioFiles.error);
            } else {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(AudioFiles);
                clip.name = Path.GetFileNameWithoutExtension(path);
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
