using UnityEngine;
using System.Collections;

/// <summary>
/// Class to drive soft eyelid effect.
/// </summary>
/// <remarks>
/// Each eye requires a core eyelid transform and then upper and lower soft eyelid joints. These
/// 'soft' joints will receive a percentage of the eyes rotation as configurd by the user.
/// </remarks>
public class AGSoftEyelids : MonoBehaviour
{
    #region Variables
    // Amount of softness to lids
    [Range(0f, 1.0f)]
    public float softnessFactor = .25f;

    // Game objects used in setup, left
    [Header("Eye Joints, Left")]
    public string eyeLf;
    public string eyelidSoftUpperLf;
    public string eyelidSoftLowerLf;

    GameObject eyeLfGo;
    GameObject eyelidSoftUpperLfGo;
    GameObject eyelidSoftLowerLfGo;

    // Game objects used in setup, left
    [Header("Eye Joints, Right")]
    public string eyeRt;
    public string eyelidSoftUpperRt;
    public string eyelidSoftLowerRt;

    GameObject eyeRtGo;
    GameObject eyelidSoftUpperRtGo;
    GameObject eyelidSoftLowerRtGo;

    // Initial eye rotation
    Quaternion eyeLfInitialRot;
    Quaternion eyeRtInitialRot;

    // Debug
    [Header("Debug options")]
    public bool DebugVisualizations = false;
    [Range(.001f, .1f)]
    public float axisScale = .01f;
    #endregion

    /// <summary>
    /// Start!
    /// </summary>
    void Start()
    {
        // Convert string arguments to game objects
        eyeLfGo = VHUtils.FindChildRecursive(gameObject, eyeLf);
        eyelidSoftUpperLfGo = VHUtils.FindChildRecursive(gameObject, eyelidSoftUpperLf);
        eyelidSoftLowerLfGo = VHUtils.FindChildRecursive(gameObject, eyelidSoftLowerLf);

        eyeRtGo = VHUtils.FindChildRecursive(gameObject, eyeRt);
        eyelidSoftUpperRtGo = VHUtils.FindChildRecursive(gameObject, eyelidSoftUpperRt);
        eyelidSoftLowerRtGo = VHUtils.FindChildRecursive(gameObject, eyelidSoftLowerRt);

        // Check joint orientations
        CheckJointOrientation(eyeLfGo, eyelidSoftUpperLfGo);
        CheckJointOrientation(eyeLfGo, eyelidSoftLowerLfGo);

        CheckJointOrientation(eyeRtGo, eyelidSoftUpperRtGo);
        CheckJointOrientation(eyeRtGo, eyelidSoftLowerRtGo);

        // Get inital eyelid rotation for later lerping
        eyeLfInitialRot = eyeLfGo.transform.rotation;
        eyeRtInitialRot = eyeRtGo.transform.rotation;
    }

    /// <summary>
    /// Update!
    /// </summary>
    void Update()
    {
        // Lerp between initial orientation and current eye orientation
        eyelidSoftUpperLfGo.transform.rotation = Quaternion.Lerp(eyeLfInitialRot, eyeLfGo.transform.rotation, softnessFactor);
        eyelidSoftLowerLfGo.transform.rotation = Quaternion.Lerp(eyeLfInitialRot, eyeLfGo.transform.rotation, softnessFactor);

        eyelidSoftUpperRtGo.transform.rotation = Quaternion.Lerp(eyeRtInitialRot, eyeRtGo.transform.rotation, softnessFactor);
        eyelidSoftLowerRtGo.transform.rotation = Quaternion.Lerp(eyeRtInitialRot, eyeRtGo.transform.rotation, softnessFactor);
    }

    /// <summary>
    /// OnDrawGizmos!
    /// </summary>
    void OnDrawGizmos()
    {
        if (DebugVisualizations == true)
        {
            // Draw axis lines
            DrawAxisGizmo(eyeLfGo.transform, axisScale);
            DrawAxisGizmo(eyelidSoftUpperLfGo.transform, axisScale);
            DrawAxisGizmo(eyelidSoftLowerLfGo.transform, axisScale);

            DrawAxisGizmo(eyeRtGo.transform, axisScale);
            DrawAxisGizmo(eyelidSoftUpperRtGo.transform, axisScale);
            DrawAxisGizmo(eyelidSoftLowerRtGo.transform, axisScale);
        }
    }

    /// <summary>
    /// Check two joints against each other to make sure their orientations match.
    /// </summary>
    void CheckJointOrientation(GameObject gameObjectA, GameObject gameObjectB)
    {
        if(gameObjectA.transform.rotation != gameObjectB.transform.rotation)
        {
            Debug.LogErrorFormat("The game objects <color=white>{0}</color> and <color=white>{1}</color> do not have matching orientations. Soft eyelids will behave unpredictably.", gameObjectA, gameObjectB);
        }
    }


    /// <summary>
    /// Draw a gizmo showing the three primary axis.
    /// </summary>
    /// <param name="target"></param>
    void DrawAxisGizmo(Transform target, float scaleMultiplier)
    {
        Gizmos.color = Color.red;
        Vector3 directionX = target.TransformDirection(Vector3.right) * scaleMultiplier;
        Gizmos.DrawRay(target.position, directionX);

        Gizmos.color = Color.blue;
        Vector3 directionZ = target.TransformDirection(Vector3.forward) * scaleMultiplier;
        Gizmos.DrawRay(target.position, directionZ);

        Gizmos.color = Color.green;
        Vector3 directionY = target.TransformDirection(Vector3.up) * scaleMultiplier;
        Gizmos.DrawRay(target.position, directionY);

    }
}
