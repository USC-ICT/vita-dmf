using UnityEngine;
using System.Collections;

public class AGSetAmbientLight : MonoBehaviour
{
    public Color ambientValue = new Color(1, 1, 1, 1);

    void OnEnable()
    {
        RenderSettings.ambientLight = ambientValue;
    }
}
