using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SimpleJSON;

public class SpeechRecognizer_GoogleASR : SpeechRecognizer
{
    #region Constants
    const string AsrUrl = "https://www.google.com/speech-api/v2/recognize";
    #endregion

    #region Variables
    //public string m_ClientName = "chromium";
    public string m_Language = "en-US";
    public int m_MaxResults = 3;
    string key = "0123456789abcdefghijklmnopqrstuvwxyzABC";
    #endregion

    #region Functions
    protected override void PerformRecognition(AudioClip clip)
    {
        base.PerformRecognition(clip);

        StartCoroutine(MakeWebRequest(clip));
    }

    IEnumerator MakeWebRequest(AudioClip clip)
    {
        List<RecognizerResult> recognizerResults = new List<RecognizerResult>();

        // for information on google url get and post params, go here: https://github.com/gillesdemey/google-speech-v2
        string url = string.Format("{0}?lang={1}&pfilter=0&maxresults={2}&key={3}&output=json&client=chromium&pfilter=2", AsrUrl, m_Language, m_MaxResults, key);

        // Google ASR requires the audio data to be in flac format
        byte[] flacData = AudioConverter.ConvertClipToFlac(clip, VHFile.GetStreamingAssetsPath() + "Flac/testwav.wav");

        WWWForm form = new WWWForm();
        form.AddBinaryData("body", flacData);

#if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 ||UNITY_3_3 ||UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3
        Hashtable headers = form.headers;
        headers["Content-Type"] = "audio/x-flac; rate=" + clip.frequency;
        headers["charset"] = "utf-8";
        headers["Content-Length"] = "" + flacData.Length;
#else
        Dictionary<string, string> headers = new Dictionary<string, string>();
        headers.Add("Content-Type", "audio/x-flac; rate=" + clip.frequency);
        headers.Add("charset", "utf-8");
        headers.Add("Content-Length", "" + flacData.Length);
#endif

        // make the request and wait
        WWW www = new WWW(url, form.data, headers);
        yield return www;

        if (!string.IsNullOrEmpty(www.error))
        {
            Debug.LogError("GoogleASR webrequest error: " + www.error);
            DispatchResults(recognizerResults);
            yield break;
        }
        else if (string.IsNullOrEmpty(www.text))
        {
            //Debug.LogError("GoogleASR webrequest didn't return anything");
            DispatchResults(recognizerResults);
            yield break;
        }

        // parse the json results
        //Debug.Log(www.text);

        JSONNode node = JSON.Parse(www.text);

        JSONArray results = node["result"].AsArray;
        for (int resultIndex = 0; resultIndex < results.Count; resultIndex++)
        {
            JSONArray alternativeArray = results[resultIndex]["alternative"].AsArray;

            for (int alternativeIndex = 0; alternativeIndex < alternativeArray.Count; alternativeIndex++)
            {
                float confidence = 0;
                JSONNode currNode = alternativeArray[alternativeIndex];
                if (currNode["confidence"] != null)
                {
                    confidence = currNode["confidence"].AsFloat;
                }
                //Debug.Log(currNode["utterance"] + " " + confidence);
                recognizerResults.Add(new RecognizerResult(currNode["transcript"], confidence));
            }
        }

        DispatchResults(recognizerResults);
    }
    #endregion
}
