using UnityEngine;
using UnityEngine.UI;

public class AGGuiToggleExpandCollapse : MonoBehaviour
{
    Image m_image;

    void Start()
    {
        m_image = this.GetComponent<Image>();
    }

    public void SetImageState(bool m_enabled)
    {
        m_image.enabled = m_enabled;
    }

    public void ToggleImageState()
    {
        m_image.enabled = !m_image.enabled;
    }

    public void ToggleGO(GameObject m_toggledGameObject)
    {
        m_toggledGameObject.SetActive(!m_toggledGameObject.activeInHierarchy);
    }
}
