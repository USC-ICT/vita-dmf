using UnityEngine;
using UnityEngine.Rendering;

static public class AGAffectFbx
{
    //-------------------------------------------------------------------------
    //Set GO options on an array of GOs
    //-------------------------------------------------------------------------
    static public void SetGameObject(bool active, string[] affectedObjects, bool recursive, GameObject root)
    {
        foreach (string i in affectedObjects)
        {
            GameObject iGameObject = GetGameObjectChildRecursive(root, i);
            //Catch null GameObject
            if (iGameObject == null)
            {
                continue;
            }

            iGameObject.SetActive(active);

            //Set recursively, if specified
            if (recursive)
            {
                foreach (Transform childTransform in iGameObject.GetComponentsInChildren<Transform>())
                {
                    childTransform.gameObject.SetActive(active);
                }
            }
        }
    }

    //-------------------------------------------------------------------------
    //Set layer on an array of objects
    //-------------------------------------------------------------------------
    static public void SetLayer(string unityLayer, string[] affectObjects, bool recursive, GameObject root)
    {
        //Catch non-existent layer name
        if (LayerMask.NameToLayer(unityLayer) == -1)
        {
            Debug.LogError("Layer name '" + unityLayer + "' does not exist. Skipping.");
            return;
        }

        foreach (string i in affectObjects)
        {
            GameObject iGameObject = GetGameObjectChildRecursive(root, i);
            //Catch null GameObject
            if (iGameObject == null)
            {
                continue;
            }

            iGameObject.layer = LayerMask.NameToLayer(unityLayer);

            //Set recursively, if specified
            if (recursive)
            {
                foreach (Transform childTransform in iGameObject.GetComponentsInChildren<Transform>())
                {
                    childTransform.gameObject.layer = LayerMask.NameToLayer(unityLayer);
                }
            }
        }
    }

    //-------------------------------------------------------------------------
    //Enabled/disable an array of objects' "MeshRenderer" or "SkinnedMeshRenderer" components.
    //-------------------------------------------------------------------------
#if UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7
    static public void SetMeshRenderer(bool enabled, bool castShadows, bool receiveShadows, string[] affectedObjects, bool recursive, GameObject root)
#else
    static public void SetMeshRenderer(bool enabled, ShadowCastingMode castShadows, bool receiveShadows, string[] affectedObjects, bool recursive, GameObject root)
#endif
    {
        bool aRendererAffected = false;
        foreach (string i in affectedObjects)
        {
            GameObject iGameObject = VHUtils.FindChildRecursive(root, i);

            //Catch null GameObject
            if (iGameObject == null)
            {
                continue;
            }
            else if (iGameObject.GetComponent<MeshRenderer>() != null || iGameObject.GetComponent<SkinnedMeshRenderer>() != null)
            {
                //Catch GameObject with no renderers

                //Set renderer
                iGameObject.GetComponent<Renderer>().enabled = enabled;
#if UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7
                iGameObject.GetComponent<Renderer>().castShadows = castShadows;
#else
                iGameObject.GetComponent<Renderer>().shadowCastingMode = castShadows;
#endif
                iGameObject.GetComponent<Renderer>().receiveShadows = receiveShadows;
                aRendererAffected = true;
            }

            //Set recursively, if specified
            if (recursive)
            {
                foreach (Transform childTransform in iGameObject.GetComponentsInChildren<Transform>())
                {
                    //Catch GameObject child with no renderers
                    if (childTransform.gameObject.GetComponent<MeshRenderer>() == null && childTransform.gameObject.GetComponent<SkinnedMeshRenderer>() == null)
                    {
                        continue;
                    }
                    childTransform.GetComponent<Renderer>().enabled = enabled;
#if UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7
                    childTransform.GetComponent<Renderer>().castShadows = castShadows;
#else
                    childTransform.GetComponent<Renderer>().shadowCastingMode = castShadows;
#endif
                    childTransform.GetComponent<Renderer>().receiveShadows = receiveShadows;
                    aRendererAffected = true;
                }
            }
        }

        //Print warning if no renderers were affected
        if (!aRendererAffected)
        {
            Debug.LogWarning("No GameObjects had renderers to set. Nothing happened.");
        }
    }

    //-------------------------------------------------------------------------
    //Set tag on an array of objects
    //-------------------------------------------------------------------------
    static public void SetTag(string tag, string[] objectsToBeTagged, GameObject root)
    {
        /*
        //"UnityEditorInternal" breaks the build... unless there is an alternative, we will disable this check for now.
        //Catch non-existent tag
        if (System.Array.IndexOf(UnityEditorInternal.InternalEditorUtility.tags, tag) == -1){
            Debug.LogError("Tag '" + tag + "' does exist. Please check/add to the Tag Manager. Skipping.");
            return;
        }
        */

        //Set tags
        foreach (string i in objectsToBeTagged)
        {
            GameObject iGameObject = VHUtils.FindChildRecursive(root, i);

            //Catch null GameObject
            if (iGameObject == null)
            {
                continue;
            }

            iGameObject.tag = tag;
        }
    }

    //--------------------------------------------------------------------------
    //Sets the parent for the specified objects. If at all possible this script
    //should not be used as changing the scene hierarchy at runtime is a bad practice.
    //--------------------------------------------------------------------------
    static public void SetParent(GameObject root, string parent, string[] objectsToBeParented)
    {
        Transform parentTransform = VHUtils.FindChildRecursive(root, parent).transform;
        foreach (string i in objectsToBeParented)
        {
            GameObject iGameObject = VHUtils.FindChildRecursive(root, i);
            iGameObject.transform.parent = parentTransform;
        }
    }


    //--------------------------------------------------------------------------
    //Gets the child object by string name. Logs and error if not found.
    //--------------------------------------------------------------------------
    static private GameObject GetGameObjectChildRecursive(GameObject root, string gameObjectName)
    {
        GameObject gameObjectChild = VHUtils.FindChildRecursive(root, gameObjectName);
        if (gameObjectChild == null)
        {
            Debug.LogError("GameObject child '" + gameObjectName + "' could not be found in '" + root.name + "'. Skipping.");
        }
        return gameObjectChild;
    }
}