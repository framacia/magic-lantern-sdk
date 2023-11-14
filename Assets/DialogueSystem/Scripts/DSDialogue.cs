using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace DS
{
    using Enumerations;
    using ScriptableObjects;
    using System.Collections;

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

        //Behaviour
        [SerializeField] private bool autoContinueSingleChoice;

        //UnityEvent
        [SerializeField] private UnityEvent OnDialogueFinishedEvent;

        private AudioSource audioSource;

        private void Start()
        {
            audioSource = GetComponent<AudioSource?>();

            startingDialogue = dialogue;

            //Remove text from displayTexts
            dialogueDisplayText.text = string.Empty;
            foreach (TMP_Text choiceDisplayText in choiceDisplayTexts)
                choiceDisplayText.gameObject.SetActive(false);
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
                dialogue = nextDialogue;
                DisplayTextCurrentDialogue();
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
            yield return new WaitForSeconds(audioSource.clip.length + 2f);

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
            }
            else //Otherwise deactivate choice text
            {
                foreach (TMP_Text text in choiceDisplayTexts)
                    text.gameObject.SetActive(false);
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
            OnDialogueFinishedEvent?.Invoke();
        }

        public void ChangeDialogueContainer(DSDialogueContainerSO newDialogueContainer)
        {
            dialogueContainer = newDialogueContainer;
        }
    }
}