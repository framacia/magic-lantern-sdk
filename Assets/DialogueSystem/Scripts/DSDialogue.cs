using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace DS
{
    using Enumerations;
    using ScriptableObjects;
    using System;

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

        private void Start()
        {
            startingDialogue = dialogue;
            DisplayTextCurrentDialogue();
        }

        public void GoToNextDialogue(int choiceIndex)
        {
            DSDialogueSO nextDialogue = dialogue.Choices[choiceIndex].NextDialogue;
            if (nextDialogue != null)
            {
                dialogue = nextDialogue;
                DisplayTextCurrentDialogue();
            }
            else
            {
                DialogueFinished(); //Can use this to trigger events
            }
        }

        private void DialogueFinished()
        {
            throw new NotImplementedException();
        }

        private void DisplayTextCurrentDialogue()
        {
            dialogueDisplayText.text = dialogue.Text;

            //If there are choices, display them
            if(dialogue.DialogueType == DSDialogueType.MultipleChoice)
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

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                if (dialogue.DialogueType == DSDialogueType.SingleChoice)
                    GoToNextDialogue(0);
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                RestartDialogue();
            }

        }

        public void RestartDialogue()
        {
            dialogue = startingDialogue;
            DisplayTextCurrentDialogue();
        }
    }
}