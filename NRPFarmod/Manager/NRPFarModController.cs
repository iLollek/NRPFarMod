using MelonLoader;
using UnityEngine;
using NRPFarmod.MelonCall;
using NRPFarmod.ContentManager;
using Il2Cpp;
using NRPFarmod.CustomUnityScripts;
using NRPFarmod.UIHelper;
using System.Collections;

namespace NRPFarmod {
    public class NRPFarModController : MelonCaller {

        #region Private

        private GUIStyle CurrentPlaying = new GUIStyle() {
            fontSize = 20,
            normal = new GUIStyleState {
                textColor = Color.green
            }
        };

        //Objecte für SongUI
        private bool EditNeedInit = true;

        private GUIStyle EditSongStyle = new GUIStyle() {
            fontSize = 15,
            normal = new GUIStyleState {
                textColor = Color.green
            }
        };

        private GUIStyle EditSongBoxStyle = new GUIStyle() {
            fontSize = 15,
            alignment = TextAnchor.UpperCenter,
            normal = new GUIStyleState {
                textColor = Color.green
            }
        };

        private GUIContent CurrentPlayingContent = new GUIContent("");
        private GUIContent DisplayLabel = new GUIContent("Display: ");
        private Rect DisplayLabelRect = Rect.zero;
        private GUIContent PitchLabel = new GUIContent("Pitch: -10   ");
        private Rect PitchLabelRect = Rect.zero;
        private GUIContent VolumeLabel = new GUIContent("Volume: -10   ");
        private Rect VolumeLabelRect = Rect.zero;
        private GUIContent SpeedLabel = new GUIContent("ReverbZoneMix: -10.00 ");
        private Rect SpeedLabelRect = Rect.zero;
        private Rect EditDisplayRect = Rect.zero;
        private Rect EditPitchRect = Rect.zero;
        private Rect EditVolumeRect = Rect.zero;
        private Rect EditSpeedRect = Rect.zero;
        private Rect ResetButtonRect = Rect.zero;
        private Texture2D? memoryView = null;
        private List<(int, double)> lastValues = new();
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
        private bool AboutNeedInit = true;
        private Texture2D? GithubNormal = null;
        private Texture2D? GithubSelected = null;
        private Texture2D? ILollekTexture = null;
        private Texture2D? FarliamTexture = null;
        private Texture2D? YTTexture = null;
        private Texture2D? YTTexture_sel = null;
        private Texture2D? PlayTexture = null;
        private Texture2D? StopTexture = null;
        private Texture2D? ForwardTexture = null;
        private Texture2D? BackTexture = null;
        private Texture2D? PlaylistMode = null;
        private Texture2D? PlaylistModeSelected = null;
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


            //UI Chain erstellen und Initialisieren
            UITabControl.AddContent("Current Song", new Action(() => {
                DrawCurrentSongUI();
            })).AddContent("Edit Song", new Action(() => {
                DrawEditSongUI();
            })).AddContent("Memory View", new Action(() => {
                DrawMemoryView();
            })).AddContent("About", new Action(() => {
                DrawAbaoutUI();
            })).Initialize(); //Init

            //Anschließend Registrieren
            NRPFarMod.Instanz?.Register(UITabControl);
        }

        private void AudioClipTrigger_PlayListTriggerEvent(object? sender, PlayListTriggerRaise e) {
            MelonLogger.Msg(ConsoleColor.Green,$"PlaylistTrigger Event!");
            RandomSong();
        }

        private bool CheckGodConstant() {
            if (godConstant == null) {
                godConstant = GodConstant.Instance;
            }
            return !(godConstant == null);
        }

        public override void OnApplicationQuit() {
            contentManager.Dispose();
        }
        #endregion

        #region Window UI

        #region Window

        public override void OnSceneWasLoaded(int buildIndex, string sceneName) {
            MelonLogger.Msg(ConsoleColor.Yellow, $"New Scene loaded: {sceneName}");
            CurrentSongNeedInit = true;
            CurSongTextureLoading = false;
            AboutNeedInit = true;
            TextureLoading = false;
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
        #endregion

        #region CurrentSongUI

        private Rect BackRect = Rect.zero;
        private Rect StopRect = Rect.zero;
        private Rect PlayRect = Rect.zero;
        private Rect NextRect = Rect.zero;
        private Rect PlayListRect = Rect.zero;

        private bool CurrentSongNeedInit = true;
        private bool CurSongTextureLoading = false;

        public IEnumerator LoadCurrentSongTextures(Action? callback) {
            yield return new WaitForSeconds(0.1f);
            TextureMananger.CreateTexture(ref PlayTexture, Properties.Resources.Play);
            TextureMananger.CreateTexture(ref StopTexture, Properties.Resources.Stop);
            TextureMananger.CreateTexture(ref BackTexture, Properties.Resources.Back);
            TextureMananger.CreateTexture(ref ForwardTexture, Properties.Resources.Forward);
            TextureMananger.CreateTexture(ref PlaylistMode, Properties.Resources.PayList);
            TextureMananger.CreateTexture(ref PlaylistModeSelected, Properties.Resources.PlayListModeAktive);
            callback?.Invoke();
        }
        private void DrawCurrentSongUI() {

            Rect drawArea = new Rect(5, UITabControl.ClientArea.y + 10, windowRect.width - 12, windowRect.height - 60);


            if (CurrentSongNeedInit && !CurSongTextureLoading) {
                CurSongTextureLoading = true;
                MelonCoroutines.Start(LoadCurrentSongTextures(new Action(() => {
                    CurSongTextureLoading = true;
                })));
            } else if (CurrentSongNeedInit && CurSongTextureLoading) {

                int buttonWidth = (int)((drawArea.width - 50) / 4);
                BackRect = new Rect(drawArea.x + 10, drawArea.y + 60, buttonWidth, 50);
                StopRect = new Rect(BackRect.x + BackRect.width + 10, BackRect.y, buttonWidth, 50);
                PlayRect = new Rect(StopRect.x + StopRect.width + 10, StopRect.y, buttonWidth, 50);
                NextRect = new Rect(PlayRect.x + PlayRect.width + 10, PlayRect.y , buttonWidth, 50);
                PlayListRect = new Rect(PlayRect.x - (buttonWidth / 2), PlayRect.y + 60 , buttonWidth, 50);

                CurrentSongNeedInit = false;
            }

            GUI.Box(drawArea, "");
            CurrentPlayingContent.text = $"Current Playing: {AudioClipTrigger.CurrentSong}";
            var size = UITabControl.MeasureString(CurrentPlayingContent, CurrentPlaying.fontSize);
            var space = (drawArea.width - size.x) / 2;

            Rect labelRect = new Rect(space, drawArea.y + 25, size.x, size.y);
            GUI.Label(labelRect, CurrentPlayingContent, CurrentPlaying);

            if (GUI.Button(BackRect, new GUIContent("", BackTexture, "Previous Song"))) {
                PreviousSong();
            }
            if (GUI.Button(StopRect, new GUIContent("", StopTexture, "Stop Song"))) {
                godConstant?.musicSource.Stop();
            }
            if (GUI.Button(PlayRect, new GUIContent("", PlayTexture, "Play Song"))) {
                if(godConstant?.musicSource == null) {
                    NextSong();
                } else {
                    godConstant?.musicSource.Play();
                }
            }
            if (GUI.Button(NextRect, new GUIContent("", ForwardTexture, "Next Song"))) {
                NextSong();
            }
            if (AudioClipTrigger.PlayListMode) {
                if (GUI.Button(PlayListRect, new GUIContent("", PlaylistModeSelected , "Playlistmode : OFF"))) {
                    AudioClipTrigger.PlayListMode = false;
                }
            } else {
                if (GUI.Button(PlayListRect, new GUIContent("", PlaylistMode, "Playlistmode: ON"))) {
                    AudioClipTrigger.PlayListMode = true;
                }
            }
         
        }

        public void DrawWindow(int windowID) {
            UITabControl.DrawWindowData(windowID);
            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }

        #endregion

        #region EditSongUI

        private Rect DefaultButtonRect = Rect.zero;

        private void DrawEditSongUI() {

            Rect drawArea = new Rect(5, UITabControl.ClientArea.y + 10, windowRect.width - 12, windowRect.height - 60);
            GUI.Box(drawArea, $"Edit - {contentManager.CurrentClipInfo.UI_Display}", EditSongBoxStyle);

            if (EditNeedInit) {
                float maxX = 0;
                var dlr = UITabControl.MeasureString(DisplayLabel, EditSongStyle.fontSize); maxX = maxX < dlr.x ? dlr.x : maxX;
                DisplayLabelRect = new Rect(drawArea.x + 5, drawArea.y + 30, dlr.x, dlr.y);
                int ySteps = (int)(dlr.y + 15); //10 Margin 
                dlr = UITabControl.MeasureString(PitchLabel, EditSongStyle.fontSize); maxX = maxX < dlr.x ? dlr.x : maxX;
                PitchLabelRect = new Rect(DisplayLabelRect.x, DisplayLabelRect.y + ySteps, dlr.x, dlr.y);
                dlr = UITabControl.MeasureString(VolumeLabel, EditSongStyle.fontSize); maxX = maxX < dlr.x ? dlr.x : maxX;
                VolumeLabelRect = new Rect(DisplayLabelRect.x, PitchLabelRect.y + ySteps, dlr.x, dlr.y);
                dlr = UITabControl.MeasureString(SpeedLabel, EditSongStyle.fontSize); maxX = maxX < dlr.x ? dlr.x : maxX;
                SpeedLabelRect = new Rect(DisplayLabelRect.x, VolumeLabelRect.y + ySteps, dlr.x, dlr.y);
                EditNeedInit = false;
                int controlLengths = (int)(drawArea.width - 30 - maxX);
                int startXPos = (int)(drawArea.width - controlLengths);
                EditDisplayRect = new Rect(DisplayLabelRect.x + startXPos, DisplayLabelRect.y - 5, controlLengths - 10, DisplayLabelRect.height);
                EditPitchRect = new Rect(DisplayLabelRect.x + startXPos, EditDisplayRect.y + EditDisplayRect.height + 23, controlLengths - 10, PitchLabelRect.height);
                EditVolumeRect = new Rect(DisplayLabelRect.x + startXPos, EditPitchRect.y + EditPitchRect.height + 17, controlLengths - 10, VolumeLabelRect.height);
                EditSpeedRect = new Rect(DisplayLabelRect.x + startXPos, EditVolumeRect.y + EditVolumeRect.height + 17, controlLengths - 10, SpeedLabelRect.height);
                ResetButtonRect = new Rect(drawArea.x + 5, windowRect.height - 40, 200, 30);
                DefaultButtonRect = new Rect(ResetButtonRect.x + ResetButtonRect.width + 5,ResetButtonRect.y,ResetButtonRect.width,ResetButtonRect.height);
            }

            PitchLabel.text = $"Pitch: {Math.Round(contentManager.CurrentClipInfo.Pitch, 2)}";
            VolumeLabel.text = $"Volume: {Math.Round(contentManager.CurrentClipInfo.Volume, 2)}";
            SpeedLabel.text = $"ReverbZoneMix: {Math.Round(contentManager.CurrentClipInfo.ReverbZoneMix, 2)}";

            GUI.Label(DisplayLabelRect, DisplayLabel, EditSongStyle);
            GUI.Label(PitchLabelRect, PitchLabel, EditSongStyle);
            GUI.Label(VolumeLabelRect, VolumeLabel, EditSongStyle);
            GUI.Label(SpeedLabelRect, SpeedLabel, EditSongStyle);

            var clip = contentManager.CurrentClipInfo;

            clip.UI_Display = GUI.TextField(EditDisplayRect, contentManager.CurrentClipInfo.UI_Display);
            clip.Pitch = GUI.HorizontalSlider(EditPitchRect, contentManager.CurrentClipInfo.Pitch, -2f, 2f);
            clip.Volume = GUI.HorizontalSlider(EditVolumeRect, contentManager.CurrentClipInfo.Volume, -10f, 10);
            clip.ReverbZoneMix = GUI.HorizontalSlider(EditSpeedRect, contentManager.CurrentClipInfo.ReverbZoneMix, 0f, 5f);

            SetSongValues(clip.Volume, clip.Pitch, clip.ReverbZoneMix);

            if (GUI.Button(ResetButtonRect, "Reset")) {
                contentManager.ResetCurrentSongInfo();
            }

            if (GUI.Button(DefaultButtonRect, "Default")) {
                SetSongDefaultValues();
            }
        }

        private void SetSongValues(float volume, float pitch, float reverbZoneMix) {
            if (godConstant == null) return;
            godConstant.musicSource.pitch = pitch;
            godConstant.musicSource.reverbZoneMix = reverbZoneMix;
            godConstant.musicVol_Target = volume;
        }

        private void SetSongDefaultValues() {
            var clip = contentManager.CurrentClipInfo;
            clip.Volume = 0;
            clip.Pitch = 1;
            clip.ReverbZoneMix = 1;
            SetSongValues(0, 1, 1);
        }
        #endregion

        #region MemoryViewUI

        private void DrawMemoryView() {
            Rect drawArea = new Rect(5, UITabControl.ClientArea.y + 10, windowRect.width - 12, windowRect.height - 60);
            GUI.Box(drawArea, "");
            if (memoryView == null) {
                memoryView = new Texture2D((int)drawArea.width - 10, (int)drawArea.height - 80);
                if (RefreshMemoryViewTexture(out var newValue)) {
                    lastValues = newValue;
                }
            }
            GUI.DrawTexture(new Rect(10, UITabControl.ClientArea.y + 20, windowRect.width - 22, windowRect.height - 80), memoryView);
            foreach (var label in lastValues) {
                GUI.Label(new Rect(drawArea.x + 5, drawArea.height - label.Item1, 200, 30), $"{label.Item2} MB");
            }
        }

        public Color LineColor = new Color(0, 0, 0, 100);
        public Color MemoryViewColor = Color.green;
        public Color MemoryViewBackColor = new Color(30f / 255f, 30f / 255f, 30f / 255f);

        private bool RefreshMemoryViewTexture(out List<(int, double)> graph) {
            graph = new List<(int, double)>();
            if (memoryView == null) return false;
            for (int x = 0; x < memoryView.width; x++) {
                for (int y = 0; y < memoryView.height; y++) {
                    memoryView.SetPixel(x, y, MemoryViewBackColor);
                }
            }
            if (contentManager.Memory.Count < 2) return false;

            int stepPrec = contentManager.Memory.Count > (memoryView.width / 10) ? (int)(memoryView.width / 10) : contentManager.Memory.Count;
            double xSteper = (double)memoryView.width / stepPrec;

            double minValue = contentManager.Memory.Min();
            double maxValue = contentManager.Memory.Max();

            MelonLogger.Msg($"MaxRam: {maxValue}");

            double pufferOffset = ((maxValue - minValue) / 100) * 10;
            minValue -= pufferOffset;
            maxValue += pufferOffset;

            minValue = Math.Floor(minValue);
            maxValue = Math.Ceiling(maxValue);

            double ySteper = (maxValue - minValue) / memoryView.height;

            double lineStep = memoryView.height / 4;
            double lineState = lineStep;

            for (int i = 0; i < 4; i++) {
                for (int x = 0; x < memoryView.width; x++) {
                    memoryView.SetPixel(x, (int)lineState, LineColor);
                }
                double ramValue = minValue + (lineState * ySteper);
                graph.Add(((int)lineState, Math.Round(ramValue, 2)));

                lineState += lineStep;
            }

            Vector2Int? lastPoint = null;

            int ix = contentManager.Memory.Count > (memoryView.width / 10) ? contentManager.Memory.Count - (memoryView.width / 10) : 0;

            for (int i = ix; i < contentManager.Memory.Count; i++) {

                int xPos = (int)((i - ix) * xSteper);
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

        #endregion

        #region AboutUI 

        private int AboutBoxWidth = 0;
        private bool TextureLoading = false;
        private bool TextureLoadFin = false;
        private Rect LeftBoxRect = Rect.zero;
        private Rect RightBoxRect = Rect.zero;
        private GUIStyle GitStyle = new GUIStyle();
        private GUIStyle YTStyle = new GUIStyle();
        private Rect LeftGithubRect = Rect.zero;
        private Rect RightGithubRect = Rect.zero;
        private Rect LeftYTRect = Rect.zero;
        private Rect RightYTRect = Rect.zero;

        private Rect FarliamRect = Rect.zero;
        private GUIContent FarliamContent = new GUIContent("Farliam");
        private Rect FarliamLabelRect = Rect.zero;

        private Rect ILollekRect = Rect.zero;
        private GUIContent ILollekContent = new GUIContent("iLollek");
        private Rect ILollekLabelRect = Rect.zero;

        private GUIStyle NameStyle = new GUIStyle() {
            fontSize = 20,
            normal = new GUIStyleState {
                textColor = Color.green
            }
        };


        public IEnumerator LoadTextures(Action? Callback) {
            yield return new WaitForSeconds(0.1f);
            TextureMananger.CreateTexture(ref GithubNormal, Properties.Resources.GithubRound);
            TextureMananger.CreateTexture(ref GithubSelected, Properties.Resources.GithubRound_sel);
            TextureMananger.CreateTexture(ref FarliamTexture, Properties.Resources.FarliamRound);
            TextureMananger.CreateTexture(ref ILollekTexture, Properties.Resources.iLollekRound);
            TextureMananger.CreateTexture(ref YTTexture, Properties.Resources.YT);
            TextureMananger.CreateTexture(ref YTTexture_sel, Properties.Resources.YT_Selected);
            Callback?.Invoke();
        }

        private void DrawAbaoutUI() {
            if (AboutNeedInit && !TextureLoading) {
                TextureLoading = true;
                MelonCoroutines.Start(LoadTextures(new Action(() => {
                    TextureLoadFin = true;
                })));
            }else if(AboutNeedInit && TextureLoadFin){
                AboutBoxWidth = (int)((windowRect.width - UITabControl.ClientArea.x - 30) / 2);
                LeftBoxRect = new Rect(10, UITabControl.ClientArea.y + 10, AboutBoxWidth, windowRect.height - 65);
                RightBoxRect = new Rect(LeftBoxRect.x + LeftBoxRect.width + 10, UITabControl.ClientArea.y + 10, AboutBoxWidth, windowRect.height - 65);
                LeftGithubRect = new Rect(LeftBoxRect.x + 15, windowRect.height-100, 60, 60);

                FarliamRect = new Rect(LeftBoxRect.x + 10, LeftBoxRect.y + 10, 70, 70);
                var width = UITabControl.MeasureString(FarliamContent, NameStyle.fontSize);
                FarliamLabelRect = new Rect(FarliamRect.x + FarliamRect.width + 25, FarliamRect.y + 25,width.x,width.y);

                ILollekRect = new Rect(RightBoxRect.x + 10, RightBoxRect.y + 10, 70, 70);
                width = UITabControl.MeasureString(ILollekContent, NameStyle.fontSize);
                ILollekLabelRect = new Rect(ILollekRect.x + ILollekRect.width + 25, ILollekRect.y + 25, width.x,width.y);

                RightGithubRect = new Rect(RightBoxRect.x + 15, LeftGithubRect.y, 60, 60);

                LeftYTRect = new Rect(LeftGithubRect.x + LeftGithubRect.width + 15, LeftGithubRect.y, 60, 60);
                RightYTRect = new Rect(RightGithubRect.width + RightGithubRect.x + 15, RightGithubRect.y, 60, 60);
;
                GitStyle = new GUIStyle(GUI.skin.button);
                GitStyle.normal.background = GithubNormal;
                GitStyle.hover.background = GithubSelected;
                GitStyle.active.background = GithubSelected;
                GitStyle.border.left = 0;
                GitStyle.border.top = 0;
                GitStyle.border.right = 0;
                GitStyle.border.bottom = 0;

                YTStyle = new GUIStyle(GUI.skin.button);
                YTStyle.normal.background = YTTexture;
                YTStyle.hover.background = YTTexture_sel;
                YTStyle.active.background = YTTexture_sel;
                YTStyle.border.left = 0;
                YTStyle.border.top = 0;
                YTStyle.border.right = 0;
                YTStyle.border.bottom = 0;

                AboutNeedInit = false;
            }
   
            if (GithubNormal == null || GithubSelected == null) return;
            Rect drawArea = new Rect(5, UITabControl.ClientArea.y + 10, windowRect.width - 12, windowRect.height - 60);
            GUI.Box(LeftBoxRect, "");
            GUI.Box(RightBoxRect, "");

            GUI.DrawTexture(FarliamRect, FarliamTexture);
            GUI.Label(FarliamLabelRect,FarliamContent, NameStyle);

            GUI.DrawTexture(ILollekRect, ILollekTexture);
            GUI.Label(ILollekLabelRect, ILollekContent, NameStyle);

            if (GUI.Button(LeftGithubRect, GUIContent.none, GitStyle)) {
                Application.OpenURL("https://github.com/Farliam93");
            }
            if (GUI.Button(RightGithubRect, GUIContent.none, GitStyle)) {
                Application.OpenURL("https://github.com/iLollek");
            }
            if (GUI.Button(LeftYTRect, GUIContent.none, YTStyle)) {
                Application.OpenURL("https://www.youtube.com/channel/UC0PvCffIrl3o6KjRAx7oK5Q");
            }
            if (GUI.Button(RightYTRect, GUIContent.none, YTStyle)) {
                Application.OpenURL("https://www.youtube.com/@ilollek");
            }
        }

        #endregion

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

    }
}
