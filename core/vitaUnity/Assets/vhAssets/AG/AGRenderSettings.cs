/* This script edits the Unity scene's render settings.
 * These settings are normally accessed through Edit > Render Settings.
 * However, clicking through that menu has become a very inefficient workflow.
 *
 * Written with Unity v4.6.0f3
 *
 * Joe Yip
 * yip@ict.usc.edu
 * 2015-Jan-28
 */
using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class AGRenderSettings : MonoBehaviour
{
    //Casting Unity internal fogMode enum names to what the expose in the editor
    public enum fogModeTypeEnum {Linear, Exponential, Exp2};

    public bool fog = false;
    public Color fogColor = new Color(0.5f, 0.5f, 0.5f);
    public fogModeTypeEnum fogMode = fogModeTypeEnum.Exp2;//FogMode.ExponentialSquared;
    public float fogDensity = 0.01f;
    public float linearFogStart = 0f;
    public float linearFogEnd = 300f;
    public Color ambientLight = new Color(0.2f, 0.2f, 0.2f);
    public Material skyboxMaterial;
    public float haloStrength = 0.5f;
    public float flareStrength = 1f;
    public float flareFadeSpeed = 3f;
    //public Texture2D haloTexture; //Seems to be unused by Unity
    //public Texture2D spotCookie; //Seems to be unused by Unity
    public bool enableInPlayMode = false;

    void Update()
    {
        if (enableInPlayMode || !Application.isPlaying)
        {
            FogMode castedFogMode = FogMode.ExponentialSquared;
            if (fogMode == fogModeTypeEnum.Linear)      castedFogMode = FogMode.Linear;
            if (fogMode == fogModeTypeEnum.Exponential) castedFogMode = FogMode.Exponential;

            RenderSettings.fog = fog;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogMode = castedFogMode;
            RenderSettings.fogDensity = fogDensity;
            RenderSettings.fogStartDistance = linearFogStart;
            RenderSettings.fogEndDistance = linearFogEnd;
            RenderSettings.ambientLight = ambientLight;
            RenderSettings.skybox = skyboxMaterial;
            RenderSettings.haloStrength = haloStrength;
            RenderSettings.flareStrength = flareStrength;
            RenderSettings.flareFadeSpeed = flareFadeSpeed;
        }
    }
}
