using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Procedurally controls head movement such as nods, shakes, and tilts. Gazing is NOT handled here
/// </summary>
public class HeadController : MonoBehaviour
{

    #region Constants
    const float DefaultSpeed = 0.5f;
    const float DirectionDampTime = 0.25f;

    public enum MovementType
    {
        Nod,
        Shake,
        Tilt,
    }

    class MovementData
    {
        public MovementType m_MovementType;
        public float m_CurrentNeckRot;
        public float m_TimePassed = 0;
        public float m_TimeToComplete = 0;
        public float m_Amplitude;
        public float m_Frequency;
        public float m_NumTimes;


        public MovementData(MovementType movementType, float amplitude, float frequency, float numTimes, float timeToComplete)
        {
            m_MovementType = movementType;
            m_NumTimes = numTimes;
            m_Amplitude = amplitude;
            m_Frequency = frequency;
            m_TimeToComplete = timeToComplete;
        }
    }
    #endregion

    #region Variables
    [SerializeField] string m_NeckTransformName = "JtSkullA";
    [SerializeField] float m_NodAmplifier = 30.0f;
    [SerializeField] float m_ShakeAmplifier = 45.0f;
    [SerializeField] float m_TiltAmplifier = 30.0f;

    Transform m_Neck;
    List<MovementData> m_CurrentHeadMovements = new List<MovementData>();
    #endregion

    #region Functions
    void Awake()
    {
        GameObject neckGO = VHUtils.FindChildRecursive(gameObject, m_NeckTransformName);
        if (neckGO != null)
        {
            m_Neck = neckGO.transform;
        }
        else
        {
            Debug.LogError("Couldn't find neck go named " + m_NeckTransformName + " on character " + name + ". Gazing, nodding, and shaking won't work");
        }
    }

    void LateUpdate()
    {
        for (int i = 0; i < m_CurrentHeadMovements.Count; i++)
        {
            if (DoHeadMovement(m_CurrentHeadMovements[i]))
            {
                // movement finished, remove it
                m_CurrentHeadMovements.RemoveAt(i--);
            }
        }
    }

    public void NodHead(float amount, float numTimes, float duration)
    {
        CreateHeadMovement(MovementType.Nod, amount, numTimes, duration);
    }

    public void ShakeHead(float amount, float numTimes, float duration)
    {
        CreateHeadMovement(MovementType.Shake, amount, numTimes, duration);
    }

    public void TiltHead(float amount, float numTimes, float duration)
    {
        CreateHeadMovement(MovementType.Tilt, amount, numTimes, duration);
    }

    void CreateHeadMovement(MovementType type, float amount, float numTimes, float duration)
    {
        amount = Mathf.Clamp(amount, -1, 1);
        if (amount == 0)
            amount = DefaultSpeed;

        float amplitude = 1;
        float frequency = 1;

        switch (type)
        {
        case MovementType.Nod:
            amplitude = amount * m_NodAmplifier;
            frequency = (Mathf.PI * 2.0f) / (duration);
            break;

        case MovementType.Shake:
            amplitude = amount * m_ShakeAmplifier;
            frequency = (Mathf.PI * 2.0f) / (duration);
            break;

        case MovementType.Tilt:
        default:
            amplitude = amount * m_TiltAmplifier;
            frequency = (Mathf.PI * 2.0f) / (duration);
            break;
        }

        m_CurrentHeadMovements.Add(new MovementData(type, amplitude, frequency, numTimes, duration));
    }

    bool DoHeadMovement(MovementData movementData)
    {
        bool isMovementFinished = false;
        //m_CurrentNeckRot = amplitude * Mathf.Sin(angularFrequency * time);
        movementData.m_CurrentNeckRot = movementData.m_Amplitude * Mathf.Sin((movementData.m_TimePassed / movementData.m_TimeToComplete) * 2.0f * Mathf.PI * movementData.m_NumTimes);
        movementData.m_TimePassed += Time.deltaTime;

        m_Neck.transform.Rotate(GetRotationAxis(movementData.m_MovementType), movementData.m_CurrentNeckRot, 0);

        if (movementData.m_TimePassed >= movementData.m_TimeToComplete)
        {
            isMovementFinished = true;
        }

        return isMovementFinished;
    }

    Vector3 GetRotationAxis(MovementType type)
    {
        Vector3 axis = m_Neck.forward;
        if (type == MovementType.Nod)
        {
            axis = m_Neck.forward;
        }
        else if (type == MovementType.Shake)
        {
            axis = m_Neck.right;
        }
        else
        {
            axis = m_Neck.up;
        }
        return axis;
    }
    #endregion

}
