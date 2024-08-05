using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace NRPFarmod.CustomUnityScripts {

    public class PlayListTriggerRaise : EventArgs {
        private readonly string LastSong;
        public PlayListTriggerRaise(string lastSong) {
            LastSong = lastSong ?? throw new ArgumentNullException(nameof(lastSong));
        }
    }


    //https://github.com/pardeike/Harmony/releases/tag/v2.3.3.0

    [HarmonyPatch(typeof(RCC_DashboardInputs), nameof(RCC_DashboardInputs.start_fade_nowPlaying))]
    public static class AudioClipTrigger {

        private static System.Random rnd = new System.Random(Guid.NewGuid().GetHashCode());

        public static event EventHandler<PlayListTriggerRaise>? PlayListTriggerEvent;

        public const float PlayListTrigger = 50f;
        public static string CurrentSong { get; private set; } = string.Empty;
        public static bool PlayListMode { get; set; } = false;

        private static RCC_DashboardInputs? Instanz;

        private static bool SelfCall = false;

        //static void Prefix(bool turnOff) {}

        static void Prefix(bool turnOff) {
            if (Instanz == null) Instanz = UnityEngine.Object.FindObjectOfType<RCC_DashboardInputs>();
            if (Instanz == null) return;

            if (SelfCall) {
                SelfCall = false;
            } else {
                if (PlayListMode && rnd.Next(0, 100) <= PlayListTrigger) {
                    PlayListTriggerEvent?.Invoke(null, new PlayListTriggerRaise(CurrentSong));
                } else {
                    GodConstant.Instance.musicSource.reverbZoneMix = 1;
                    GodConstant.Instance.musicVol_Target = 0;
                    GodConstant.Instance.musicSource.pitch = 1;
                    CurrentSong = Instanz.ui_nowPlayingText.text;
                    MelonLogger.Msg($"[AudioClipTrigger] Now Playling: {CurrentSong}");
                }
            }
        }


        static AudioClipTrigger() {
            Instanz = UnityEngine.Object.FindObjectOfType<RCC_DashboardInputs>();
        }

        public static void SetNowPlaying(string message) {
            if (Instanz != null) {
                SelfCall = true;
                Instanz.start_fade_nowPlaying(false);
                Instanz.ui_nowPlayingText.text = message;
                CurrentSong = Instanz.ui_nowPlayingText.text;
                MelonLogger.Msg($"[AudioClipTrigger] Now Playling: {CurrentSong}");
            } else {
                Instanz = UnityEngine.Object.FindObjectOfType<RCC_DashboardInputs>();
            }
        }

    }


}



