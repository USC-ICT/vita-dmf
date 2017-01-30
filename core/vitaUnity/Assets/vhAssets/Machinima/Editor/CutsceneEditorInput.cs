using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class CutsceneEditorInput : TimelineInput
{
    #region Constants

    #endregion

    #region Variables
    CutsceneEditor m_CutsceneEditor;
    #endregion

    #region Properties
    Cutscene SelectedCutscene
    {
        get { return m_CutsceneEditor.GetSelectedCutscene(); }
    }
    #endregion

    #region Functions
    public CutsceneEditorInput(CutsceneEditor editor)
        : base(editor)
    {
        m_CutsceneEditor = editor;
    }

    protected override void DragTimeSlider(Vector2 mousePos, float amount)
    {
        base.DragTimeSlider(mousePos, amount);
        HandleTimeJump();
    }

    protected override void HandleTimeJump()
    {
        if (Application.isPlaying)
        {
            editor.m_MockPlaying = false;
            SelectedCutscene.FastForward(editor.CurrentTime, Cutscene.MaxFastForwardSpeed, OnFinishedFastForwarding);
            m_CutsceneEditor.IsFastForwarding = true;
        }
    }

    /// <summary>
    /// Returns a list of Cutscene events that the rubberband selection area intersects
    /// </summary>
    /// <returns></returns>
    override protected List<CutsceneTrackItem> GetRubberBandSelections()
    {
        List<CutsceneTrackItem> selected = new List<CutsceneTrackItem>();
        foreach (CutsceneEvent se in SelectedCutscene.CutsceneEvents)
        {
            if (!se.Hidden && VHMath.IsRectOverlapping(m_RubberBandSelectionArea, se.GuiPosition))
            {
                selected.Add(se);
            }
        }
        return selected;
    }

    void OnFinishedFastForwarding(Cutscene cutscene)
    {
        m_CutsceneEditor.IsFastForwarding = false;
    }

    protected override void HandleSequencerDragDrop(string[] dragAndDropObjectPaths)
    {
        Array.ForEach<string>(dragAndDropObjectPaths, s => Debug.Log(s));
        string xmlFile = Array.Find<string>(dragAndDropObjectPaths, s => Path.GetExtension(s) == ".xml");
        string bmlFile = Array.Find<string>(dragAndDropObjectPaths, s => Path.GetExtension(s) == ".bml");
        if (!string.IsNullOrEmpty(xmlFile) && !string.IsNullOrEmpty(bmlFile))
        {
            m_CutsceneEditor.RequestFileOpenBMLXMLPair(xmlFile);
        }
        else if (!string.IsNullOrEmpty(xmlFile))
        {
            m_CutsceneEditor.RequestFileOpenXML(xmlFile);
        }
        else if (!string.IsNullOrEmpty(bmlFile))
        {
            m_CutsceneEditor.RequestFileOpenBML(bmlFile);
        }
        else if (DragAndDrop.objectReferences != null && DragAndDrop.objectReferences.Length > 0)
        {
            foreach (UnityEngine.Object obj in DragAndDrop.objectReferences)
            {
                GameObject go = obj as GameObject;
                if (go != null)
                {
                    AudioSpeechFile asf = go.GetComponent<AudioSpeechFile>();
                    if (asf != null)
                    {
                        if (asf.m_Xml != null && asf.m_LipSyncInfo != null)
                        {
                            m_CutsceneEditor.RequestFileOpenBMLXMLPair(AssetDatabase.GetAssetPath(asf.m_Xml));
                        }
                        else if (asf.m_Xml != null)
                        {
                            m_CutsceneEditor.RequestFileOpenXML(AssetDatabase.GetAssetPath(asf.m_Xml));
                        }
                        else if (asf.m_LipSyncInfo)
                        {
                            m_CutsceneEditor.RequestFileOpenBML(AssetDatabase.GetAssetPath(asf.m_LipSyncInfo));
                        }
                    }
                }
            }
        }
    }
    #endregion
}
