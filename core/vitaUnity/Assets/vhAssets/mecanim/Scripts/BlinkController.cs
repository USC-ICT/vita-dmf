using UnityEngine;
using System.Collections;

public class BlinkController : MonoBehaviour
{
    #region Constants
    enum BlinkMode
    {
        Animation,
        BlendTree,
        BlendShape
    }
    #endregion

    #region Variables
    [SerializeField] float m_MinBlinkInterval = 4.0f;
    [SerializeField] float m_MaxBlinkInterval = 8.0f;
    [SerializeField] float m_BlinkLength = 0.2f;
    [SerializeField] bool m_IsBlinkingOn = true;
    [SerializeField] BlinkMode m_BlinkMode = BlinkMode.BlendTree;
    [SerializeField] string m_BlinkAnimName = "";
    [SerializeField] string[] m_EyeLidControllerParams = new string[] { "045_blink_lf", "045_blink_rt" };
    [SerializeField] string[] m_EyeLidBlendShapes = new string[] { "045_blink_lf", "045_blink_rt" };
    [SerializeField] string m_BlendShapeSkinnedMeshName = "";

    float m_BlinkPeriod = 1;
    Animator m_Animator;
    SkinnedMeshRenderer m_SkinnedMeshRenderer;
    #endregion

    #region Properties
    public bool IsBlinkingOn
    {
        get { return m_IsBlinkingOn; }
        set { m_IsBlinkingOn = value; }
    }

    #endregion

    #region Functions
    void Start()
    {
        m_Animator = GetComponentInChildren<Animator>();
        if (m_Animator == null)
        {
            Debug.LogError("Blink Controller needs and animator");
        }

        GameObject go = VHUtils.FindChildRecursive(gameObject, m_BlendShapeSkinnedMeshName);
        if (go != null)
        {
            m_SkinnedMeshRenderer = go.GetComponent<SkinnedMeshRenderer>();
        }

        StartCoroutine(BlinkUpdate());
    }


    IEnumerator BlinkUpdate()
    {
        while (true)
        {
            m_BlinkPeriod = Random.Range(m_MinBlinkInterval, m_MaxBlinkInterval);

            yield return new WaitForSeconds(m_BlinkPeriod);

            if (IsBlinkingOn)
            {
                Blink();
            }
        }
    }

    public void Blink()
    {
        switch (m_BlinkMode)
        {
            case BlinkMode.Animation:
                HandleAnimationBlink();
                break;

            case BlinkMode.BlendTree:
                HandleBlendTreeBlink();
                break;

            case BlinkMode.BlendShape:
                HandleBlendShapeBlink();
                break;
        }
    }

    void HandleAnimationBlink()
    {
        m_Animator.Play(m_BlinkAnimName, GetComponent<MecanimCharacter>().FaceLayerIndex);
    }

    void HandleBlendTreeBlink()
    {
        StartCoroutine(PerformBlink(m_BlinkLength));
    }


    void HandleBlendShapeBlink()
    {
        StartCoroutine(PerformBlink(m_BlinkLength));
    }

    IEnumerator PerformBlink(float blinkSpeed)
    {
        float timePassed = 0;
        float[] weights = GetCurrentEyeLidWeights();
        while (timePassed < blinkSpeed)
        {
            for (int i = 0; i < weights.Length; i++)
            {
                SetEyeLidWeight(GetEyeLidName(i), Mathf.SmoothStep(weights[i], 1, timePassed / blinkSpeed));
            }

            timePassed += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        timePassed = 0;
        while (timePassed < blinkSpeed)
        {
            for (int i = 0; i < weights.Length; i++)
            {
                SetEyeLidWeight(GetEyeLidName(i), Mathf.SmoothStep(1, 0, timePassed / blinkSpeed));
            }

            timePassed += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }

    string GetEyeLidName(int index)
    {
        return m_BlinkMode == BlinkMode.BlendTree ? m_EyeLidControllerParams[index] : m_EyeLidBlendShapes[index];
    }

    float[] GetCurrentEyeLidWeights()
    {
        float[] weights = null;
        if (m_BlinkMode == BlinkMode.BlendTree)
        {
            weights = new float[m_EyeLidControllerParams.Length];
            for (int i = 0; i < weights.Length; i++)
            {
                weights[i] = m_Animator.GetFloat(m_EyeLidControllerParams[i]);
            }
        }
        else if (m_BlinkMode == BlinkMode.BlendShape)
        {
            weights = new float[m_EyeLidBlendShapes.Length];
            for (int i = 0; i < weights.Length; i++)
            {
                int index = m_SkinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(m_EyeLidBlendShapes[i]);
                weights[i] = m_SkinnedMeshRenderer.GetBlendShapeWeight(index);
            }
        }
        return weights;
    }

    void SetEyeLidWeight(string lidName, float weight)
    {
        if (m_BlinkMode == BlinkMode.BlendTree)
        {
            m_Animator.SetFloat(lidName, weight);
        }
        else if (m_BlinkMode == BlinkMode.BlendShape)
        {
            int index = m_SkinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(lidName);
            m_SkinnedMeshRenderer.SetBlendShapeWeight(index, weight);
        }
    }

    #endregion
}
