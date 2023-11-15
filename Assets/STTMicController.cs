using System.Collections;
using System.Collections.Generic;
using System.Drawing.Text;
using UnityEngine;
using DS;

public class STTMicController : MonoBehaviour
{
    [SerializeField] VoskSpeechToText voskSTT;
    [SerializeField] DSDialogue dsDialogue;

    [Header("UI")]
    [SerializeField] GameObject recordingIcon;

    private bool isRecording = false;

    void Awake()
    {
        voskSTT.OnTranscriptionResult += OnTranscriptionResult;
    }

    private void Start()
    {
        recordingIcon.SetActive(false);
    }

    private void OnTranscriptionResult(string obj)
    {
        var result = new RecognitionResult(obj);
        //for (int i = 0; i < result.Phrases.Length; i++)
        //{
        //    if (i > 0)
        //    {
        //        ResultText.text += "\n ---------- \n";
        //    }

        //    ResultText.text += result.Phrases[0].Text + " | " + "Confidence: " + result.Phrases[0].Confidence;
        //}

        dsDialogue.CheckTranscriptionResult(result.Phrases[0].Text);
    }

    public void ToggleRecording()
    {
        //Change bool
        isRecording = !isRecording;

        recordingIcon.SetActive(isRecording);
        voskSTT?.ToggleRecording();
    }
}
