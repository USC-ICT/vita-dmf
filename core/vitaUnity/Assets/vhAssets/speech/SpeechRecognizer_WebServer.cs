using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;
using SimpleJSON;

public class SpeechRecognizer_WebServer : SpeechRecognizer
{
    #region Constants
    const string OutputWav = "testwav.wav";
    const string Splitter = "___MSG_SPLIT___";
    #endregion

    #region Variables
    public string m_WebServerUrl = "https://vhtoolkitwww.ict.usc.edu/VHMsgAsp/SpeechRecognizer.aspx";
    string m_ParticipantId = Guid.NewGuid().ToString();
    public string m_User = "adam";
    public string m_Topic = "General - Medium";
    #endregion

    #region Properties

    #endregion

    #region Functions

    protected override void PerformRecognition(AudioClip clip)
    {
        base.PerformRecognition(clip);

        StartCoroutine(UploadAudioClip(m_WebServerUrl, clip));
    }

    IEnumerator UploadAudioClip(string webServerUrl, AudioClip clip)
    {
        List<RecognizerResult> recognizerResults = new List<RecognizerResult>();
        WWWForm form = new WWWForm();
        SavWav.AudioData audioData = SavWav.ConvertAudioClipToAudioData(clip);

        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < audioData.buffer.Length; i++)
        {
            builder.Append(audioData.buffer[i].ToString("f4"));
            builder.Append('|');
        }
        builder = builder.Remove(builder.Length - 1, 1);

        form.AddField("User", m_User);
        form.AddField("Topic", m_Topic);
        form.AddField("Samples", (audioData.samples.ToString()));
        form.AddField("Channels", (audioData.channels.ToString()));
        form.AddField("Frequency", (audioData.frequency.ToString()));
        form.AddField("Buffer", (builder.ToString()));
        string url = string.Format("{0}?ClientNeedsResponse=true&ParticipantId={1}&IsMicInput=true", webServerUrl, m_ParticipantId);
        WWW www = new WWW(url, form);

        //Debug.Log("url: " + url);

        yield return www;

        if (!string.IsNullOrEmpty(www.error))
        {
            Debug.LogError("WebServer webrequest error: " + www.error);
            DispatchResults(recognizerResults);
            yield break;
        }
        else if (string.IsNullOrEmpty(www.text))
        {
            //Debug.LogError("GoogleASR webrequest didn't return anything");
            DispatchResults(recognizerResults);
            yield break;
        }

        //Debug.Log(www.text);

        int index = www.text.IndexOf(Splitter);
        if (index != -1)
        {
            string text = www.text.Substring(0, index);
            recognizerResults.Add(new RecognizerResult(text));
        }

        DispatchResults(recognizerResults);
    }
    #endregion
}
