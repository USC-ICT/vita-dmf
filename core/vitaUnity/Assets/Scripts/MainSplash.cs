using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text;

public class MainSplash : MonoBehaviour
{
    [Serializable]
    public class SplashInfo
    {
        public Texture2D splashScreen;
        public float timeout;
        public float fadeout;
    }


    public SplashInfo [] m_splashScreens;
    int m_currentSplash = 0;
    float m_currentSplashStartTime;
    float m_currentSplashAlpha = 1.0f;


    void Start()
    {
        Application.targetFrameRate = 60;
        Application.runInBackground = true;


        m_currentSplashStartTime = Time.time;
    }


    void Update()
    {
        if (m_currentSplash < m_splashScreens.Length)
        {
            m_currentSplashAlpha = Time.time - m_currentSplashStartTime - m_splashScreens[m_currentSplash].timeout;
            if (m_currentSplashAlpha <= 0)
            {
                m_currentSplashAlpha = 1.0f;
            }
            else
            {
                m_currentSplashAlpha = 1.0f - (m_currentSplashAlpha / m_splashScreens[m_currentSplash].fadeout);
            }

            //Debug.Log(string.Format("{0} - {1} - {2} - {3}", Time.time, m_currentSplashStartTime, m_splashScreens[m_currentSplash].timeout, m_currentSplashAlpha));


            if (Time.time - m_currentSplashStartTime > m_splashScreens[m_currentSplash].timeout + m_splashScreens[m_currentSplash].fadeout)
            {
                m_currentSplash++;
                m_currentSplashStartTime = Time.time;
            }
        }

        if (m_currentSplash >= m_splashScreens.Length)
        {
            VHUtils.SceneManagerLoadScene("CDKey");
        }


        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            VHUtils.SceneManagerLoadScene("CDKey");
        }


        if (Input.GetKeyDown(KeyCode.Escape))
        {
            VHUtils.ApplicationQuit();
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            GameObject.FindObjectOfType<DebugInfo>().NextMode();
        }
    }


    void OnGUI()
    {
        if (m_currentSplash < m_splashScreens.Length)
        {
            GUI.color = new Color(1, 1, 1, m_currentSplashAlpha);
            Rect r;
            r = new Rect(0, 0, Screen.width, Screen.height);
            GUI.DrawTexture(r, m_splashScreens[m_currentSplash].splashScreen, ScaleMode.ScaleAndCrop, true);
            GUI.color = Color.white;
        }
    }
}
