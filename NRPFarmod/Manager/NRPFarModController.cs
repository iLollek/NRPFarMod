using MelonLoader;
using UnityEngine;
using NRPFarmod.MelonCall;
using NRPFarmod.ContentManager;
using Il2Cpp;
using NRPFarmod.CustomUnityScripts;
using NRPFarmod.UIHelper;
using System.Collections;
using NRPFarmod.UI;

namespace NRPFarmod {
    public class NRPFarModController : MelonCaller {

        #region Private Variabel
        private readonly ContentManager<AudioClip> contentManager = new();
        private readonly EditSongGUIManager<AudioClip> EditSongGUIManager;
        private readonly MemoryGUIManager<AudioClip> MemoryGUIManager;
        private readonly AboutGUIManager AboutGUIManager;
        private readonly CurrentSongGUIManager CurrentSongGUIManager;
        private readonly UITabControl UITabControl;
        private readonly Color UIBackground = new Color(30f / 255f, 30f / 255f, 30f / 255f);
        private Rect windowRect = new Rect(20, 20, 800, 300);
        private bool IsVisible = false;
        private GUIStyle infoFont = new();
        private GUIStyle hotkeyFont = new();
        private GodConstant godConstant;
        #endregion

        #region Instanz
        private static object _lock = new object();
        private static NRPFarModController? _instanz = null;

        public static NRPFarModController? Instanz {
            get => LockMe();
        }

        private static NRPFarModController? LockMe() {
            lock (_lock) {
                return _instanz;
            }
        }
        #endregion

        #region Base&Override
        public override void OnInitializeMelon() {
            infoFont.normal.textColor = Color.green;
            infoFont.fontSize = 15;
            hotkeyFont.fontSize = 10;
            hotkeyFont.normal.textColor = Color.white;
            MelonLogger.Msg("Init \u001b[32mNRPFarModController\u001b[0m");
            AudioClipTrigger.PlayListTriggerEvent += AudioClipTrigger_PlayListTriggerEvent;
        }

        public NRPFarModController() : base() {
            _instanz = this;
            godConstant = GodConstant.Instance;
            UITabControl = new UITabControl(ref windowRect, new Vector2(2, 22), new Vector2(windowRect.width - 2, windowRect.height - 22));
            AboutGUIManager = new AboutGUIManager(windowRect, new Vector2(10, 45));
            EditSongGUIManager = new EditSongGUIManager<AudioClip>(contentManager, windowRect, new Vector2(10, 45));
            CurrentSongGUIManager = new CurrentSongGUIManager(windowRect, new Vector2(10, 45), PreviousSong, NextSong);
            MemoryGUIManager = new MemoryGUIManager<AudioClip>(contentManager, windowRect, new Vector2(10, 45));
        }

        public override void OnMelonCallerLoaded() {

            MelonLogger.Msg("Start Convert...");
            contentManager.OnInitialize();
            MelonLogger.Msg(ConsoleColor.Green, "Convert end...");
            SingleKeyInputController.Instanz?.AddKeyCallback(KeyCode.Insert, SwitchWindowVisibility);
            SingleKeyInputController.Instanz?.AddKeyCallback(KeyCode.O, PreviousSong);
            SingleKeyInputController.Instanz?.AddKeyCallback(KeyCode.P, NextSong);
            SingleKeyInputController.Instanz?.AddKeyCallback(KeyCode.Z, RandomSong);


            // UI Chain create and initialize
            UITabControl.AddContent("Current Song", new Action(() => {
                CurrentSongGUIManager.DrawUI();
            })).AddContent("Edit Song", new Action(() => {
                EditSongGUIManager.DrawUI();
            })).AddContent("Memory View", new Action(() => {
                MemoryGUIManager.DrawUI();
            })).AddContent("About", new Action(() => {
                AboutGUIManager.DrawUI();
            })).Initialize(); //Init

            // followup: registration
            NRPFarMod.Instanz?.Register(UITabControl);
            NRPFarMod.Instanz?.Register(AboutGUIManager);
            NRPFarMod.Instanz?.Register(CurrentSongGUIManager);
            NRPFarMod.Instanz?.Register(MemoryGUIManager);
        }

        private void AudioClipTrigger_PlayListTriggerEvent(object? sender, PlayListTriggerRaise e) {
            MelonLogger.Msg(ConsoleColor.Green, $"PlaylistTrigger Event!");
            RandomSong();
        }

        public override void OnApplicationQuit() {
            contentManager.Dispose();
        }
        #endregion

        #region Window

        public override void OnSceneWasLoaded(int buildIndex, string sceneName) {
            MelonLogger.Msg(ConsoleColor.Yellow, $"New Scene loaded: {sceneName}");
        }

        public override void OnGUI() {
            if (IsVisible) {
                GUI.backgroundColor = UIBackground;
                UnityEngine.Color color = UnityEngine.Color.green;
                GUI.skin.window.normal.textColor = color;
                GUI.skin.window.hover.textColor = color;
                GUI.skin.window.active.textColor = color;
                GUI.skin.window.focused.textColor = color;
                GUI.skin.window.onNormal.textColor = color;
                GUI.skin.window.onHover.textColor = color;
                GUI.skin.window.onActive.textColor = color;
                GUI.skin.window.onFocused.textColor = color;
                windowRect = GUI.Window(0, windowRect, (GUI.WindowFunction)DrawWindow, "NRPFarMod 08.2024", GUI.skin.window);
            } else {
                GUI.Label(new Rect(Screen.width - 150, 10, 425, 50), "NRPFarMod 08.2024", infoFont);
            }
        }

        public void DrawWindow(int windowID) {
            UITabControl.DrawWindowData(windowID);
            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }

        #endregion

        #region Definition der Callback Funktionen

        private void SwitchWindowVisibility() {
            IsVisible = !IsVisible;
            UITabControl.Visibility = IsVisible;
            Cursor.visible = IsVisible;
            Cursor.lockState = IsVisible ? CursorLockMode.None : CursorLockMode.Locked;
        }

        private void NextSong() {
            contentManager.LoadNextSong(godConstant!.musicSource);
            MemoryGUIManager.RefreshTexture();
        }

        private void PreviousSong() {
            contentManager.LoadPrevSong(godConstant!.musicSource);
            MemoryGUIManager.RefreshTexture();
        }

        private void RandomSong() {
            contentManager.LoadRandomSong(godConstant!.musicSource);
            MemoryGUIManager.RefreshTexture();
        }

        #endregion

    }
}
