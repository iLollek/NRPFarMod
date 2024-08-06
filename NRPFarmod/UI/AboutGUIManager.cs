using MelonLoader;
using NRPFarmod.ContentManager;
using NRPFarmod.MelonCall;
using NRPFarmod.UIHelper;
using System.Collections;
using UnityEngine;


namespace NRPFarmod.UI
{


    public sealed class AboutGUIManager : MelonCaller, IGUIManager {

        private bool OnUpdateLock = false;
        private Vector2 ClientRect = Vector2.zero;
        private bool NeedInit = true;
        private Rect WindowRect = Rect.zero;
        private Texture2D? GithubNormal = null;
        private Texture2D? GithubSelected = null;
        private Texture2D? ILollekTexture = null;
        private Texture2D? FarliamTexture = null;
        private Texture2D? YTTexture = null;
        private Texture2D? YTTexture_sel = null;
        private int AboutBoxWidth = 0;
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

        private AboutGUIManager() { }

        public AboutGUIManager(Rect windowRect, Vector2 clientRect) {
            WindowRect = windowRect;
            ClientRect = clientRect;
        }

        public void DrawUI() {

            if (NeedInit) FirstInit();

            Rect drawArea = new Rect(5, ClientRect.y + 10, WindowRect.width - 12, WindowRect.height - 60);
            GUI.Box(LeftBoxRect, "");
            GUI.Box(RightBoxRect, "");

            GUI.DrawTexture(FarliamRect, FarliamTexture);
            GUI.Label(FarliamLabelRect, FarliamContent, NameStyle);

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

        public override void OnUpdate() {
            if (OnUpdateLock) return;
            if (GithubNormal == null ||
                GithubSelected == null ||
                ILollekTexture == null ||
                FarliamTexture == null ||
                YTTexture == null ||
                YTTexture_sel == null) {
                OnUpdateLock = true;
                MelonCoroutines.Start(LoadTextures());
            }
        }

        public void FirstInit() {
            AboutBoxWidth = (int)((WindowRect.width - ClientRect.x - 30) / 2);
            LeftBoxRect = new Rect(10, ClientRect.y + 10, AboutBoxWidth, WindowRect.height - 65);
            RightBoxRect = new Rect(LeftBoxRect.x + LeftBoxRect.width + 10, ClientRect.y + 10, AboutBoxWidth, WindowRect.height - 65);
            LeftGithubRect = new Rect(LeftBoxRect.x + 15, WindowRect.height - 100, 60, 60);
            FarliamRect = new Rect(LeftBoxRect.x + 10, LeftBoxRect.y + 10, 70, 70);
            var width = UITabControl.MeasureString(FarliamContent, NameStyle.fontSize);
            FarliamLabelRect = new Rect(FarliamRect.x + FarliamRect.width + 25, FarliamRect.y + 25, width.x, width.y);
            ILollekRect = new Rect(RightBoxRect.x + 10, RightBoxRect.y + 10, 70, 70);
            width = UITabControl.MeasureString(ILollekContent, NameStyle.fontSize);
            ILollekLabelRect = new Rect(ILollekRect.x + ILollekRect.width + 25, ILollekRect.y + 25, width.x, width.y);
            RightGithubRect = new Rect(RightBoxRect.x + 15, LeftGithubRect.y, 60, 60);
            LeftYTRect = new Rect(LeftGithubRect.x + LeftGithubRect.width + 15, LeftGithubRect.y, 60, 60);
            RightYTRect = new Rect(RightGithubRect.width + RightGithubRect.x + 15, RightGithubRect.y, 60, 60);
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
            NeedInit = false;
        }

        public IEnumerator LoadTextures() {
            yield return new WaitForSeconds(0.1f);
            if (GithubNormal == null) TextureMananger.CreateTexture(ref GithubNormal, Properties.Resources.GithubRound);
            if (GithubSelected == null) TextureMananger.CreateTexture(ref GithubSelected, Properties.Resources.GithubRound_sel);
            if (ILollekTexture == null) TextureMananger.CreateTexture(ref ILollekTexture, Properties.Resources.iLollekRound);
            if (FarliamTexture == null) TextureMananger.CreateTexture(ref FarliamTexture, Properties.Resources.FarliamRound);
            if (YTTexture == null) TextureMananger.CreateTexture(ref YTTexture, Properties.Resources.YT);
            if (YTTexture_sel == null) TextureMananger.CreateTexture(ref YTTexture_sel, Properties.Resources.YT_Selected);
            OnUpdateLock = false;
        }

    }
}
