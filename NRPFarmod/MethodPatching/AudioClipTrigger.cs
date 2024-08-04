using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace NRPFarmod.CustomUnityScripts {



    //https://github.com/pardeike/Harmony/releases/tag/v2.3.3.0

    [HarmonyPatch(typeof(RCC_DashboardInputs), nameof(RCC_DashboardInputs.start_fade_nowPlaying))]
    public static class AudioClipTrigger {

        public static string CurrentSong { get; private set; } = string.Empty;

        private static RCC_DashboardInputs? Instanz;

        //static void Prefix(bool turnOff) {}

        static void Prefix(bool turnOff) {
            if (Instanz == null) {
                Instanz = UnityEngine.Object.FindObjectOfType<RCC_DashboardInputs>();
            } else {
                CurrentSong = Instanz.ui_nowPlayingText.text;
                MelonLogger.Msg($"[AudioClipTrigger] Now Playling: {CurrentSong}");
            }
        }

        static AudioClipTrigger() {
            Instanz = UnityEngine.Object.FindObjectOfType<RCC_DashboardInputs>();
        }

        public static void SetNowPlaying(string message) {
            if (Instanz != null) {
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



