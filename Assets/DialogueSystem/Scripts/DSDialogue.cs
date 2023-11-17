using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

namespace DS
{
    using Enumerations;
    using ScriptableObjects;
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Text;
    using Unity.VisualScripting;

    public class DSDialogue : MonoBehaviour
    {
        /* Dialogue Scriptable Objects */
        [SerializeField] private DSDialogueContainerSO dialogueContainer;
        [SerializeField] private DSDialogueGroupSO dialogueGroup;
        [SerializeField] private DSDialogueSO dialogue;
        [SerializeField] private DSDialogueSO startingDialogue;

        /* Filters */
        [SerializeField] private bool groupedDialogues;
        [SerializeField] private bool startingDialoguesOnly;

        /* Indexes */
        [SerializeField] private int selectedDialogueGroupIndex;
        [SerializeField] private int selectedDialogueIndex;

        // Dialogue Text
        [SerializeField] private TMP_Text dialogueDisplayText;
        [SerializeField] private List<TMP_Text> choiceDisplayTexts;

        // Behaviour
        enum DialogueInteractionType
        {
            Selection,
            Speaking
        }
        [SerializeField] private DialogueInteractionType dialogueInteractionType = DialogueInteractionType.Selection;

        [SerializeField] private bool autoContinueSingleChoice;
        [SerializeField] private float secondsToAutoContinue;

        // Default Answers
        [SerializeField] private AudioClip didNotHearClip;
        [SerializeField] private AudioClip didNotUnderstandClip;

        // UnityEvent
        [SerializeField] private UnityEvent OnDialogueFinishedEvent;

        //STT Vosk
        [SerializeField] private STTMicController sttMicController;

        private AudioSource audioSource;

        private void Start()
        {
            audioSource = GetComponent<AudioSource?>();

            startingDialogue = dialogue;

            //If Interaction Type is not Selection, deactivate choice camera pointed objects
            if (dialogueInteractionType != DialogueInteractionType.Selection)
            {
                foreach (var choiceText in choiceDisplayTexts)
                {
                    choiceText.GetComponent<CameraPointedObject?>().enabled = false;
                }
            }

            //Remove text from displayTexts
            dialogueDisplayText.text = string.Empty;
            foreach (TMP_Text choiceDisplayText in choiceDisplayTexts)
                choiceDisplayText.gameObject.SetActive(false);

            sttMicController.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.J))
            {
                RestartDialogue();
            }
        }

        public void GoToNextDialogue(int choiceIndex)
        {
            DSDialogueSO nextDialogue = dialogue.Choices[choiceIndex].NextDialogue;
            if (nextDialogue != null)
            {
                if (dialogue.DialogueType == DSDialogueType.MultipleChoice)
                {
                    Debug.Log($"Dialogue Choice {dialogue.Choices[choiceIndex].Text} was selected");
                }

                dialogue = nextDialogue;
                DisplayTextCurrentDialogue();

                //Stop then Start Audio
                StopCoroutine(PlayAudioCurrentDialogue());
                StartCoroutine(PlayAudioCurrentDialogue());
            }
            else
            {
                DialogueFinished(); //Can use this to trigger events
                dialogueDisplayText.text = "";
            }
        }

        private IEnumerator PlayAudioCurrentDialogue()
        {
            if (dialogue.AudioClip == null || audioSource == null)
                yield break;

            //Stop current clip if there was one
            audioSource.Stop();
            audioSource.clip = dialogue.AudioClip;
            audioSource.Play();

            //Wait for current clip to end + silence offset
            yield return new WaitForSeconds(audioSource.clip.length + secondsToAutoContinue);

            //If it's single choice and autoContinue activated, go to Next Dialogue
            if (dialogue.DialogueType == DSDialogueType.SingleChoice && autoContinueSingleChoice)
            {
                GoToNextDialogue(0);
            }
        }

        private void DisplayTextCurrentDialogue()
        {
            dialogueDisplayText.text = dialogue.Text;

            //If there are choices, display them
            if (dialogue.DialogueType == DSDialogueType.MultipleChoice)
            {
                for (int i = 0; i < dialogue.Choices.Count; i++)
                {
                    choiceDisplayTexts[i].gameObject.SetActive(true);
                    choiceDisplayTexts[i].text = dialogue.Choices[i].Text;
                }

                if (dialogueInteractionType == DialogueInteractionType.Speaking)
                {
                    //Activate STT Mic
                    sttMicController.gameObject.SetActive(true);                    

                    //Optional, start recording
                    //StartCoroutine(sttMicController.ToggleRecording(3));
                }
            }
            else //Otherwise deactivate choice text
            {
                foreach (TMP_Text text in choiceDisplayTexts)
                    text.gameObject.SetActive(false);

                //Deactivate STT Mic
                sttMicController.gameObject.SetActive(false);
            }
        }

        public void RestartDialogue()
        {
            dialogue = startingDialogue;
            DisplayTextCurrentDialogue();
            StopCoroutine(PlayAudioCurrentDialogue());
            StartCoroutine(PlayAudioCurrentDialogue());
        }

        private void DialogueFinished()
        {
            Debug.Log($"Dialogue {dialogueContainer.FileName} finished");
            OnDialogueFinishedEvent?.Invoke();
        }

        public void ChangeDialogueContainer(DSDialogueContainerSO newDialogueContainer)
        {
            dialogueContainer = newDialogueContainer;
        }

        public void CheckTranscriptionResult(string result)
        {
            //Nothing was received
            if (string.IsNullOrEmpty(result))
            {
                //Say that you have not heard the person
                StartCoroutine(DisplayDefaultAnswer(0));
                return;
            }

            var compareInfo = CultureInfo.InvariantCulture.CompareInfo;

            List<int> possibleMatchesIndexes = new List<int>();

            //Loop through all choice texts to find matches
            for (int i = 0; i < dialogue.Choices.Count; i++)
            {
                if (RemoveDiacritics(result).Contains(RemoveDiacritics(dialogue.Choices[i].Text), StringComparison.InvariantCultureIgnoreCase))
                {
                    //Success! Add to match list
                    possibleMatchesIndexes.Add(i);
                }
            }

            //If more than one match, or none, ask again
            if (possibleMatchesIndexes.Count != 1)
            {
                //Ask again, say the answer was not clear
                StartCoroutine(DisplayDefaultAnswer(1));
            }
            else //If one match exactly, answer accepted
            {
                GoToNextDialogue(possibleMatchesIndexes[0]);
            }
        }

        private IEnumerator DisplayDefaultAnswer(int defaultAnswerIndex)
        {
            yield return new WaitForSeconds(1);

            AudioClip defaultAnswerClip;
            string defaultAnswerText;

            switch (defaultAnswerIndex)
            {
                case 0:
                    defaultAnswerClip = didNotHearClip;
                    defaultAnswerText = "Perdona, no te he oído, ¿puedes volver a hablar?";
                    break;
                case 1:
                    defaultAnswerClip = didNotUnderstandClip;
                    defaultAnswerText = "Disculpa, no te he entendido, ¿puedes responder de nuevo?";
                    break;
                default:
                    defaultAnswerClip = didNotUnderstandClip;
                    defaultAnswerText = "Disculpa, no te he entendido, ¿puedes responder de nuevo?";
                    break;
            }

            //Display/say default answer
            dialogueDisplayText.text = defaultAnswerText;

            //Stop current clip if there was one
            audioSource.Stop();
            audioSource.clip = defaultAnswerClip;
            audioSource.Play();

            yield break;
            //I added this to repeat the question, but maybe it's weird to do that

            //yield return new WaitForSeconds(defaultAnswerClip.length + 3);

            ////Go back to choice dialogue
            //DisplayTextCurrentDialogue();

            ////Stop then Start Audio
            //StopCoroutine(PlayAudioCurrentDialogue());
            //StartCoroutine(PlayAudioCurrentDialogue());
        }

        static string RemoveDiacritics(string text)
        {
            string formD = text.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();

            foreach (char ch in formD)
            {
                UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (uc != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(ch);
                }
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}