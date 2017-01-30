// Utilities for working in the "TestMarkupScene" scene.

using UnityEngine;


public class AGTestMarkupSceneHelper : MonoBehaviour
{
    #region Variables
    // Character transfrom
    public Transform CharacterTransform;

    // Fake toggles
    public bool SnapToChair = false;
    bool toggleChairCurrentState;

    public bool SnapToStart = false;
    bool toggleStartCurrentState;

    // Locations
    public Transform transLocChair;
    public Transform transLocStart;
    #endregion


    void Start()
    {
    }


    void Update()
    {
        // chair toggle
        if (SnapToChair != toggleChairCurrentState)
        {
            // Move object
            if (CharacterTransform == null)
            {
                Debug.LogWarning("Nothing specified for <color=orange>CharacterTransform</color> parameter. Nothing happened.");
                SnapToChair = false;
                return;
            }
            CharacterTransform.position = transLocChair.position;
            toggleChairCurrentState = SnapToChair;

            // Toggle other button
            SnapToStart = false;
            toggleStartCurrentState = false;
        }

        // start toggle
        if (SnapToStart != toggleStartCurrentState)
        {
            // Move object
            if (CharacterTransform == null)
            {
                Debug.LogWarning("Nothing specified for <color=orange>CharacterTransform</color> parameter. Nothing happened.");
                SnapToStart = false;
                return;
            }
            CharacterTransform.position = transLocStart.position;
            toggleStartCurrentState = SnapToStart;

            // Toggle other button
            SnapToChair = false;
            toggleChairCurrentState = false;
        }
    }
}
