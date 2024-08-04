
using MelonLoader;
using UnityEngine;
using NRPFarmod;
using NRPFarmod.MelonCall;
using NRPFarmod.ContentManager;
using Il2Cpp;
using NRPFarmod.CustomUnityScripts;
using Il2CppInterop.Runtime.Injection;
using NRPFarmod.UIHelper;




namespace NRPFarmod {
    public class NRPFarModController : MelonCaller {


        private Color UIBackground = new Color(30f / 255f, 30f / 255f, 30f / 255f);
        private Color UIForeground = Color.green;
        private bool windowStyle = false;
        private readonly ContentManager<AudioClip> contentManager = new();
        private Rect windowRect = new Rect(20, 20, 600, 300);
        private bool IsVisible = false;
        private GUIStyle infoFont = new();
        private GUIStyle hotkeyFont = new();

        private GodConstant? godConstant;

        private UITabControl UITabControl;


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

        public override void OnInitializeMelon() {
            infoFont.normal.textColor = Color.green;
            infoFont.fontSize = 15;
            hotkeyFont.fontSize = 10;
            hotkeyFont.normal.textColor = Color.white;


            MelonLogger.Msg("Init \u001b[32mNRPFarModController\u001b[0m");
        }

        public NRPFarModController() : base() {
            _instanz = this;
            UITabControl = new UITabControl(ref windowRect, new Vector2(2, 17), new Vector2(windowRect.width - 2, windowRect.height - 17));
        }


        public override void OnMelonCallerLoaded() {

            MelonLogger.Msg("Sart Convert...");
            contentManager.OnInitialize();
            MelonLogger.Msg(ConsoleColor.Green, "Convert end...");
            SingleKeyInputController.Instanz?.AddKeyCallback(KeyCode.Insert, SwitchWindowVisibility);
            SingleKeyInputController.Instanz?.AddKeyCallback(KeyCode.O, PreviousSong);
            SingleKeyInputController.Instanz?.AddKeyCallback(KeyCode.P, NextSong);
            SingleKeyInputController.Instanz?.AddKeyCallback(KeyCode.Z, RandomSong);



            //UI Tab Chain erstellen und Initialisieren
            UITabControl.AddContent("Current Song", new Action(() => {
                //Draw UI
            })).AddContent("Edit Song", new Action(() => {
                //Draw UI
            })).AddContent("Memory View", new Action(() => {
                //Draw UI
            })).Initialize(); //Init

            //Anschließend Registrieren
            NRPFarMod.Instanz?.Register(UITabControl);
        }

        #region Window UI

        public override void OnGUI() {
            if (IsVisible) {
                GUI.backgroundColor = UIBackground;
                GUI.skin.window.normal.textColor = UIForeground;
                GUI.skin.window.hover.textColor = UIForeground;
                GUI.skin.window.active.textColor = UIBackground;
                windowRect = GUI.Window(0, windowRect, (GUI.WindowFunction)DrawWindow, "NRPFarMod by Farliam & iLollek - 2024");
            } else {
                GUI.Label(new Rect(Screen.width - 425, 10, 425, 50), "NRPFarMod - Custom Song Loader - Made by Farliam & iLollek", infoFont);
                GUI.Label(new Rect(Screen.width - 425, 25, 425, 50), $"Now Playing: ", infoFont);
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
            if (CheckGodConstant()) {
                contentManager.LoadNextSong(godConstant!.musicSource);
            }
        }

        private void PreviousSong() {
            if (CheckGodConstant()) {
                contentManager.LoadPrevSong(godConstant!.musicSource);
            }
        }

        private void RandomSong() {
            if (CheckGodConstant()) {
                contentManager.LoadRandomSong(godConstant!.musicSource);
            }
        }

        #endregion

        private bool CheckGodConstant() {
            if (godConstant == null) {
                godConstant = GodConstant.Instance; //Warum jedes mal die komplette Assembly nach ner Instanz durchsuchen :D
            }
            return !(godConstant == null);
        }

        public override void OnApplicationQuit() {
            contentManager.Dispose();
        }
    }
}
