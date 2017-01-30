using UnityEngine;
using System.Collections;

/// <summary>
/// This lets users have a simpler and fool-proof way of applying prefab in Editor. It is also a solution to the nested prefab workflow.
/// </summary>
public class AGPrefabPublisher : MonoBehaviour
{
    public GameObject m_targetGameObject;
    public Object m_targetPrefab;

    //The script takes two arguments as shown, and replaces the prefab (keeping links), does a "Save Project", and the user's workspace is untouched.
    public void Publish()
    {
        if (m_targetGameObject == null || m_targetPrefab == null)
        {
            Debug.LogError("Missing target GameObject or Prefab. Nothing happened.");
            return;
        }

        //Publish
        Debug.Log("Publishing prefab from gameobject (" + m_targetGameObject.name + " >> " + m_targetPrefab.name + ")");
        GameObject newGO = (GameObject)GameObject.Instantiate(m_targetGameObject);
#if UNITY_EDITOR
        UnityEditor.PrefabUtility.ReplacePrefab(newGO, m_targetPrefab, UnityEditor.ReplacePrefabOptions.ReplaceNameBased);

        //Clean up
        UnityEditor.AssetDatabase.Refresh();
        UnityEditor.AssetDatabase.SaveAssets();
#endif
        Transform.DestroyImmediate(newGO);
    }
}
