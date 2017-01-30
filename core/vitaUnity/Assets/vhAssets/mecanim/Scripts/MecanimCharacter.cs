/// <summary>
///
/// </summary>

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

//[RequireComponent(typeof(Animator))]

//Name of class must be name of file as well

[RequireComponent(typeof(FacialAnimationPlayer_Animator))]
[RequireComponent(typeof(HeadController))]
[RequireComponent(typeof(GazeController_IK))]
[RequireComponent(typeof(SaccadeController))]
public class MecanimCharacter : ICharacter
{
    #region Constants

    #endregion

    #region Variables
    [SerializeField] string m_StartingPosture = "";
    [SerializeField] int m_BaseLayerIndex = 0;
    [SerializeField] int m_UpperBodyLayerIndex = 1;
    [SerializeField] int m_FaceLayerIndex = 2;

    protected Animator animator;
    protected FacialAnimationPlayer m_FacialAnimator;
    protected HeadController m_HeadController;
    protected GazeController m_GazeController;
    protected SaccadeController m_SaccadeController;
    #endregion

    #region Properties
    public override string CharacterName
    {
        get { return name; }
    }

    public override AudioSource Voice
    {
        get { return GetComponentInChildren<AudioSource>(); }
    }

    public int BaseLayerIndex {  get { return m_BaseLayerIndex; } }
    public int UpperBodyLayerIndex { get { return m_UpperBodyLayerIndex; } }
    public int FaceLayerIndex { get { return m_FaceLayerIndex; } }
    #endregion

    #region Functions
    // Use this for initialization
    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        if (animator == null)
        {
            Debug.LogError("MecanimCharacter " + name + " doesn't have an Animator component");
        }
        else
        {
            // the animator needs this component in order to signal when it receives unity driver function messages
            // such as OnAvatarIK and OnStateIK
            AnimatorMessenger messenger = animator.GetComponent<AnimatorMessenger>();
            if (messenger == null)
            {
                messenger = animator.gameObject.AddComponent<AnimatorMessenger>();
            }
            messenger.SetMessengerTarget(this);
        }

        m_FacialAnimator = GetComponent<FacialAnimationPlayer>();
        m_HeadController = GetComponent<HeadController>();
        m_GazeController = GetComponent<GazeController>();
        m_SaccadeController = GetComponent<SaccadeController>();

        if (!string.IsNullOrEmpty(m_StartingPosture))
        {
            animator.Play(m_StartingPosture, m_BaseLayerIndex);
        }
    }

    public void SetFloatParam(string paramName, float paramData)
    {
        animator.SetFloat(paramName, paramData);
    }

    public void SetBoolParam(string paramName, bool paramData)
    {
        animator.SetBool(paramName, paramData);
    }

    public void SetIntParam(string paramName, int paramData)
    {
        animator.SetInteger(paramName, paramData);
    }

    public void MoveToPoint(Vector3 point, Quaternion rot)
    {
        animator.SetFloat("Speed", 1);
        //animator.SetFloat("Direction", h, DirectionDampTime, Time.deltaTime);
        //animator.MatchTarget(point, rot, AvatarTarget.Root, 1.0f);
    }

    IEnumerator DoMoveToPoint()
    {
        yield break;
    }

    public void PlayPosture(string postureName)
    {
        PlayPosture(postureName, 0);
    }

    public override void PlayPosture(string postureName, float startTime)
    {
        if (animator == null)
        {
            Debug.LogError("null animator: " + name);
        }
        animator.CrossFadeInFixedTime(postureName, 0.5f, m_BaseLayerIndex);
    }

    public override void PlayAnim(string animName)
    {
        PlayAnim(animName, m_UpperBodyLayerIndex);
    }

    public void PlayAnim(string animName, int layer)
    {
        if (animator == null)
        {
            Debug.LogError("null animator: " + name);
        }
        animator.CrossFadeInFixedTime(animName, 0.5f, layer);
    }

    public override void PlayAnim(string animName, float readyTime, float strokeStartTime, float emphasisTime, float strokeTime, float relaxTime)
    {
        PlayAnim(animName);
    }

    public void PlayAnim(string animName, float startDelay)
    {
        StartCoroutine(PlayAnimDelayed(startDelay, animName));
    }

    IEnumerator PlayAnimDelayed(float delay, string animName)
    {
        yield return new WaitForSeconds(delay);
        animator.CrossFadeInFixedTime(animName, 0.5f, m_UpperBodyLayerIndex);
    }

    public override void PlayXml(string xml)
    {
        BMLEventHandler bmlHandler = GetComponent<BMLEventHandler>();
        if (bmlHandler != null)
        {
            bmlHandler.LoadXMLString(CharacterName, xml);
        }
        else
        {
            Debug.LogError("PlayXml function failed on character " + name + ". Add BMLEventHandler to the gameobject.");
        }
    }

    public override void PlayXml(AudioSpeechFile xml)
    {
        PlayXml(xml.ConvertedXml);
    }

    public void PlayAU(int au, string side, float weight, float time)
    {
        //animator.SetFloat(au,
    }

    public override void PlayViseme(string viseme, float weight)
    {
        animator.SetFloat(viseme, weight);
    }

    public override void PlayViseme(string viseme, float weight, float totalTime, float blendTime)
    {
        m_FacialAnimator.RampViseme(viseme, weight, 0, totalTime, blendTime);
    }

    public void SetVisemeWeightMultiplier(float multiplier)
    {
        m_FacialAnimator.VisemeWeightMultiplier = multiplier;
    }

    public override void PlayAudio(AudioSpeechFile speechFile)
    {
        // often times, the facial curves need to start before the audio starts playing
        // find the most negative curve start time and wait that long before playing the audio
        //float audioWaitTime = speechFile.UtteranceTiming.GetEarliestCurveTime();
        AudioSource src = Voice;
        if (src != null)
        {
            src.clip = speechFile.m_AudioClip;
            //src.PlayDelayed(Mathf.Abs(audioWaitTime));
            src.Play();
        }

        m_FacialAnimator.Play(speechFile.UtteranceTiming);
    }

    public void PlayAudio(List<TtsReader.WordTiming> timings)
    {
        m_FacialAnimator.Play(timings);
    }

    public void SetGazeTarget(GameObject gazeTarget)
    {
        if (gazeTarget != null)
        {
            m_GazeController.SetGazeTarget(gazeTarget);
        }
    }

    GameObject FindGazeTarget(string gazeAt)
    {
        GameObject gazeTarget = GameObject.Find(gazeAt);
        if (gazeTarget == null)
        {
            Debug.LogError("Could not find gaze target " + gazeAt);
        }
        return gazeTarget;
    }
       

    public override void Gaze(string gazeAt)
    {
        GameObject gazeTarget = FindGazeTarget(gazeAt);
        if (gazeTarget != null)
        {
            SetGazeTarget(gazeTarget);
        }
    }

    public override void Gaze(string gazeAt, float headSpeed)
    {
        GameObject gazeTarget = FindGazeTarget(gazeAt);
        if (gazeTarget != null)
        {
            SetGazeTargetWithSpeed(gazeTarget, headSpeed, 0, 0);
        }
    }

    public override void Gaze(string gazeAt, float headSpeed, float eyeSpeed, CharacterDefines.GazeJointRange jointRange)
    {
        GameObject gazeTarget = FindGazeTarget(gazeAt);
        if (gazeTarget != null)
        {
            float bodySpeed = ((jointRange & CharacterDefines.GazeJointRange.CHEST) == CharacterDefines.GazeJointRange.CHEST) ? GazeController.DefaultBodyGazeSpeed : 0;
            SetGazeTargetWithSpeed(gazeTarget, headSpeed, eyeSpeed, bodySpeed);
        }
    }

    public override void Gaze(string gazeAt, string targetBone, CharacterDefines.GazeDirection gazeDirection, CharacterDefines.GazeJointRange jointRange,
        float angle, float headSpeed, float eyeSpeed, float fadeOut, string gazeHandleName, float duration)
    {
        Gaze(gazeAt, headSpeed, eyeSpeed, jointRange);
        if (duration > 0)
        {
            StopGazeLater(duration, fadeOut);
        }
    }

    public void StopGazeLater(float secondsToWait)
    {
        StartCoroutine(StopGazeLaterCR(secondsToWait, GazeController.DefaultFadeOutTime));
    }

    public void StopGazeLater(float secondsToWait, float fadeOutTime)
    {
        StartCoroutine(StopGazeLaterCR(secondsToWait, fadeOutTime));
    }
        
    IEnumerator StopGazeLaterCR(float secondsToWait, float fadeOutTime)
    {
        yield return new WaitForSeconds(secondsToWait);
        StopGaze(fadeOutTime);
    }

    public void SetGazeTargetWithSpeed(GameObject gazeTarget, float headSpeed, float eyesSpeed, float bodySpeed)
    {
        m_GazeController.SetGazeTargetWithSpeed(gazeTarget, headSpeed, eyesSpeed, bodySpeed);
    }

    public void SetGazeTargetWithTime(GameObject gazeTarget, float headFadeInTime, float eyesFadeInTime, float bodyFadeInTime)
    {
        m_GazeController.SetGazeTargetWithDuration(gazeTarget, headFadeInTime, eyesFadeInTime, bodyFadeInTime);
    }

    public void SetGazeWeights(float head, float eyes, float body)
    {
        m_GazeController.HeadGazeWeight = head;
        m_GazeController.EyeGazeWeight = eyes;
        m_GazeController.BodyGazeWeight = body;
    }

    public override void StopGaze()
    {
        m_GazeController.StopGaze();
    }

    public override void StopGaze(float fadeoutTime)
    {
        m_GazeController.StopGaze(fadeoutTime);
    }

    public void UpdateGaze()
    {
        m_GazeController.UpdateGaze();
    }

    public override void Nod(float amount, float numTimes, float duration)
    {
        m_HeadController.NodHead(amount, numTimes, duration);
    }

    public override void Shake(float amount, float numTimes, float duration)
    {
        m_HeadController.ShakeHead(amount, numTimes, duration);
    }

    public void Tilt(float amount, float numTimes, float duration)
    {
        m_HeadController.TiltHead(amount, numTimes, duration);
    }

    public override void Saccade(CharacterDefines.SaccadeType type, bool finish, float duration)
    {
        SetSaccadeBehaviour(type);
    }

    public override void StopSaccade()
    {
        m_SaccadeController.SetBehaviourMode(CharacterDefines.SaccadeType.End);
    }

    public override void Saccade(CharacterDefines.SaccadeType type, bool finish, float duration, float angleLimit, float direction, float magnitude)
    {
        SetSaccadeBehaviour(type);
        m_SaccadeController.Perform(direction, magnitude, duration);
    }

    public void Saccade(float direction, float magnitude, float duration)
    {
        m_SaccadeController.Perform(direction, magnitude, duration);
    }

    public void SetSaccadeBehaviour(CharacterDefines.SaccadeType mode)
    {
        m_SaccadeController.SetBehaviourMode(mode);
    }

    public override void Transform(float x, float y, float z)
    {
        transform.position = new Vector3(x, y, z);
    }

    public override void Transform(float y, float p)
    {
        transform.position = new Vector3(transform.position.x, y, transform.position.z);
        Vector3 currRot = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(currRot.x, p, currRot.z);
    }

    public override void Transform(float x, float y, float z, float h, float p, float r)
    {
        Transform(x, y, z);
        transform.rotation = Quaternion.Euler(p, h, r);
    }

    public override void Transform(Transform trans)
    {
        transform.position = trans.position;
        transform.rotation = trans.rotation;
    }

    public override void Transform(Vector3 pos, Quaternion rot)
    {
        transform.position = pos;
        transform.rotation = rot;
    }

    public override void Rotate(float h)
    {
        Vector3 currRot = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(currRot.x, h, currRot.z);
    }


    #endregion
}
