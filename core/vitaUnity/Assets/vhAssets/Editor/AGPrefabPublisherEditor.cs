using UnityEditor;
using UnityEngine;
using System.Collections;

/// <summary>
/// This adds a "Publish Prefab" button on the inspector menu after rendering default inspector.
/// </summary>
[CustomEditor(typeof(AGPrefabPublisher))]
public class AGPrefabPublisherEditor : Editor
{
    public override void OnInspectorGUI()
    {
        AGPrefabPublisher publisherScript = (AGPrefabPublisher)target;
        DrawDefaultInspector();
        if (GUILayout.Button("Publish Prefab", GUILayout.Height(20)))
        {
            publisherScript.Publish();
        }
    }
}