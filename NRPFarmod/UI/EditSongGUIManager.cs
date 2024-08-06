using Il2Cpp;
using NRPFarmod.ContentManager;
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
    public sealed class EditSongGUIManager<T> : MelonCaller, IGUIManager where T : UnityEngine.Object
    {

        private Rect windowRect = Rect.zero;
        private Vector2 ClientArea = Vector2.zero;

        private bool OnUpdateLock = false;
        private bool NeedInit = true;
        private GodConstant? godConstant = null;
        private ContentManager<T> contentManager;

        private Rect DefaultButtonRect = Rect.zero;
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
        private GUIContent DisplayLabel = new GUIContent("Display: ");
        private Rect DisplayLabelRect = Rect.zero;
        private GUIStyle EditSongBoxStyle = new GUIStyle()
        {
            fontSize = 15,
            alignment = TextAnchor.UpperCenter,
            normal = new GUIStyleState
            {
                textColor = Color.green
            }
        };
        private GUIStyle EditSongStyle = new GUIStyle()
        {
            fontSize = 15,
            normal = new GUIStyleState
            {
                textColor = Color.green
            }
        };
#pragma warning disable 
        private EditSongGUIManager()
        {

        }
#pragma warning restore

        public EditSongGUIManager(ContentManager<T> contentManager, Rect windowRect, Vector2 ClientArea)
        {
            this.contentManager = contentManager;
            this.windowRect = windowRect;
            this.ClientArea = ClientArea;
        }

        public void DrawUI()
        {
            if (NeedInit) FirstInit();

            Rect drawArea = new Rect(5, ClientArea.y + 10, windowRect.width - 12, windowRect.height - 60);
            GUI.Box(drawArea, $"Edit - {contentManager.CurrentClipInfo.UI_Display}", EditSongBoxStyle);

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
            clip.Volume = GUI.HorizontalSlider(EditVolumeRect, contentManager.CurrentClipInfo.Volume, -50f, 10);
            clip.ReverbZoneMix = GUI.HorizontalSlider(EditSpeedRect, contentManager.CurrentClipInfo.ReverbZoneMix, 0f, 5f);

            SetSongValues(clip.Volume, clip.Pitch, clip.ReverbZoneMix);

            if (GUI.Button(ResetButtonRect, "Reset"))
            {
                contentManager.ResetCurrentSongInfo();
            }

            if (GUI.Button(DefaultButtonRect, "Default"))
            {
                SetSongDefaultValues();
            }
        }

        public void FirstInit()
        {
            godConstant = GodConstant.Instance;
            float maxX = 0;
            Rect drawArea = new Rect(5, ClientArea.y + 10, windowRect.width - 12, windowRect.height - 60);
            var dlr = UITabControl.MeasureString(DisplayLabel, EditSongStyle.fontSize); maxX = maxX < dlr.x ? dlr.x : maxX;
            DisplayLabelRect = new Rect(drawArea.x + 5, drawArea.y + 30, dlr.x, dlr.y);
            int ySteps = (int)(dlr.y + 15); //10 Margin 
            dlr = UITabControl.MeasureString(PitchLabel, EditSongStyle.fontSize); maxX = maxX < dlr.x ? dlr.x : maxX;
            PitchLabelRect = new Rect(DisplayLabelRect.x, DisplayLabelRect.y + ySteps, dlr.x, dlr.y);
            dlr = UITabControl.MeasureString(VolumeLabel, EditSongStyle.fontSize); maxX = maxX < dlr.x ? dlr.x : maxX;
            VolumeLabelRect = new Rect(DisplayLabelRect.x, PitchLabelRect.y + ySteps, dlr.x, dlr.y);
            dlr = UITabControl.MeasureString(SpeedLabel, EditSongStyle.fontSize); maxX = maxX < dlr.x ? dlr.x : maxX;
            SpeedLabelRect = new Rect(DisplayLabelRect.x, VolumeLabelRect.y + ySteps, dlr.x, dlr.y);
            int controlLengths = (int)(drawArea.width - 30 - maxX);
            int startXPos = (int)(drawArea.width - controlLengths);
            EditDisplayRect = new Rect(DisplayLabelRect.x + startXPos, DisplayLabelRect.y - 5, controlLengths - 10, DisplayLabelRect.height);
            EditPitchRect = new Rect(DisplayLabelRect.x + startXPos, EditDisplayRect.y + EditDisplayRect.height + 23, controlLengths - 10, PitchLabelRect.height);
            EditVolumeRect = new Rect(DisplayLabelRect.x + startXPos, EditPitchRect.y + EditPitchRect.height + 17, controlLengths - 10, VolumeLabelRect.height);
            EditSpeedRect = new Rect(DisplayLabelRect.x + startXPos, EditVolumeRect.y + EditVolumeRect.height + 17, controlLengths - 10, SpeedLabelRect.height);
            ResetButtonRect = new Rect(drawArea.x + 5, windowRect.height - 40, 200, 30);
            DefaultButtonRect = new Rect(ResetButtonRect.x + ResetButtonRect.width + 5, ResetButtonRect.y, ResetButtonRect.width, ResetButtonRect.height);
            NeedInit = false;
        }

        private void SetSongValues(float volume, float pitch, float reverbZoneMix)
        {
            if (godConstant == null) return;
            godConstant.musicSource.pitch = pitch;
            godConstant.musicSource.reverbZoneMix = reverbZoneMix;
            godConstant.musicVol_Target = volume;
        }

        private void SetSongDefaultValues()
        {
            var clip = contentManager.CurrentClipInfo;
            clip.Volume = 0;
            clip.Pitch = 1;
            clip.ReverbZoneMix = 1;
            SetSongValues(0, 1, 1);
        }

        public IEnumerator LoadTextures()
        {
            yield return new WaitForSeconds(0);
        }
    }
}
