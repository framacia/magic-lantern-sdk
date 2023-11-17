using System.Collections.Generic;
using UnityEditor;

namespace DS.Inspectors
{
    using Utilities;
    using ScriptableObjects;

    [CustomEditor(typeof(DSDialogue))]
    public class DSInspector : Editor
    {
        /* Dialogue Scriptable Objects */
        private SerializedProperty dialogueContainer;
        private SerializedProperty dialogueGroup;
        private SerializedProperty dialogue;

        /* Filters */
        private SerializedProperty groupedDialogues;
        private SerializedProperty startingDialoguesOnly;

        /* Indexes */
        private SerializedProperty selectedDialogueGroupIndex;
        private SerializedProperty selectedDialogueIndex;

        // Display Text
        private SerializedProperty dialogueDisplayText;
        private SerializedProperty choiceDisplayTexts;

        //Behaviour
        private SerializedProperty autoContinueSingleChoice;
        private SerializedProperty secondsToAutoContinue;
        private SerializedProperty dialogueInteractionType;

        //Default Answers
        private SerializedProperty didNotHearClip;
        private SerializedProperty didNotUnderstandClip;

        //UnityEvent
        private SerializedProperty OnDialogueFinishedEvent;

        //STT Vosk
        private SerializedProperty sttMicController;


        private void OnEnable()
        {
            dialogueContainer = serializedObject.FindProperty(nameof(dialogueContainer));
            dialogueGroup = serializedObject.FindProperty(nameof(dialogueGroup));
            dialogue = serializedObject.FindProperty(nameof(dialogue));

            groupedDialogues = serializedObject.FindProperty(nameof(groupedDialogues));
            startingDialoguesOnly = serializedObject.FindProperty(nameof(startingDialoguesOnly));

            selectedDialogueGroupIndex = serializedObject.FindProperty(nameof(selectedDialogueGroupIndex));
            selectedDialogueIndex = serializedObject.FindProperty(nameof(selectedDialogueIndex));

            dialogueDisplayText = serializedObject.FindProperty(nameof(dialogueDisplayText));
            choiceDisplayTexts = serializedObject.FindProperty(nameof(choiceDisplayTexts));

            autoContinueSingleChoice = serializedObject.FindProperty(nameof(autoContinueSingleChoice));
            secondsToAutoContinue = serializedObject.FindProperty(nameof(secondsToAutoContinue));
            dialogueInteractionType = serializedObject.FindProperty(nameof(dialogueInteractionType));

            didNotHearClip = serializedObject.FindProperty(nameof(didNotHearClip));
            didNotUnderstandClip = serializedObject.FindProperty(nameof(didNotUnderstandClip));

            sttMicController = serializedObject.FindProperty(nameof(sttMicController));

            OnDialogueFinishedEvent = serializedObject.FindProperty(nameof(OnDialogueFinishedEvent));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDialogueContainerArea();

            DSDialogueContainerSO currentDialogueContainer = (DSDialogueContainerSO) dialogueContainer.objectReferenceValue;

            if (currentDialogueContainer == null)
            {
                StopDrawing("Select a Dialogue Container to see the rest of the Inspector.");

                return;
            }

            DrawFiltersArea();

            bool currentGroupedDialoguesFilter = groupedDialogues.boolValue;
            bool currentStartingDialoguesOnlyFilter = startingDialoguesOnly.boolValue;
            
            List<string> dialogueNames;

            string dialogueFolderPath = $"Assets/DialogueSystem/Dialogues/{currentDialogueContainer.FileName}";

            string dialogueInfoMessage;

            if (currentGroupedDialoguesFilter)
            {
                List<string> dialogueGroupNames = currentDialogueContainer.GetDialogueGroupNames();

                if (dialogueGroupNames.Count == 0)
                {
                    StopDrawing("There are no Dialogue Groups in this Dialogue Container.");

                    return;
                }

                DrawDialogueGroupArea(currentDialogueContainer, dialogueGroupNames);

                DSDialogueGroupSO dialogueGroupSO = (DSDialogueGroupSO) dialogueGroup.objectReferenceValue;

                dialogueNames = currentDialogueContainer.GetGroupedDialogueNames(dialogueGroupSO, currentStartingDialoguesOnlyFilter);

                dialogueFolderPath += $"/Groups/{dialogueGroupSO.GroupName}/Dialogues";

                dialogueInfoMessage = "There are no" + (currentStartingDialoguesOnlyFilter ? " Starting" : "") + " Dialogues in this Dialogue Group.";
            }
            else
            {
                dialogueNames = currentDialogueContainer.GetUngroupedDialogueNames(currentStartingDialoguesOnlyFilter);

                dialogueFolderPath += "/Global/Dialogues";

                dialogueInfoMessage = "There are no" + (currentStartingDialoguesOnlyFilter ? " Starting" : "") + " Ungrouped Dialogues in this Dialogue Container.";
            }

            if (dialogueNames.Count == 0)
            {
                StopDrawing(dialogueInfoMessage);

                return;
            }

            DrawDialogueArea(dialogueNames, dialogueFolderPath);

            DrawDisplayTextArea();

            DrawBehaviourArea();

            DrawDefaultAnswersArea();

            DrawSTTArea();

            DrawUnityEvent();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawDialogueContainerArea()
        {
            DSInspectorUtility.DrawHeader("Dialogue Container");

            dialogueContainer.DrawPropertyField();

            DSInspectorUtility.DrawSpace();
        }

        private void DrawFiltersArea()
        {
            DSInspectorUtility.DrawHeader("Filters");

            groupedDialogues.DrawPropertyField();
            startingDialoguesOnly.DrawPropertyField();

            DSInspectorUtility.DrawSpace();
        }

        private void DrawDialogueGroupArea(DSDialogueContainerSO dialogueContainer, List<string> dialogueGroupNames)
        {
            DSInspectorUtility.DrawHeader("Dialogue Group");

            int oldSelectedDialogueGroupIndex = selectedDialogueGroupIndex.intValue;

            DSDialogueGroupSO oldDialogueGroup = (DSDialogueGroupSO) dialogueGroup.objectReferenceValue;

            bool isOldDialogueGroupNull = oldDialogueGroup == null;

            string oldDialogueGroupName = isOldDialogueGroupNull ? "" : oldDialogueGroup.GroupName;

            UpdateIndexOnNamesListUpdate(dialogueGroupNames, selectedDialogueGroupIndex, oldSelectedDialogueGroupIndex, oldDialogueGroupName, isOldDialogueGroupNull);

            selectedDialogueGroupIndex.intValue = DSInspectorUtility.DrawPopup("Dialogue Group", selectedDialogueGroupIndex, dialogueGroupNames.ToArray());

            string selectedDialogueGroupName = dialogueGroupNames[selectedDialogueGroupIndex.intValue];

            DSDialogueGroupSO selectedDialogueGroup = DSIOUtility.LoadAsset<DSDialogueGroupSO>($"Assets/DialogueSystem/Dialogues/{dialogueContainer.FileName}/Groups/{selectedDialogueGroupName}", selectedDialogueGroupName);

            dialogueGroup.objectReferenceValue = selectedDialogueGroup;

            DSInspectorUtility.DrawDisabledFields(() => dialogueGroup.DrawPropertyField());

            DSInspectorUtility.DrawSpace();
        }

        private void DrawDialogueArea(List<string> dialogueNames, string dialogueFolderPath)
        {
            DSInspectorUtility.DrawHeader("Dialogue");

            int oldSelectedDialogueIndex = selectedDialogueIndex.intValue;

            DSDialogueSO oldDialogue = (DSDialogueSO) dialogue.objectReferenceValue;

            bool isOldDialogueNull = oldDialogue == null;

            string oldDialogueName = isOldDialogueNull ? "" : oldDialogue.DialogueName;

            UpdateIndexOnNamesListUpdate(dialogueNames, selectedDialogueIndex, oldSelectedDialogueIndex, oldDialogueName, isOldDialogueNull);

            selectedDialogueIndex.intValue = DSInspectorUtility.DrawPopup("Dialogue", selectedDialogueIndex, dialogueNames.ToArray());

            string selectedDialogueName = dialogueNames[selectedDialogueIndex.intValue];

            DSDialogueSO selectedDialogue = DSIOUtility.LoadAsset<DSDialogueSO>(dialogueFolderPath, selectedDialogueName);

            dialogue.objectReferenceValue = selectedDialogue;

            DSInspectorUtility.DrawDisabledFields(() => dialogue.DrawPropertyField());
        }

        private void DrawDisplayTextArea()
        {
            DSInspectorUtility.DrawHeader("Display Text");
            dialogueDisplayText.DrawPropertyField();
            choiceDisplayTexts.DrawPropertyField();
        }

        private void DrawBehaviourArea()
        {
            DSInspectorUtility.DrawHeader("Behaviour");
            dialogueInteractionType.DrawPropertyField();
            autoContinueSingleChoice.DrawPropertyField();
            secondsToAutoContinue.DrawPropertyField();
        }

        private void DrawDefaultAnswersArea()
        {
            DSInspectorUtility.DrawHeader("Default Answers");
            didNotHearClip.DrawPropertyField();
            didNotUnderstandClip.DrawPropertyField();
        }

        private void DrawSTTArea()
        {
            DSInspectorUtility.DrawHeader("TTS");
            sttMicController.DrawPropertyField();
        }

        private void DrawUnityEvent()
        {
            DSInspectorUtility.DrawHeader("Event");
            OnDialogueFinishedEvent.DrawPropertyField();
        }

        private void StopDrawing(string reason, MessageType messageType = MessageType.Info)
        {
            DSInspectorUtility.DrawHelpBox(reason, messageType);

            DSInspectorUtility.DrawSpace();

            DSInspectorUtility.DrawHelpBox("You need to select a Dialogue for this component to work properly at Runtime!", MessageType.Warning);

            serializedObject.ApplyModifiedProperties();
        }

        private void UpdateIndexOnNamesListUpdate(List<string> optionNames, SerializedProperty indexProperty, int oldSelectedPropertyIndex, string oldPropertyName, bool isOldPropertyNull)
        {
            if (isOldPropertyNull)
            {
                indexProperty.intValue = 0;

                return;
            }

            bool oldIndexIsOutOfBoundsOfNamesListCount = oldSelectedPropertyIndex > optionNames.Count - 1;
            bool oldNameIsDifferentThanSelectedName = oldIndexIsOutOfBoundsOfNamesListCount || oldPropertyName != optionNames[oldSelectedPropertyIndex];

            if (oldNameIsDifferentThanSelectedName)
            {
                if (optionNames.Contains(oldPropertyName))
                {
                    indexProperty.intValue = optionNames.IndexOf(oldPropertyName);

                    return;
                }

                indexProperty.intValue = 0;
            }
        }
    }
}