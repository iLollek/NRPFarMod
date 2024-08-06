using MelonLoader;
using NRPFarmod.ContentManager;
using NRPFarmod.MelonCall;
using System.Collections;
using UnityEngine;

namespace NRPFarmod.UI {
    public class MemoryGUIManager<T> : MelonCaller, IGUIManager where T : UnityEngine.Object {

        private Texture2D? memoryView = null;
        private List<(int, double)> lastValues = new();
        private Vector2 ClientArea = Vector2.zero;
        private Rect windowRect = Rect.zero;
        private bool OnUpdateLock = false;
        private bool NeedInit = true;
        private Color LineColor = new Color(0, 0, 0, 100);
        private Color MemoryViewColor = Color.green;
        private Color MemoryViewBackColor = new Color(30f / 255f, 30f / 255f, 30f / 255f);
        private Color Transparent = new Color(0, 0, 0, 0);
        private ContentManager<T> contentManager;
        private Rect AutoRefreshRect = Rect.zero;

        private bool AutoRefresh = false;
        private float RefreshTimeout = 1.0f;
        private float CurrentTime = 0f;

        public MemoryGUIManager(ContentManager<T> contentManager, Rect windowRect, Vector2 clientArea) {
            this.contentManager = contentManager;
            this.windowRect = windowRect;
            ClientArea = clientArea;
        }

#pragma warning disable
        private MemoryGUIManager() { }
#pragma warning restore

        private Rect drawArea = Rect.zero;
        public void DrawUI() {
            if (NeedInit) FirstInit();        
            GUI.Box(drawArea, "");
            if (memoryView == null) {
                memoryView = new Texture2D((int)drawArea.width - 10, (int)drawArea.height - 100);
                if (RefreshMemoryViewTexture(out var newValue)) {
                    lastValues = newValue;
                }
            }
            GUI.DrawTexture(new Rect(10, ClientArea.y + 20, windowRect.width - 22, windowRect.height - 100), memoryView);
            AutoRefresh = GUI.Toggle(AutoRefreshRect, AutoRefresh, $"Auto Refresh: {AutoRefresh}");
            foreach (var label in lastValues) {
                GUI.Label(new Rect(drawArea.x + 5, drawArea.height - label.Item1, 200, 30), $"{label.Item2} MB");
            }
        }

        public void FirstInit() {
            drawArea = new Rect(5, ClientArea.y + 10, windowRect.width - 12, windowRect.height - 60);
            memoryView = new Texture2D((int)drawArea.width - 10, (int)drawArea.height - 80);
            AutoRefreshRect = new Rect(10, drawArea.height + 30, 200, 30);
            for (int y = 0; y < memoryView.height; y++)
                for (int x = 0; x < memoryView.width; x++)
                    memoryView.SetPixel(x, y, Transparent);
            memoryView.Apply();
            NeedInit = false;
        }

        public IEnumerator LoadTextures() {
            yield return new WaitForSeconds(0.01f);
            if (RefreshMemoryViewTexture(out var newValue)) {
                lastValues = newValue;
            }
            OnUpdateLock = false;
        }

        public override void OnUpdate() {
            if (OnUpdateLock) return;
            if (AutoRefresh) {
                CurrentTime += Time.deltaTime;
                if(CurrentTime > RefreshTimeout) {
                    OnUpdateLock = true;
                    CurrentTime = 0f;
                    contentManager.ManagedContent.MemorySnapshot();         
                    MelonCoroutines.Start(LoadTextures());
                }
            }
        }

        public void RefreshTexture() {
            if (RefreshMemoryViewTexture(out var newValue)) {
                lastValues = newValue;
            }
        }

        #region DrawMemoryGraph
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
        /// Draws a line according to Bresenhams algorithm
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
    }
}
