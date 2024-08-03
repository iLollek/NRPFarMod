using Il2Cpp;
using MelonLoader;
using NAudio.Wave;
using UnityEngine.Networking;
using UnityEngine;
using System.Collections;

namespace NRPFarmod.ContentManager {


    /// <summary>
    /// Neuer Song erstellt
    /// </summary>
    public class NewAudioClipEventArgs : EventArgs {
        public readonly UnityEngine.AudioClip clip;
        public NewAudioClipEventArgs(UnityEngine.AudioClip clip) {
            this.clip = clip ?? new UnityEngine.AudioClip();
        }
    }

    /// <summary>
    /// Music Data
    /// </summary>
    /// <param name="FilePath"></param>
    /// <param name="FileName"></param>
    public record CustomMusic(string FilePath, string FileName);

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
        protected IReadOnlyDictionary<int, CustomMusic>? MusicData;
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
        public event EventHandler<NewAudioClipEventArgs>? NewAudiClip;
        /// <summary>
        /// Raise Event
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnNewAudioClip(NewAudioClipEventArgs args) {
            EventHandler<NewAudioClipEventArgs>? handler = NewAudiClip;
            handler?.Invoke(this, args);
        }

        #endregion

        #region Konstruktor & Initialize
        /// <summary>
        /// Konstruktor
        /// </summary>
        public ContentManager() {
            TempPath = Path.Combine(Environment.CurrentDirectory, $"Mods\\src\\{Guid.NewGuid().ToString().Replace("-", "")}");
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
            MusicData = LoadWaveFiles(ContentFolder)
            .Concat(LoadWaveFiles(TempPath))
            .Select((file, index) => new { Index = index, File = file })
            .ToDictionary(x => x.Index, x => x.File);
        }
        /// <summary>
        /// Informationen über die Wave Dateien sammeln
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public virtual IReadOnlyList<CustomMusic> LoadWaveFiles(string folderPath) {
            var waveFiles = Directory.GetFiles(folderPath, "*.wav");
            if (waveFiles != null) {
                return waveFiles.Select(x => new CustomMusic(Path.GetFileName(x), x)).ToList();
            }
            return new List<CustomMusic>();
        }
        /// <summary>
        /// Konvertiert die Mp3 Dateien und verschiebt sie in den TempPfad
        /// </summary>
        /// <param name="folderPath"></param>
        public virtual void ConvertMp3ToWave(string folderPath) {
            var mp3Files = Directory.GetFiles(folderPath, "*.mp3");
            foreach (var mp3 in mp3Files) {
                try {
                    MelonLogger.Msg($"Start Convert {mp3}");
                    var newFileName = Path.Combine(TempPath, Path.GetFileName(mp3).Replace(".mp3", ".wav"));
                    using (var mp3Reader = new Mp3FileReader(mp3)) {
                        using (var waveStream = WaveFormatConversionStream.CreatePcmStream(mp3Reader)) {
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
            if (managedContent.type == typeof(AudioClip)) {
                player.clip = managedContent.Value as AudioClip;
                player.Play();
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
            MelonCoroutines.Start(LoadAudioClipHTTP(song.FileName, new Action<object>((input) => {
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
            managedContent.Dispose();
            Directory.Delete(TempPath, true);
        }
        #endregion

    }
}
