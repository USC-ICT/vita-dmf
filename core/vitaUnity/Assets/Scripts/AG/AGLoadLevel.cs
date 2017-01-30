using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Provide options to load level additively, merge, or change scenes through the inspector.
/// </summary>
public class AGLoadLevel : MonoBehaviour
{
    public bool m_loadOnStart = true;
    public bool m_additiveLoad = true;
    public bool m_additiveLoadMerge = false;
    public Object m_level;
    bool m_merged = false;

    #region Private Functions
    void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (m_additiveLoadMerge)
        {
            StartCoroutine("MergeScenes");
        }
    }

    IEnumerator MergeScenes()
    {
        yield return new WaitForEndOfFrame();
        if (!m_merged)
        {
            SceneManager.MergeScenes(SceneManager.GetSceneByName(m_level.name), SceneManager.GetActiveScene());
            m_merged = true;
            StopCoroutine("MergeScenes");
        }
    }

    void LoadInternal()
    {
        if (m_level != null)
        {
            //Reset merge state
            m_merged = false;

            if (m_additiveLoad)
            {
                SceneManager.LoadScene(m_level.name, LoadSceneMode.Additive);
                SceneManager.sceneLoaded += OnSceneLoaded;
            }
            else
            {
                SceneManager.LoadScene(m_level.name, LoadSceneMode.Single);
            }
        }
    }
    #endregion

    void Start()
    {
        if (m_loadOnStart)
        {
            Load();
        }
    }

    public void Load()
    {
        LoadInternal();
    }

    public void Load(bool additiveLoad, bool additiveLoadMerge)
    {
        m_additiveLoad = additiveLoad;
        m_additiveLoadMerge = additiveLoadMerge;
        LoadInternal();
    }

    public void Load(Object level, bool additiveLoad, bool additiveLoadMerge)
    {
        m_level = level;
        m_additiveLoad = additiveLoad;
        m_additiveLoadMerge = additiveLoadMerge;
        LoadInternal();
    }
}
