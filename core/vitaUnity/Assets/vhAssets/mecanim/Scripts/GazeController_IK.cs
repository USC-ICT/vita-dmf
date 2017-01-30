using UnityEngine;
using System.Collections;

public class GazeController_IK : GazeController
{
    #region Variables
    [SerializeField]
    [Range(0, 1)]
    float m_HeadGazeWeight = 0.5f;
    [SerializeField]
    [Range(0, 1)]
    float m_BodyGazeWeight = 0;
    [SerializeField]
    [Range(0, 1)]
    float m_EyeGazeWeight = 1;

    [SerializeField]
    [Range(0, 1)]
    float m_CurrentTotalGazeWeight = 0;

    [SerializeField]
    [Range(0, 1)]
    float m_CurrentEyeGazeWeight = 0;

    [SerializeField]
    [Range(0, 1)]
    float m_CurrentHeadGazeWeight = 0;

    [SerializeField]
    [Range(0, 1)]
    float m_CurrentBodyGazeWeight = 0;
    Animator m_Animator;
    #endregion

    #region Properties
    override public float HeadGazeWeight
    {
        get { return m_HeadGazeWeight; }
        set { m_HeadGazeWeight = value; }
    }
    override public float EyeGazeWeight
    {
        get { return m_EyeGazeWeight; }
        set { m_EyeGazeWeight = value; }
    }
    override public float BodyGazeWeight
    {
        get { return m_BodyGazeWeight; }
        set { m_BodyGazeWeight = value; }
    }
    override protected float CurrentHeadGazeWeight
    {
        get { return m_CurrentHeadGazeWeight; }
        set { m_CurrentHeadGazeWeight = value; }
    }
    override protected float CurrentEyeGazeWeight
    {
        get { return m_CurrentEyeGazeWeight; }
        set { m_CurrentEyeGazeWeight = value; }
    }
    override protected float CurrentBodyGazeWeight
    {
        get { return m_CurrentBodyGazeWeight; }
        set { m_CurrentBodyGazeWeight = value; }
    }
    override protected float CurrentTotalGazeWeight
    {
        get { return m_CurrentTotalGazeWeight; }
        set { m_CurrentTotalGazeWeight = value; }
    }
    #endregion

    #region Functions
    void Start()
    {
        Setup();

        if (m_GazeTarget != null)
        {
            InitGaze(m_GazeTarget);
			SetGazeTargetWithSpeed(m_GazeTarget, GazeParts.All);
        }
    }

    void Setup()
    {
        if (m_Animator == null)
        {
            m_Animator = GetComponentInChildren<Animator>();
            if (m_Animator == null)
            {
                Debug.LogError("No animator found in hierarchy of " + name + ". Gaze won't work");
            }
        }
    }

    protected override void InitGaze(GameObject gazeTarget)
    {
        Setup();
        m_CurrentTotalGazeWeight = 1;
        base.InitGaze(gazeTarget);
    }

    public override void UpdateGaze()
    {
        if (m_GazeTarget != null && m_Animator != null)
        {
            m_Animator.SetLookAtPosition(GetGazePosition());
            m_Animator.SetLookAtWeight(m_CurrentTotalGazeWeight, m_CurrentBodyGazeWeight, m_CurrentHeadGazeWeight, m_CurrentEyeGazeWeight);
        }
    }

    void OnAnimatorIK(int layer)
    {
        UpdateGaze();
    }
    #endregion
}
