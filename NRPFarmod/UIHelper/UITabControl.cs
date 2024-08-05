using MelonLoader;
using NRPFarmod.MelonCall;
using System.Net.Http.Headers;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

namespace NRPFarmod.UIHelper {



    /// <summary>
    /// Options for the "Headers"
    /// </summary>
    public enum TabControlHeaderStyle {
        Default = 0,
        Expand = 1,
        FitToContent = 2
    }


    /// <summary>
    /// Provides methods to create a TabControl-like control
    /// </summary>
    public sealed class UITabControl : MelonCaller {


        #region Public Propertys
        /// <summary>
        /// Returns an offset for the content area
        /// </summary>
        public Vector2 ClientArea { get; private set; }
        /// <summary>
        /// Indicates whether the TabContainer is visible
        /// </summary>
        public bool Visibility { get; set; } = false;
        /// <summary>
        /// Minimum height for the header
        /// </summary>
        public int MinHeight { get; set; } = 5;
        /// <summary>
        /// Header Display
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
        /// Includes additional custom design options
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
        /// Stores information about a page
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
        /// Measure String only works within the OnGUI
        /// </summary>
        private bool NeedInit = true;
        /// <summary>
        /// Determines whether the UI can already be drawn
        /// </summary>
        private bool CanDraw = false;
        /// <summary>
        /// Storage for the pages
        /// </summary>
        private List<UITabChild> pages = new();
        /// <summary>
        /// Current Selected Page
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
        /// Offset Vector
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
        /// Creates a new Tabcontrol
        /// </summary>
        /// <param name="rect">Tabsize</param>
        public UITabControl(ref Rect rectOrginal, Vector2 offset, Vector2 size) {
            ContentDrawAreal = rectOrginal;
            Offset = offset;    
            Size = size;
        }
        #endregion

        #region Init
        /// <summary>
        /// Method Chaining - Add new Pages
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
        /// Must be carried out once to calculate the dimensions
        /// </summary>

        public void Initialize() {
            if (pages.Count <= 0) return;
            //Berechnen der Ausrichtung vom Header
            DoWindow = DrawWindowData;
            NeedInit = true;
        }

        /// <summary>
        /// Calculate the header size depending on <see cref="TabControlHeaderStyle"/>
        /// </summary>
        /// <returns></returns>
        private void CalculateHeaderPosition() {
            // Takes the entire space and divides it evenly
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
        /// Calculates the dimensions of a GUIContent
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
        /// Checks whether the rectangles overlap each other
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
