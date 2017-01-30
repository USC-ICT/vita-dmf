using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;
using System.Linq;

/// <summary>
/// The purpose of this class is to add menu items to the unity menu toolbar.
/// Clicking these menu items will perform some functionality describe in this file.
/// Non-EditorWindow MenuItems should go in this class to keep them in a common place
/// EditorWindow MenuItems go in their own specific classes (like SBMWindow.cs)
/// </summary>
public class VHMenuItems : MonoBehaviour
{
    [MenuItem("VH/Scene Reporting/Check For Duplicate Materials")]
    static void CheckForDuplicateMaterials()
    {
        //Object[] allProjectMaterials = Component.FindObjectsOfTypeIncludingAssets(typeof(Material));
        List<string> allProjectMaterials = new List<string>(Directory.GetFiles(Application.dataPath, "*.mat", SearchOption.AllDirectories));
        List<string> duplicateMaterials = new List<string>();

        if (allProjectMaterials == null || allProjectMaterials.Count <= 0)
        {
            Debug.Log("Couldn't find any materials in the project");
            return;
        }

        Debug.Log("Num materials in project: " + allProjectMaterials.Count);

        // format the paths for unity to load assets properly
        for (int i = 0; i < allProjectMaterials.Count; i++)
        {
            // All asset names & paths in Unity use forward slashes, paths using backslashes will not work.
            allProjectMaterials[i] = allProjectMaterials[i].Replace('\\', '/');

            // remove everything before Unity's "Assets" folder, otherwise the load fails
            allProjectMaterials[i] = VHFile.RemovePathUpTo("Assets/", allProjectMaterials[i]);
        }

        // O(n^2)
        Material matI, matJ;
        StringBuilder dupMaterialString = new StringBuilder();
        for (int i = 0; i < allProjectMaterials.Count; i++)
        {
            //Debug.Log("allProjectMaterials[i]: " + allProjectMaterials[i]);
            matI = (Material)AssetDatabase.LoadAssetAtPath(allProjectMaterials[i], typeof(Material));
            for (int j = i + 1; j < allProjectMaterials.Count; j++)
            {
                matJ = (Material)AssetDatabase.LoadAssetAtPath(allProjectMaterials[j], typeof(Material));

                if (matI.shader.name == matJ.shader.name && matI.mainTexture == matJ.mainTexture
                    && matI.color == matJ.color)
                {
                    // this appears to be a duplicate
                    dupMaterialString.Append(", " + matJ.name);
                    allProjectMaterials.RemoveAt(j--);
                }
            }

            if (dupMaterialString.Length > 0)
            {
                // duplicate material names have been stored
                duplicateMaterials.Add("Potential duplicate materials using " + matI.shader.name + ": " + matI.name + dupMaterialString);

                // clear string
                dupMaterialString = dupMaterialString.Remove(0, dupMaterialString.Length);
            }
        }

        // show the duplicates
        for (int i = 0; i < duplicateMaterials.Count; i++)
        {
            Debug.LogWarning(duplicateMaterials[i]);
        }

        Resources.UnloadUnusedAssets();
    }

    [MenuItem("VH/Generate Audio Prefabs")]
    static void GenerateAudioPrefabs()
    {
        Debug.Log("--- GenerateAudioPrefabs() started -------------");

        List<string> filesToConvert = new List<string>();

        UnityEngine.Object[] selectedObjects = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets);
        foreach (UnityEngine.Object obj in selectedObjects)
        {
            string assetPath = AssetDatabase.GetAssetPath(obj);

            if (assetPath.EndsWith(".bml"))
            {
                string newFile = string.Format("{0}/{1}", Application.dataPath.Replace("/Assets", ""), assetPath + ".txt");

                if (!File.Exists(newFile))
                {
                    // this hasn't been created yet, so create it
                    AssetDatabase.CopyAsset(assetPath, assetPath + ".txt");
                    AssetDatabase.ImportAsset(assetPath + ".txt");

                    assetPath = assetPath + ".txt";
                }
            }

            if (assetPath.EndsWith(".bml.txt"))
            {
                filesToConvert.Add(assetPath);
            }
        }


        if (filesToConvert.Count == 0)
        {
            EditorUtility.DisplayDialog("Error", "Select a folder or a *.bml.txt in order to generate", "Ok");
        }

        foreach (string file in filesToConvert)
        {
            BMLConverter.Convert(file);
        }

        Debug.Log(string.Format("--- GenerateAudioPrefabs() ended - {0} Prefabs generated -------------", filesToConvert.Count));
    }

    [MenuItem("VH/Fix Cutscene References")]
    public static void FixCutsceneReferences()
    {
        if (Selection.gameObjects == null || Selection.gameObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("Error", "Please select 1 or more cutscenes in the hierarchy", "ok");
        }

        foreach (GameObject go in Selection.gameObjects)
        {
            Cutscene cutscene = go.GetComponent<Cutscene>();
            if (cutscene == null)
            {
                continue;
            }

            Transform genericEvents = cutscene.transform.FindChild("GenericEvents");
            if (genericEvents == null)
            {
                Debug.LogError(string.Format("Cutscene {0} doesn't have a GenericEvents gameobject??", cutscene.CutsceneName));
                continue;
            }

            foreach (CutsceneEvent ce in cutscene.CutsceneEvents)
            {
                if (!GenericEventNames.IsCustomEvent(ce.EventType))
                {
                    Debug.Log(ce.Name);
                    ce.SetFunctionTargets(genericEvents.gameObject, genericEvents.GetComponent(ce.TargetComponent.GetType()));
                }
            }

            EditorUtility.SetDirty(cutscene.gameObject);
        }
    }
}
