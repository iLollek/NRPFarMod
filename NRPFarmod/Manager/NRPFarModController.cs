
using MelonLoader;
using UnityEngine;
using NRPFarmod;
using NRPFarmod.MelonCall;
using NRPFarmod.ContentManager;
using Il2Cpp;
using NRPFarmod.CustomUnityScripts;
using Il2CppInterop.Runtime.Injection;
using NRPFarmod.UIHelper;
using UnityEngine.Playables;






namespace NRPFarmod {
    public class NRPFarModController : MelonCaller {


        private Color UIBackground = new Color(30f / 255f, 30f / 255f, 30f / 255f);
        private Color UIForeground = Color.green;
        private bool windowStyle = false;
        private readonly ContentManager<AudioClip> contentManager = new();
        private Rect windowRect = new Rect(20, 20, 800, 300);
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
            UITabControl = new UITabControl(ref windowRect, new Vector2(2, 22), new Vector2(windowRect.width - 2, windowRect.height - 22));
        }


        public override void OnMelonCallerLoaded() {

            MelonLogger.Msg("Start Convert...");
            contentManager.OnInitialize();
            MelonLogger.Msg(ConsoleColor.Green, "Convert end...");
            SingleKeyInputController.Instanz?.AddKeyCallback(KeyCode.Insert, SwitchWindowVisibility);
            SingleKeyInputController.Instanz?.AddKeyCallback(KeyCode.O, PreviousSong);
            SingleKeyInputController.Instanz?.AddKeyCallback(KeyCode.P, NextSong);
            SingleKeyInputController.Instanz?.AddKeyCallback(KeyCode.Z, RandomSong);



            //UI Tab Chain erstellen und Initialisieren
            UITabControl.AddContent("Current Song", new Action(() => {
                DrawCurrentSongUI();
            })).AddContent("Edit Song", new Action(() => {
                DrawEditSongUI();
            })).AddContent("Memory View", new Action(() => {
                DrawMemoryView();
            })).Initialize(); //Init

            //Anschließend Registrieren
            NRPFarMod.Instanz?.Register(UITabControl);
        }

        #region Window UI

        private GUIStyle CurrentPlaying = new GUIStyle() {
            fontSize = 20,
            normal = new GUIStyleState {
                textColor = Color.green
            }
        };

        private GUIContent CurrentPlayingContent = new GUIContent("");




        private void DrawCurrentSongUI() {
            Rect drawArea = new Rect(5, UITabControl.ClientArea.y + 10, windowRect.width - 12, windowRect.height - 60);
            GUI.Box(drawArea, "");
            CurrentPlayingContent.text = $"Current Playing: {AudioClipTrigger.CurrentSong}";
            var size = UITabControl.MeasureString(CurrentPlayingContent, CurrentPlaying.fontSize);
            var space = (drawArea.width - size.x) / 2;

            Rect labelRect = new Rect(space, drawArea.y + 5, size.x, size.y);
            GUI.Label(labelRect, CurrentPlayingContent, CurrentPlaying);
        }

        private void DrawEditSongUI() {

        }

        private Texture2D? memoryView = null;

        private List<(int, double)> lastValues = new();

        private void DrawMemoryView() {
            Rect drawArea = new Rect(5, UITabControl.ClientArea.y + 10, windowRect.width - 12, windowRect.height - 60);
            GUI.Box(drawArea, "");

            if (memoryView == null) {
                memoryView = new Texture2D((int)drawArea.width - 10, (int)drawArea.height - 80);
                if(RefreshMemoryViewTexture(out var newValue)) {
                    lastValues = newValue;
                }        
            }

            GUI.DrawTexture(new Rect(10, UITabControl.ClientArea.y + 20, windowRect.width - 22, windowRect.height - 80), memoryView);

            foreach (var label in lastValues) {
                GUI.Label(new Rect(drawArea.x, drawArea.height -  label.Item1, 200, 30), $"{label.Item2} MB");
            }

        }

        public Color LineColor = new Color(0, 0, 0, 100);

        public Color MemoryViewColor = Color.green;

        public Color MemoryViewBackColor = new Color(30f / 255f, 30f / 255f, 30f / 255f);

        private bool RefreshMemoryViewTexture(out List<(int,double)> graph) {
            graph = new List<(int,double)> ();
            if (memoryView == null) return false;

            for (int x = 0; x < memoryView.width; x++) {
                for (int y = 0; y < memoryView.height; y++) {
                    memoryView.SetPixel(x, y, MemoryViewBackColor);
                }
            }
            if (contentManager.Memory.Count < 2) return false;

            double xSteper = (double)memoryView.width / (contentManager.Memory.Count - 1);

            double minValue = contentManager.Memory.Min();
            double maxValue = contentManager.Memory.Max();

            
            double pufferOffset = ((maxValue - minValue) / 100) * 10;
            minValue -= pufferOffset;
            maxValue += pufferOffset;

            minValue = Math.Floor(minValue);
            maxValue = Math.Ceiling(maxValue);



            double ySteper = (maxValue - minValue) / memoryView.height;

            double lineStep = memoryView.height / 4;
            double lineState = lineStep;

            for (int i = 0; i < 4; i++)  
            {
                for (int x = 0; x < memoryView.width; x++) {
                    memoryView.SetPixel(x, (int)lineState, LineColor);
                }
                double ramValue = minValue + (lineState / ySteper);
                graph.Add(((int)lineState, Math.Round(ramValue,2)));

                lineState += lineStep;
            }




            Vector2Int? lastPoint = null;

            for (int i = 0; i < contentManager.Memory.Count; i++) {

                int xPos = (int)(i * xSteper);
                int yPos = (int)((contentManager.Memory[i] - minValue) / ySteper);

                if (xPos >= memoryView.width - 1) xPos = memoryView.width - 1;
                if (yPos >= memoryView.height - 1) yPos = memoryView.height - 1;

                if (lastPoint == null) {
                    lastPoint = new Vector2Int(xPos, yPos);
                } else {
                    Vector2Int newPoint = new Vector2Int(xPos, yPos);
                    DrawLine(lastPoint.Value, newPoint, MemoryViewColor, ref memoryView);
                    lastPoint = newPoint;
                }
            }

            memoryView.Apply();
            return true;
        }

        /// <summary>
        /// Zeichnet eine Linie nach Bresenhams Algorithmus
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="color"></param>
        /// <param name="texture"></param>
        void DrawLine(Vector2Int start, Vector2Int end, Color color, ref Texture2D texture) {
            int dx = Mathf.Abs(end.x - start.x);
            int dy = Mathf.Abs(end.y - start.y);
            int sx = (start.x < end.x) ? 1 : -1;
            int sy = (start.y < end.y) ? 1 : -1;
            int err = dx - dy;
            while (true) {
                texture.SetPixel(start.x, start.y, color);

                if (start.x == end.x && start.y == end.y) break;
                int e2 = err * 2;
                if (e2 > -dy) {
                    err -= dy;
                    start.x += sx;
                }
                if (e2 < dx) {
                    err += dx;
                    start.y += sy;
                }
            }
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
                windowRect = GUI.Window(0, windowRect, (GUI.WindowFunction)DrawWindow, "NRPFarMod by Farliam & iLollek - 2024", GUI.skin.window);
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
                if (RefreshMemoryViewTexture(out var newValue)) {
                    lastValues = newValue;
                }
            }
        }

        private void PreviousSong() {
            if (CheckGodConstant()) {
                contentManager.LoadPrevSong(godConstant!.musicSource);
                if (RefreshMemoryViewTexture(out var newValue)) {
                    lastValues = newValue;
                }
            }
        }

        private void RandomSong() {
            if (CheckGodConstant()) {
                contentManager.LoadRandomSong(godConstant!.musicSource);
                if (RefreshMemoryViewTexture(out var newValue)) {
                    lastValues = newValue;
                }
            }
        }

        #endregion

        private bool CheckGodConstant() {
            if (godConstant == null) {
                godConstant = GodConstant.Instance; 
            return !(godConstant == null);
        }

        public override void OnApplicationQuit() {
            contentManager.Dispose();
        }
    }
}
