using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class AnimatorPoller : MonoBehaviour
{
    #region Variables
    [SerializeField] Animator m_Animator;
    [SerializeField] Slider m_Slider;
    [SerializeField] string m_FloatParamName = "";
    #endregion

    #region Functions
    void Start()
    {

    }

    void Update()
    {
        m_Slider.onValueChanged.SetPersistentListenerState(0, UnityEngine.Events.UnityEventCallState.Off);
        m_Slider.value = m_Animator.GetFloat(m_FloatParamName);
        m_Slider.onValueChanged.SetPersistentListenerState(0, UnityEngine.Events.UnityEventCallState.RuntimeOnly);
    }
    #endregion
}
