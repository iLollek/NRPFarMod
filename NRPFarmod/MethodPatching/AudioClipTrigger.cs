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

        public static float? OriginalVolume = null;

        private static RCC_DashboardInputs? Instanz;
        
        private static bool SelfCall = false;
        private static System.Random rnd = new System.Random(Guid.NewGuid().GetHashCode());
        public static event EventHandler<PlayListTriggerRaise>? PlayListTriggerEvent;
        public const float PlayListTrigger = 50f;

        public static string CurrentSong { get; private set; } = string.Empty;
        public static bool PlayListMode { get; set; } = false;

        //static void Prefix(bool turnOff) {}

        static void Postfix(bool turnOff) {
            if (CheckInstanz()) {
                if (SelfCall) {
                    SelfCall = false;
                } else {
                    if (PlayListMode && rnd.Next(0, 100) <= PlayListTrigger) {
                        PlayListTriggerEvent?.Invoke(null, new PlayListTriggerRaise(CurrentSong));
                    } else {
                        GodConstant.Instance.musicSource.reverbZoneMix = 1;
                        if(OriginalVolume != null) {
                            GodConstant.Instance.musicVol_Target = OriginalVolume.Value;
                            OriginalVolume = null;
                        } else {
                            OriginalVolume = GodConstant.Instance.musicVol_Target;
                        }
                        GodConstant.Instance.musicSource.pitch = 1;
                        CurrentSong = Instanz!.ui_nowPlayingText.text;
                    }
                }
            }
        }

        static AudioClipTrigger() {
            Instanz = UnityEngine.Object.FindObjectOfType<RCC_DashboardInputs>();
        }

        public static void SetNowPlaying(string message) {
            if (CheckInstanz()) {
                SelfCall = true;
                CurrentSong = message;
                Instanz!.start_fade_nowPlaying(false);
                Instanz!.ui_nowPlayingText.text = $" Now Playling: {message}";
                Instanz!.phoneNowPlaying.text = message;
            }
        }

        private static bool CheckInstanz() {
            if (Instanz == null) Instanz = UnityEngine.Object.FindObjectOfType<RCC_DashboardInputs>();
            return Instanz != null;
        }
    }


}



