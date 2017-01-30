using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

public class SpeechRecognizer_Dragon : SpeechRecognizer
{
    #region Constants

    #endregion

    #region Variables
    public string m_User = "adam";
    public string m_Topic = "General - Medium";
    public int m_TimeOut = 30;
    #endregion

    #region Functions
    void Start()
    {

    }

    protected override void PerformRecognition(AudioClip clip)
    {
        string wavPath = VHFile.GetStreamingAssetsPath() + "Dragon/testwav.wav";
        AudioConverter.ConvertClipToWav(clip, wavPath);
        StartCoroutine(TranscribeAudio(m_User, m_Topic, wavPath, Path.ChangeExtension(wavPath, ".txt"), m_TimeOut));
    }

    IEnumerator TranscribeAudio(string user, string topic, string wavPath, string outputTextPath, int timeout)
    {
        if (VHUtils.IsWebGL())
            yield break;

        System.Diagnostics.Process transcriptionProcess = new System.Diagnostics.Process();
        transcriptionProcess.StartInfo.FileName = VHFile.GetStreamingAssetsPath() + "Dragon/trscribe_ftof.exe";
        transcriptionProcess.StartInfo.Arguments = string.Format(@"-username=""{0}"" -topicname=""{1}"" -wavefile=""{2}"" -outfile=""{3}"" -timeout={4}",
            string.Format("{0}Dragon/Users/{1}", VHFile.GetStreamingAssetsPath(), user), topic, wavPath, outputTextPath, timeout);
#if !UNITY_WEBGL
        transcriptionProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
#endif
        transcriptionProcess.Start();
        //transcriptionProcess.WaitForExit();

#if !UNITY_WEBGL
        while (!transcriptionProcess.HasExited)
        {
            yield return new WaitForEndOfFrame();
        }
#endif

        // read the transcription
        string transcription = "";
        if (File.Exists(outputTextPath))
        {
            transcription = VHFile.FileWrapper.ReadAllText(outputTextPath);
        }

        List<RecognizerResult> recognizerResults = new List<RecognizerResult>();
        recognizerResults.Add(new RecognizerResult(transcription));
        DispatchResults(recognizerResults);
    }
    #endregion
}
