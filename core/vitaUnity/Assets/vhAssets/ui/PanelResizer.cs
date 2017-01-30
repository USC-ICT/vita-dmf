using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;

public class PanelResizer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    #region Constants
    const string XPosKey = ":x";
    const string YPosKey = ":y";
    const string WidthKey = ":w";
    const string HeightKey = ":h";
    const string LastUsedResKey = "LastUsedRes";

    [System.Serializable]
    public class RectResizer
    {
        [SerializeField] public RectTransform m_Rect;
        [SerializeField] public bool m_HorizontalMovement;
        [SerializeField] public bool m_VerticalMovement;
        [SerializeField] public bool m_PositionDisplacement;
    }
    #endregion

    #region Variables
    [SerializeField] bool m_SavePosition = false;
    [SerializeField] Texture2D m_MouseCursor;
    [SerializeField] RectResizer[] m_Targets;
    [SerializeField] Rect m_NormalizedDragBounds = new Rect(0, 0, 1, 1);
    #endregion

    #region Functions
    void Start()
    {
        /*
        foreach (RectResizer target in m_Targets)
        {
            target.m_NormalizedScreenBounds.x *= Screen.width;
            target.m_NormalizedScreenBounds.y *= Screen.height;
            target.m_NormalizedScreenBounds.width *= Screen.width;
            target.m_NormalizedScreenBounds.height *= Screen.height;
        }
        */

        //m_NormalizedDragBounds.x *= Screen.width;
        //m_NormalizedDragBounds.y *= Screen.height;
        //m_NormalizedDragBounds.width *= Screen.width;
        //m_NormalizedDragBounds.height *= Screen.height;
        LoadPanelSettings();
    }

    void LoadPanelSettings()
    {
        if (StringifyResolution() != PlayerPrefs.GetString(LastUsedResKey))
        {
            // they are using a different resolution, so things won't look correct, so just use the defaults
            return;
        }

        if (m_SavePosition)
        {
            foreach (RectResizer target in m_Targets)
            {
                Vector2 localPos = target.m_Rect.anchoredPosition;
                Vector2 size = target.m_Rect.sizeDelta;
                if (PlayerPrefs.HasKey(target.m_Rect.name + XPosKey))
                {
                    localPos.x = PlayerPrefs.GetFloat(target.m_Rect.name + XPosKey);
                }
                if (PlayerPrefs.HasKey(target.m_Rect.name + YPosKey))
                {
                    localPos.y = PlayerPrefs.GetFloat(target.m_Rect.name + YPosKey);
                }
                if (PlayerPrefs.HasKey(target.m_Rect.name + WidthKey))
                {
                    size.x = PlayerPrefs.GetFloat(target.m_Rect.name + WidthKey);
                }
                if (PlayerPrefs.HasKey(target.m_Rect.name + HeightKey))
                {
                    size.y = PlayerPrefs.GetFloat(target.m_Rect.name + HeightKey);
                }

                target.m_Rect.anchoredPosition = localPos;
                target.m_Rect.sizeDelta = size;
            }
        }

    }

    void SavePanelSettings()
    {
        if (m_SavePosition)
        {
            foreach (RectResizer target in m_Targets)
            {
                PlayerPrefs.SetFloat(target.m_Rect.name + XPosKey, target.m_Rect.anchoredPosition.x);
                PlayerPrefs.SetFloat(target.m_Rect.name + YPosKey, target.m_Rect.anchoredPosition.y);
                PlayerPrefs.SetFloat(target.m_Rect.name + WidthKey, target.m_Rect.sizeDelta.x);
                PlayerPrefs.SetFloat(target.m_Rect.name + HeightKey, target.m_Rect.sizeDelta.y);
                PlayerPrefs.SetString(LastUsedResKey, StringifyResolution());
            }
        }
    }

    string StringifyResolution()
    {
        return string.Format("{0}x{1}", Screen.width, Screen.height);
    }

    void OnDestroy()
    {
        SavePanelSettings();
    }
    #endregion

    #region IPointerEnterHandler implementation

    void IPointerEnterHandler.OnPointerEnter (PointerEventData eventData)
    {
        if (m_MouseCursor != null)
        {
            Cursor.SetCursor(m_MouseCursor, new Vector2(m_MouseCursor.width / 2, m_MouseCursor.height / 2), CursorMode.Auto);
        }

    }

    #endregion

    #region IPointerExitHandler implementation

    void IPointerExitHandler.OnPointerExit (PointerEventData eventData)
    {
        if (m_MouseCursor != null)
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }

    #endregion

    #region IBeginDragHandler implementation

    void IBeginDragHandler.OnBeginDrag (PointerEventData eventData)
    {
        //throw new System.NotImplementedException ();
    }

    #endregion

    bool CanDragHorizontal(PointerEventData eventData)
    {
        /*return (eventData.delta.x < 0 && eventData.position.x > m_NormalizedDragBounds.xMin)
            || (eventData.delta.x > 0 && eventData.position.x < m_NormalizedDragBounds.xMax);*/
        Rect bounds = m_NormalizedDragBounds;
        bounds.x *= Screen.width;
        bounds.y *= Screen.height;
        bounds.width *= Screen.width;
        bounds.height *= Screen.height;
        return eventData.position.x >= bounds.xMin && eventData.position.x <= bounds.xMax;
    }

    bool CanDragVertical(PointerEventData eventData)
    {
        Rect bounds = m_NormalizedDragBounds;
        bounds.x *= Screen.width;
        bounds.y *= Screen.height;
        bounds.width *= Screen.width;
        bounds.height *= Screen.height;
        return eventData.position.y >= bounds.yMin && eventData.position.y <= bounds.yMax;
    }

    #region IDragHandler implementation

    void IDragHandler.OnDrag (PointerEventData eventData)
    {
        Vector3 posDelta = Vector3.zero;
        Vector2 sizeDelta = Vector2.zero;

        foreach (RectResizer target in m_Targets)
        {
            posDelta = target.m_Rect.anchoredPosition;
            sizeDelta = target.m_Rect.sizeDelta;

            if (target.m_PositionDisplacement)
            {
                if (target.m_HorizontalMovement && CanDragHorizontal(eventData))
                {
                    posDelta.x += eventData.delta.x;
                    sizeDelta.x -= eventData.delta.x;
                }
                if (target.m_VerticalMovement && CanDragVertical(eventData))
                {
                    posDelta.y += eventData.delta.y;
                    sizeDelta.y -= eventData.delta.y;
                }
            }
            else
            {
                if (target.m_HorizontalMovement && CanDragHorizontal(eventData))
                {
                    sizeDelta.x += eventData.delta.x;
                }
                if (target.m_VerticalMovement && CanDragVertical(eventData))
                {
                    sizeDelta.y -= eventData.delta.y;
                }
            }

            target.m_Rect.anchoredPosition = posDelta;
            target.m_Rect.sizeDelta = sizeDelta;

        }

    }

    #endregion

    #region IEndDragHandler implementation

    void IEndDragHandler.OnEndDrag (PointerEventData eventData)
    {
        //throw new System.NotImplementedException ();
    }

    #endregion
}
