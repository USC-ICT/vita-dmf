/*--------------------------------------------------------------------------------------------------
 * This script batch changes the selected materials' shaders.
 *
 * Joe Yip
 * yip@ict.usc.edu
 * 2011-Jul-25
--------------------------------------------------------------------------------------------------*/

using UnityEditor;
using UnityEngine;

class ChangeMaterialShader{
    const string menuChangeShader = "VH/Change Selected Material Shaders";

    [MenuItem(menuChangeShader + "/Decal (2 UV Sets)")]
    static void toDecal2UVsMenu(){
        Object[] list = (Object[])Selection.objects;

        foreach (Material mat in list)
        {
#if UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7
            mat.shader = (Shader)Resources.LoadAssetAtPath("Assets/Resources/Shaders/Decal2UVs.shader", typeof(Shader));
#else
            mat.shader = (Shader)AssetDatabase.LoadAssetAtPath("Assets/Resources/Shaders/Decal2UVs.shader", typeof(Shader));
#endif
            mat.SetColor("_SpecColor", Color.black);
            Debug.Log("Assigned shader: Decal2UVs to material: " + mat.name);
        }
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
    }

    [MenuItem(menuChangeShader + "/AG Diffuse Specular Normal")]
    static void toAGDiffuseMenu(){
        Object[] list = (Object[])Selection.objects;

        foreach (Material mat in list)
        {
#if UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7
            mat.shader = (Shader)Resources.LoadAssetAtPath("Assets/vhAssets/shaders/AGDiffuseSpecularNormal.shader", typeof(Shader));
#else
            mat.shader = (Shader)AssetDatabase.LoadAssetAtPath("Assets/vhAssets/shaders/AGDiffuseSpecularNormal.shader", typeof(Shader));
#endif
            mat.SetColor("_SpecColor", Color.black);
            Debug.Log("Assigned shader: AGDiffuseSpecularNormal to material: " + mat.name);
        }
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
    }

    [MenuItem(menuChangeShader + "/AG Diffuse Specular Normal Alpha")]
    static void toAGDiffuseAlphaMenu(){
        Object[] list = (Object[])Selection.objects;

        foreach (Material mat in list)
        {
#if UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7
            mat.shader = (Shader)Resources.LoadAssetAtPath("Assets/vhAssets/shaders/AGDiffuseSpecularNormalAlpha.shader", typeof(Shader));
#else
            mat.shader = (Shader)AssetDatabase.LoadAssetAtPath("Assets/vhAssets/shaders/AGDiffuseSpecularNormalAlpha.shader", typeof(Shader));
#endif
            mat.SetColor("_SpecColor", Color.black);
            Debug.Log("Assigned shader: AGDiffuseSpecularNormalAlpha to material: " + mat.name);
        }
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
    }

    [MenuItem(menuChangeShader + "/Diffuse")]
    static void toDiffuseMenu(){
        Object[] list = (Object[])Selection.objects;

        foreach (Material mat in list){
            mat.shader = Shader.Find("Diffuse");
            Debug.Log("Assigned shader: Diffuse to material: " + mat.name);
        }
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
    }

    //Validates the menu; the item will be disabled if no object is selected.
    //Returns True if the menu item is valid.
    [MenuItem(menuChangeShader, true)]
    static bool ValidateChangeShaderMenu(){
        return Selection.activeGameObject != null;
    }
}
