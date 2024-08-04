using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace NRPFarmod.CustomUnityScripts {


    //https://github.com/pardeike/Harmony/releases/tag/v2.3.3.0

    [HarmonyPatch(typeof(RCC_DashboardInputs), nameof(RCC_DashboardInputs.start_fade_nowPlaying))]
    public static class AudioClipTrigger {

        private static string lastSong = string.Empty;

        private static RCC_DashboardInputs? Instanz;

        //static void Prefix(bool turnOff) {}

        static void Postfix(bool turnOff) {
            if (Instanz == null) {
                Instanz = UnityEngine.Object.FindObjectOfType<RCC_DashboardInputs>();
            } else {
                if (Instanz.ui_nowPlayingText.text != lastSong) {
                    lastSong = Instanz.ui_nowPlayingText.text;
                    MelonLogger.Msg($"Now Playling: {lastSong}");
                }
            }
        }

        public static void SetNowPlaying(string message) {
            if( Instanz != null ) {
                Instanz.start_fade_nowPlaying(false);
                Instanz.ui_nowPlayingText.text = message;
            }
        }

    }
}
