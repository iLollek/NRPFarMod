using MelonLoader;
using NRPFarmod.MelonCall;
using System.Net.Http.Headers;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

namespace NRPFarmod.UIHelper {



    /// <summary>
    /// Optionen für die "Header"
    /// </summary>
    public enum TabControlHeaderStyle {
        Default = 0,
        Expand = 1,
        FitToContent = 2
    }


    /// <summary>
    /// Stellt Methoden bereit um ein TabControl ähnliches Steuerelement zu erzeigen
    /// </summary>
    public sealed class UITabControl : MelonCaller {


        #region Public Propertys
        /// <summary>
        /// Gibt ein offset für den Contentbereich zurück
        /// </summary>
        public Vector2 ClientArea { get; private set; }
        /// <summary>
        /// Gibt an ob der TabContainer sichtbar ist
        /// </summary>
        public bool Visibility { get; set; } = false;
        /// <summary>
        /// Mindesthöhe für den Header
        /// </summary>
        public int MinHeight { get; set; } = 5;
        /// <summary>
        /// Header Darstellung
        /// </summary>
        public TabControlHeaderStyle HeaderStyle { get; set; } = TabControlHeaderStyle.Expand;
        /// <summary>
        /// Header Selected Foreground
        /// </summary>
        public Color Header_Selected_Foreground { get; set; } = Color.blue;
        /// <summary>
        /// Header Selected Background
        /// </summary>
        public Color Header_Selected_Background { get; set; } = Color.white;
        /// <summary>
        /// Header Default Foreground
        /// </summary>
        public Color Header_Foreground { get; set; } = Color.black;
        /// <summary>
        /// Header Default Background
        /// </summary>
        public Color Header_Background { get; set; } = Color.gray;
        #endregion

        #region Hilfe
        /// <summary>
        /// Enthält weitere Custom Gestaltungsoptionen
        /// </summary>
        public class UITabContentInformation {
            public int FontSize { get; set; } = 15;
            public Color Normal_TextColor { get; set; } = Color.white;
            public Color Hover_TextColor { get; set; } = Color.white;
            public Color Active_TextColor { get; set; } = Color.green;
            public Color Normal_BackColor { get; set; } = new Color(63f / 255f, 64f /255f,70f /255f,255f);
            public Color Hover_BackColor { get; set; } = new Color(63f / 255f, 64f / 255f, 70f / 255f, 255f);
            public Color Active_BackColor { get; set; } = Color.black;
            public Color Selected_Normal_TextColor { get; set; } = Color.green;
            public Color Selected_Hover_TextColor { get; set; } = Color.green;
            public Color Selected_Active_TextColor { get; set; } = Color.green;
            public Color Selected_Normal_BackColor { get; set; } = new Color(31f / 255f, 31f / 255, 31f/255,255f);
            public Color Selected_Hover_BackColor { get; set; } = new Color(31f / 255f, 31f / 255, 31f / 255, 255f);
            public Color Selected_Active_BackColor { get; set; } = new Color(31f / 255f, 31f / 255, 31f / 255, 255f);
            public static UITabContentInformation Empty { get =>  new UITabContentInformation(); }
        }
        /// <summary>
        /// Speichert Informationen über eine Page
        /// </summary>
        internal class UITabChild {
            public UITabChild(string name, Action addPageContent) {
                Name = name;
                AddPageContent = addPageContent;
            }

            public string Name { get; set; }
            public Action AddPageContent { get; set; }
            public UITabContentInformation Information { get; set; } = UITabContentInformation.Empty;
            public Rect Header { get; set; } = Rect.zero;

            private GUIStyle? Selected = null;

            private GUIStyle? Default = null;

            public GUIStyle GetSelected {
                get {
                    if (Selected == null) {
                        Selected = new GUIStyle(GUI.skin.button);
                        Selected.fontSize = Information.FontSize;
                        Selected.normal.textColor = Information.Selected_Normal_TextColor;
                        Selected.hover.textColor = Information.Selected_Hover_TextColor;
                        Selected.active.textColor = Information.Selected_Active_TextColor;
                        Texture2D normalTex = new Texture2D(1, 1);
                        normalTex.SetPixel(0, 0, Information.Selected_Normal_BackColor);
                        normalTex.Apply();
                        Texture2D hoverTex = new Texture2D(1, 1);
                        hoverTex.SetPixel(0, 0, Information.Selected_Hover_BackColor);
                        hoverTex.Apply();
                        Texture2D activeTex = new Texture2D(1, 1);
                        activeTex.SetPixel(0, 0, Information.Selected_Active_BackColor);
                        activeTex.Apply();
                        Selected.normal.background = normalTex;
                        Selected.hover.background = hoverTex;
                        Selected.active.background = activeTex;
                        return Selected;
                    } else {
                        return Selected;
                    }
                }                   
            }

            public GUIStyle GetDefault {
                get {
                    if (Default == null) {
                        Default = new GUIStyle(GUI.skin.button);
                        Default.fontSize = Information.FontSize;
                        Default.normal.textColor = Information.Normal_TextColor;
                        Default.hover.textColor = Information.Hover_TextColor;
                        Default.active.textColor = Information.Active_TextColor;
                        Texture2D normalTex = new Texture2D(1, 1);
                        normalTex.SetPixel(0, 0, Information.Normal_BackColor);
                        normalTex.Apply();
                        Texture2D hoverTex = new Texture2D(1, 1);
                        hoverTex.SetPixel(0, 0, Information.Normal_BackColor);
                        hoverTex.Apply();
                        Texture2D activeTex = new Texture2D(1, 1);
                        activeTex.SetPixel(0, 0, Information.Normal_BackColor);
                        activeTex.Apply();
                        Default.normal.background = normalTex;
                        Default.hover.background = hoverTex;
                        Default.active.background = activeTex;
                        return Default;
                    } else {
                        return Default;
                    }
                }
            }

        }
        #endregion

        #region Private
        /// <summary>
        /// Measure String funktioniert nur innerhalb der OnGUI
        /// </summary>
        private bool NeedInit = true;
        /// <summary>
        /// Bestimmt ob die UI bereits gezeichnet werden kann
        /// </summary>
        private bool CanDraw = false;
        /// <summary>
        /// Speicher für die Pages
        /// </summary>
        private List<UITabChild> pages = new();
        /// <summary>
        /// Current Selected PAge
        /// </summary>
        private int CurrentPage = 0;
        /// <summary>
        /// Full Draw Area
        /// </summary>
        public Rect ContentDrawAreal { get; set; }
        /// <summary>
        /// Window Draw Callback
        /// </summary>
        public Action<int>? DoWindow { get; private set; }   
        /// <summary>
        /// Offset Vectorr
        /// </summary>
        public Vector2 Offset { get; private set; }
        /// <summary>
        /// Size
        /// </summary>
        public Vector2 Size { get; private set; }
        #endregion

        #region Konstruktor
        /// <summary>
        /// Private ctor Lock
        /// </summary>
        private UITabControl() {}
        /// <summary>
        /// Erstellt ein neues Tabcontrol
        /// </summary>
        /// <param name="rect">Tabgröße</param>
        public UITabControl(ref Rect rectOrginal, Vector2 offset, Vector2 size) {
            ContentDrawAreal = rectOrginal;
            Offset = offset;    
            Size = size;
        }
        #endregion

        #region Init
        /// <summary>
        /// Method Chaining - Füge neue Pages hinzu
        /// </summary>
        /// <param name="Header"></param>
        /// <param name="Content"></param>
        /// <returns></returns>
        public UITabControl AddContent(string Header, Action Content, UITabContentInformation? optional = null) {
            var child = new UITabChild(Header, Content);
            if(optional != null) child.Information = optional;
            pages.Add(child);
            return this;
        }

        /// <summary>
        /// Muss einmalig ausgeführt werden um die Dimensionen zu berechnen
        /// </summary>

        public void Initialize() {
            if (pages.Count <= 0) return;
            //Berechnen der Ausrichtung vom Header
            DoWindow = DrawWindowData;
            NeedInit = true;
        }

        /// <summary>
        /// Berechnen abhängig von <see cref="TabControlHeaderStyle"/> die Header größe
        /// </summary>
        /// <returns></returns>
        private void CalculateHeaderPosition() {
            //Nimmt den gesamten Raum und teilt diesen gleichmäßig auf
            if(HeaderStyle == TabControlHeaderStyle.Default || HeaderStyle == TabControlHeaderStyle.Expand) {

                int width = (int)((Size.x - Offset.x) / pages.Count);
                int rest =  (int)((Size.x - Offset.x) % width);
                var measure = MeasureString(new GUIContent("Hello World"), pages[0].Information.FontSize);
                int height = (int)(measure.y < MinHeight ? MinHeight : measure.y);

                ClientArea = new Vector2(Offset.x, Offset.y + height);  

                int xStep = (int)Offset.x;
                for(int i = 0; i < pages.Count; i++) {
                    var page = pages[i];
                    if(i == pages.Count - 1 && rest > 0) {
                        page.Header = new Rect(xStep, Offset.y, width + rest, height);
                    } else {
                        page.Header = new Rect(xStep, Offset.y, width, height);
                    }
                    xStep += width;
                }
            }else if(HeaderStyle == TabControlHeaderStyle.FitToContent) {
                int xStep = (int)Offset.x; ;
                for (int i = 0;i < pages.Count; i++) {
                    var page = pages[i];
                    var measure = MeasureString(new GUIContent(page.Name), page.Information.FontSize);
                    int height = (int)(measure.y < MinHeight ? MinHeight : measure.y);
                    page.Header = new Rect(xStep,Offset.y,measure.x, height);
                    xStep += (int)measure.x;
                }
            }
        }
        #endregion

        #region Static Helper 
        /// <summary>
        /// Berechnet die Dimensionen eines GUIContent
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static Vector2 MeasureString(GUIContent content, int fontSize) {
            try {
                GUIStyle style = new GUIStyle(GUI.skin.label);
                style.fontSize = fontSize;
                return style.CalcSize(content);
            } catch (Exception e) {
                MelonLogger.Error(e);
                return Vector2.zero;
            }
        }

        /// <summary>
        /// Überprüft ob die Rechtecke miteinander überschneiden
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool IntersectWith(Rect left, Rect right) {
            bool horizontalOverlap = left.xMin < right.xMax && left.xMax > right.xMin;
            bool verticalOverlap = left.yMin < right.yMax && left.yMax > right.yMin;
            return horizontalOverlap && verticalOverlap;
        }
        #endregion

        #region DRAW
        public void DrawWindowData(int key) {
            if (NeedInit) {
                CalculateHeaderPosition();
                CanDraw = true;
                NeedInit = false;
            }
            if (!CanDraw || !Visibility) return;
            for (int i = 0; i < pages.Count; i++) {
                var page = pages[i];
                if (i == CurrentPage) {
                    if (GUI.Button(page.Header, new GUIContent(page.Name), page.GetSelected)) { }
                    page.AddPageContent?.Invoke();
                } else {
                    if (GUI.Button(page.Header, new GUIContent(page.Name), page.GetDefault)) {
                        CurrentPage = i;
                        page.AddPageContent?.Invoke();
                    }
                }
            }
        }
        #endregion
    }
}
