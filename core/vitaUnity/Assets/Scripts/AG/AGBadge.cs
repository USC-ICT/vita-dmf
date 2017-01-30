using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class AGBadge : MonoBehaviour
{
    public string m_badgeName;
    public string m_badgeDescription;
    public Sprite m_badgeIcon;
    public Sprite m_badgeLockedIcon;
    [NonSerialized] public bool m_badgeIsUnlocked;
    [NonSerialized] public string m_dateUnlocked;
}
