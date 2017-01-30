/* AGRenderSettingsPBL.cs
 *
 * This script edits the Unity scene's render settings.
 * These settings are normally accessed through Edit > Render Settings.
 * However, clicking through that menu has become a very inefficient workflow.
 *
 * In Edit mode, this script will [as much as possible, due to lacking Unity APIs] override
 * settings under the "Lighting" window.
 *
 * The script initial values are set to Unity's default values.
 *
 * Written with Unity v4.6.0f3
 * Updated for Unity 5's Physically based lighting v5.2.3f1
 *
 * Usage
 * Add this script to a gameobject to use.  By default, the
 * script will run once OnEnable().
 *
 * Known inaccessible settings are:
 * - Enable/disable Pre-compute GI
 * - Enable/disable Baked GI
 * - Various options in realtime/baked variables (see documentation below in variables section)
 * - "Bake" button isn't included, since this isn't an option for during runtime, also to avoid needing an editor/custom inspector script
 *
 * Joe Yip
 * yip@ict.usc.edu
 * 2015-Jan-28
 */
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;

[ExecuteInEditMode]
public class AGRenderSettingsPBL : MonoBehaviour
{
    #region General Settings
    [Header("Script Settings")]
    public bool runOnEnable = true;         //Runs once OnEnable()
    public bool enableInPlayMode = false;   //Runs OnUpdate() if true
    public bool autoRunOnce = false;        //Used during runtime to make sure settings are applied
    bool autoRanOnce = true;                //Internal checker to make sure it has run once, used with var:"autoRunOnce"
    #endregion

    #region Environment Lighting
    //Environment Lighting
    public enum ambientModeEnum { Skybox, Gradient, Color }; //Needed since the ambient mode internal names and the exposed are mismatched
    [Header("Environment Lighting")]
    public Material skyboxMaterial;
    //public Light sun; Unity has not exposed a way to set this in its API. Brightest directional will be auto-assigned
    public ambientModeEnum ambientSource = ambientModeEnum.Skybox;
    public Color ambientLight = new Color(0.2f, 0.2f, 0.2f);
    public Color ambientEquatorColor = new Color(0.113f, 0.125f, 0.133f);
    public Color ambientGroundColor = new Color(0.047f, 0.043f, 0.035f);
    #if UNITY_5_3_OR_NEWER
    public ReflectionProbeMode ambientGI = ReflectionProbeMode.Realtime; //No destination plug
    #else
    [Range (0, 8)] public float ambientIntensity = 1f;
    #endif

    //Reflection
    public DefaultReflectionMode reflectionSource = DefaultReflectionMode.Skybox;
    public enum reflectionResolutionEnum { _128, _256, _512, _1024 };
    public reflectionResolutionEnum reflectionResolution = reflectionResolutionEnum._128;
    #if UNITY_EDITOR
    public ReflectionCubemapCompression reflectionSourceCompression = ReflectionCubemapCompression.Auto;
    #endif
    public Cubemap reflectionCustomCubemap;
    [Range (0, 1)] public float reflectionIntensity = 1f;
    [Range (1, 5)] public int reflectionBounces = 1;
    #endregion

    #region GI
    //Realtime
    [Header("Precomputed Realtime GI")]
    public static bool giPrecomputedRealtime = true; //No destination plug
    public bool NoAccessGiPrecomputedRealtime;
    public int giRealtimeResolution = 40;
    public static string giCpuUsage = "Low (Default)"; //No destination plug
    public string NoAccessGiCpuUsage;

    //Baked
    [Header("Baked GI")]
    public static bool giBaked = true; //No destination plug
    public bool NoAccessGiBaked;
    public int giBakedResolution = 40;
    public int giBakedPadding = 2;
    public bool giCompressed = true;
    #if UNITY_5_3_OR_NEWER
    public static bool giAmbientOcclusion = true; //No destination plug
    public bool NoAccessGiAmbientOcclusion;
    [Range(0, 9999)] public float giAoMaxDistance = 1;
    [Range(0, 10)] public static float giAoIndirect = 1f; //No destination plug
    public float NoAccessGiAoDirectgiAoIndirect;
    [Range(0, 10)] public static float giAoDirect = 0f; //No destination plug
    public float NoAccessGiAoDirect;
    #endif
    public static bool giFinalGather = false; //No destination plug
    public bool NoAccessGiAoDirectgiFinalGather;
    public static int giFinalGatherRayCount = 1024; //No destination plug
    public int NoAccessGiFinalGatherRayCount;
    public static bool giFinalGatherDenoising = true; //No destination plug
    public bool NoAccessGiFinalGatherDenoising;
    public enum atlasSizes { _32, _64, _128, _256, _512, _1024, _2048, _4096 };
    public atlasSizes giAtlasSize = atlasSizes._1024;
    #if UNITY_5_3_OR_NEWER
    public static bool lightProbesAddDirectLight = true; //No destination plug
    public bool NoAccessLightProbesAddDirectLight;
    #endif

    //General
    #if UNITY_5_3_OR_NEWER
    [Header("General GI")]
    public static string giDirectionMode = "Directional"; //No destination plug
    public string NoAccessGiDirectionMode;
    [Range(0, 5)] public float giIndirectIntensity = 1f;
    [Range(1, 10)] public float giBounceBoost = 1f;
    public static string giDefaultParamters = "Default-Medium";//What is this?; no destination plug
    public string NoAccessGiDefaultParameters;
    #else
    #endif
    #endregion

    #region Fog
    [Header("Fog")]
    public bool fog = false;
    public Color fogColor = new Color(0.5f, 0.5f, 0.5f);
    public FogMode fogMode = FogMode.ExponentialSquared;
    public float fogDensity = 0.01f;
    public float linearFogStart = 0f;
    public float linearFogEnd = 300f;
    #endregion

    #region Other Settings
    [Header("Other Settings")]
    //public Texture2D haloTexture; //No destination plug
    public float haloStrength = 0.5f;
    public float flareStrength = 1f;
    public float flareFadeSpeed = 3f;
    //public Texture2D spotCookie; //No destination plug
    #endregion

    public bool autoBake = true;

    void Awake()
    {
#if UNITY_EDITOR
        skyboxMaterial = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Material>("Default-Skybox.mat");
#endif
    }

    void OnEnable()
    {
        if (runOnEnable) SetSettings();
    }

    void Update()
    {

        /* //Current trials in trying to reflect into the lighting window - only got as far as to reflecting into the WINDOW, nothing lighting related yet
        //Get the lighting window as a serializedObject so we can "reflect" into its properties
        //var serializedObject = new UnityEditor.SerializedObject(UnityEditor.EditorWindow.GetWindow(typeof(UnityEditor.EditorWindow), false, "Lighting"));
        var serializedObject = new UnityEditor.SerializedObject(UnityEditor.EditorWindow.GetWindow(typeof(UnityEditor.EditorWindow), false, "Lighting"));
        const System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.NonPublic |
                                                     System.Reflection.BindingFlags.Public |
                                                     System.Reflection.BindingFlags.Instance |
                                                     System.Reflection.BindingFlags.Static;

        //Check lighting window's properties
        System.Reflection.PropertyInfo[] properties = serializedObject.targetObject.GetType().GetProperties(flags);
        string propertyNames = "";
        foreach (System.Reflection.PropertyInfo propertyInfo in properties)
        {
            propertyNames += propertyInfo.Name + " (" + propertyInfo.PropertyType.ToString() + ")\n";
        }
        Debug.Log(propertyNames);

        //Debug.Log(string.Format("{0}, {1}", propertyInfo.Name, propertyInfo.PropertyType));
        ////if (propertyInfo.PropertyType.Equals(typeof(UnityEditor.Editor)) || propertyInfo.PropertyType.Equals(typeof(Object)))
        //if (propertyInfo.Name == "renderSettings")
        //{
        //    //Debug.Log(propertyInfo.GetValue(serializedObject.targetObject, null));
        //    //Get the propertyInfo as an object reference, then create a new serializedObject from it
        //    Object newObj = (Object)propertyInfo.GetValue(serializedObject.targetObject, null);
        //    var newSerializedObject = new UnityEditor.SerializedObject(newObj);
        //    System.Reflection.PropertyInfo[] moreProps = newSerializedObject.targetObject.GetType().GetProperties(flags);

        //    //Check its property names
        //    foreach (System.Reflection.PropertyInfo moreInfo in moreProps)
        //    {
        //        Debug.Log(string.Format("    {0}.{1}", propertyInfo.Name, moreInfo.Name));
        //    }
        //}

        //Debug.Log(RenderSettings.skybox);
        //Debug.Log(UnityEditor.AssetDatabase.GetBuiltinExtraResource<Material>("Resources/unity_builtin_extra/Default-Skybox.mat"));
        //Debug.Log(UnityEditor.AssetDatabase.GetBuiltinExtraResource<Material>("unity_builtin_extra/Default-Skybox.mat"));
        //Debug.Log(UnityEditor.AssetDatabase.GetBuiltinExtraResource<Material>("Default-Skybox.mat"));
        //skyboxMaterial = (Material)Resources.Load("unity_builtin_extra/Default-Skybox");

        return;

        Debug.Log(serializedObject.targetObjects);
        foreach (Object sdf in serializedObject.targetObjects)
        {
            Debug.Log(sdf.name);
        }
        //Debug.Log(serializedObject.targetObject);

        var normalizedBlendValues = serializedObject.FindProperty("skybox");
        foreach (System.Reflection.PropertyInfo propertyInfo in properties)
        {
            Debug.Log(propertyInfo.Name);
            Debug.Log(propertyInfo);
            try
            {
                Debug.Log(propertyInfo.GetGetMethod().Attributes);
            }
            catch ( System.NullReferenceException)
            {

                throw;
            }
        }
        */

        //Set settings if user toggles the check box
        if (autoRunOnce == true)
        {
            autoRanOnce = false;
        }

        if (enableInPlayMode || !Application.isPlaying)
        {
            SetSettings();
        }
        //Set settings once on activation
        else if (autoRunOnce == true && autoRanOnce == false)
        {
            SetSettings();
            autoRunOnce = false;
            autoRanOnce = true;
        }
    }

    void SetSettings()
    {
        #region No Access Params
        NoAccessGiPrecomputedRealtime = giPrecomputedRealtime;
        NoAccessGiCpuUsage = giCpuUsage;
        NoAccessGiBaked = giBaked;
        NoAccessGiAmbientOcclusion = giAmbientOcclusion;
        NoAccessGiAoDirectgiAoIndirect = giAoIndirect;
        NoAccessGiAoDirect = giAoDirect;
        NoAccessGiAoDirectgiFinalGather = giFinalGather;
        NoAccessGiFinalGatherRayCount = giFinalGatherRayCount;
        NoAccessGiFinalGatherDenoising = giFinalGatherDenoising;
        NoAccessLightProbesAddDirectLight = lightProbesAddDirectLight;
        NoAccessGiDirectionMode = giDirectionMode;
        NoAccessGiDefaultParameters = giDefaultParamters;
        #endregion

        #region Environment Lighting
#if UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7
#else
        //Environment Lighting
        AmbientMode ambientSourceCasted = AmbientMode.Skybox;
        if (ambientSource == ambientModeEnum.Gradient)  ambientSourceCasted = AmbientMode.Trilight;
        if (ambientSource == ambientModeEnum.Color)     ambientSourceCasted = AmbientMode.Flat;
        RenderSettings.skybox = skyboxMaterial;
        //Sun GO cannot be assigned. See above in variable declaration for details
        RenderSettings.ambientMode = ambientSourceCasted;
        RenderSettings.ambientLight = ambientLight;
        RenderSettings.ambientEquatorColor = ambientEquatorColor;
        RenderSettings.ambientGroundColor = ambientGroundColor;
    #if UNITY_5_3_OR_NEWER
        //plug for ambientGI?
    #else
        RenderSettings.ambientIntensity = ambientIntensity;
    #endif

        //Reflection
        RenderSettings.defaultReflectionMode = reflectionSource;
        if (reflectionResolution == reflectionResolutionEnum._1024)     RenderSettings.defaultReflectionResolution = 1024;
        if (reflectionResolution == reflectionResolutionEnum._512)      RenderSettings.defaultReflectionResolution = 512;
        if (reflectionResolution == reflectionResolutionEnum._256)      RenderSettings.defaultReflectionResolution = 256;
        if (reflectionResolution == reflectionResolutionEnum._128)      RenderSettings.defaultReflectionResolution = 128;
    #if UNITY_EDITOR
        UnityEditor.LightmapEditorSettings.reflectionCubemapCompression = reflectionSourceCompression;
    #endif
        RenderSettings.customReflection = reflectionCustomCubemap;
        RenderSettings.reflectionIntensity = reflectionIntensity;
        RenderSettings.reflectionBounces = reflectionBounces;
#endif
        #endregion

        #region GI
#if UNITY_EDITOR
        //Realtime //Needs Realtime enable/disable, CPU usage amounts
        //giPrecomputedRealtime
        UnityEditor.LightmapEditorSettings.resolution = giRealtimeResolution; //In Unity 5.2.x this also controls "Indirect Resolution" under "Baked GI"
        //UnityEditor.LightmapEditorSettings

        //Baked //Needs Baked enable/disable, AO amount, Final Gather bool, FG ray count
        //giBaked
        UnityEditor.LightmapEditorSettings.bakeResolution = giBakedResolution;
        UnityEditor.LightmapEditorSettings.padding = giBakedPadding;
        UnityEditor.LightmapEditorSettings.textureCompression = giCompressed;
    #if UNITY_5_3_OR_NEWER
        //giAmbientOcclusion; //no destination plug
        UnityEditor.LightmapEditorSettings.aoMaxDistance = giAoMaxDistance;
        //giAoDirect //no destination plug
        //giAoIndirect //no destination plug
    #endif
        //UnityEditor.LightmapEditorSettings.= giFinalGather; //no destination plug
        //UnityEditor.LightmapEditorSettings.finalGatherRays = giFinalGatherRayCount; //no destination plug
        //Denoising //No destination plug
        UnityEditor.LightmapEditorSettings.maxAtlasWidth = int.Parse(giAtlasSize.ToString().Replace("_", ""));
        UnityEditor.LightmapEditorSettings.maxAtlasHeight = int.Parse(giAtlasSize.ToString().Replace("_", ""));
        //Light probs add direct light no destination plug

        //General
    #if UNITY_5_3_OR_NEWER
        //Directional Mode plug?
        UnityEditor.Lightmapping.indirectOutputScale = giIndirectIntensity;
        UnityEditor.Lightmapping.bounceBoost = giBounceBoost;
        //Default Params plug?
    #else
    #endif
#endif
        #endregion

        #region Fog
        //Fog
        RenderSettings.fog = fog;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogMode = fogMode;
        RenderSettings.fogDensity = fogDensity;
        RenderSettings.fogStartDistance = linearFogStart;
        RenderSettings.fogEndDistance = linearFogEnd;
        #endregion

        #region Other Settings
        //Other Settings
        //halo texture plug?
        RenderSettings.haloStrength = haloStrength;
        RenderSettings.flareStrength = flareStrength;
        RenderSettings.flareFadeSpeed = flareFadeSpeed;
        //cookie plug?
        #endregion

#if UNITY_EDITOR
        UnityEditor.Lightmapping.giWorkflowMode = (autoBake) ? UnityEditor.Lightmapping.GIWorkflowMode.Iterative : UnityEditor.Lightmapping.GIWorkflowMode.OnDemand;
#endif
    }
}
