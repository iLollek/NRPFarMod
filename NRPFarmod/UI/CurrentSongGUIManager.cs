using Il2Cpp;
using MelonLoader;
using NRPFarmod.ContentManager;
using NRPFarmod.CustomUnityScripts;
using NRPFarmod.MelonCall;
using NRPFarmod.UIHelper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NRPFarmod.UI
{
    public class CurrentSongGUIManager : MelonCaller, IGUIManager {

        private bool OnUpdateLock = false;
        private bool NeedInit = true;

        private Rect drawArea = Rect.zero;
        private Vector2 ClientArea = Vector2.zero;
        private Rect windowRect = Rect.zero;
        private Rect BackRect = Rect.zero;
        private Rect StopRect = Rect.zero;
        private Rect PlayRect = Rect.zero;
        private Rect NextRect = Rect.zero;
        private Rect PlayListRect = Rect.zero;

        private Texture2D? PlayTexture = null;
        private Texture2D? StopTexture = null;
        private Texture2D? ForwardTexture = null;
        private Texture2D? BackTexture = null;
        private Texture2D? PlaylistMode = null;
        private Texture2D? PlaylistModeSelected = null;

        private GUIContent CurrentPlayingContent = new GUIContent("");
        private GUIStyle CurrentPlaying = new GUIStyle() {
            fontSize = 20,
            normal = new GUIStyleState {
                textColor = Color.green
            }
        };

        public Action? PreviousSong { get; set; } = null;
        public Action? NextSong { get; set; } = null;


        private GodConstant godConstant = GodConstant.Instance;

        private CurrentSongGUIManager() { }

        public CurrentSongGUIManager(Rect windowRect, Vector2 clientRect, Action previous, Action next) {
            this.windowRect = windowRect;
            ClientArea = clientRect;
            PreviousSong = previous;
            NextSong = next;
        }

        public void DrawUI() {
            if (NeedInit) FirstInit();
            CurrentPlayingContent.text = $"Current Playing: {AudioClipTrigger.CurrentSong}";
            var size = UITabControl.MeasureString(CurrentPlayingContent, CurrentPlaying.fontSize);
            var space = (drawArea.width - size.x) / 2;
            GUI.Box(drawArea, "");
            Rect labelRect = new Rect(space, drawArea.y + 25, size.x, size.y);
            GUI.Label(labelRect, CurrentPlayingContent, CurrentPlaying);

            if (GUI.Button(BackRect, new GUIContent("", BackTexture, "Previous Song"))) {
                PreviousSong?.Invoke();
            }
            if (GUI.Button(StopRect, new GUIContent("", StopTexture, "Stop Song"))) {
                godConstant?.musicSource.Stop();
            }
            if (GUI.Button(PlayRect, new GUIContent("", PlayTexture, "Play Song"))) {
                if (godConstant?.musicSource == null) {
                    NextSong?.Invoke();
                } else {
                    godConstant?.musicSource.Play();
                }
            }
            if (GUI.Button(NextRect, new GUIContent("", ForwardTexture, "Next Song"))) {
                NextSong?.Invoke();
            }
            if (AudioClipTrigger.PlayListMode) {
                if (GUI.Button(PlayListRect, new GUIContent("", PlaylistModeSelected, "Playlistmode : OFF"))) {
                    AudioClipTrigger.PlayListMode = false;
                }
            } else {
                if (GUI.Button(PlayListRect, new GUIContent("", PlaylistMode, "Playlistmode: ON"))) {
                    AudioClipTrigger.PlayListMode = true;
                }
            }
        }

        public void FirstInit() {
            drawArea = new Rect(5, ClientArea.y + 10, windowRect.width - 12, windowRect.height - 60);
            int buttonWidth = (int)((drawArea.width - 50) / 4);
            BackRect = new Rect(drawArea.x + 10, drawArea.y + 60, buttonWidth, 50);
            StopRect = new Rect(BackRect.x + BackRect.width + 10, BackRect.y, buttonWidth, 50);
            PlayRect = new Rect(StopRect.x + StopRect.width + 10, StopRect.y, buttonWidth, 50);
            NextRect = new Rect(PlayRect.x + PlayRect.width + 10, PlayRect.y, buttonWidth, 50);
            PlayListRect = new Rect(PlayRect.x - (buttonWidth / 2), PlayRect.y + 60, buttonWidth, 50);
        }

        public override void OnUpdate() {
            if (OnUpdateLock) return;
            if (PlayTexture == null ||
                StopTexture == null ||
                ForwardTexture == null ||
                BackTexture == null ||
                PlaylistMode == null ||
                PlaylistModeSelected == null) {
                OnUpdateLock = true;
                MelonCoroutines.Start(LoadTextures());
            }
        }

        public IEnumerator LoadTextures() {
            yield return new WaitForSeconds(0.1f);
            TextureMananger.CreateTexture(ref PlayTexture, Properties.Resources.Play);
            TextureMananger.CreateTexture(ref StopTexture, Properties.Resources.Stop);
            TextureMananger.CreateTexture(ref BackTexture, Properties.Resources.Back);
            TextureMananger.CreateTexture(ref ForwardTexture, Properties.Resources.Forward);
            TextureMananger.CreateTexture(ref PlaylistMode, Properties.Resources.PayList);
            TextureMananger.CreateTexture(ref PlaylistModeSelected, Properties.Resources.PlayListModeAktive);
            OnUpdateLock = false;
        }
    }
}
