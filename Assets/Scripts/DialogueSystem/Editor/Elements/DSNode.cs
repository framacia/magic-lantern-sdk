using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DS.Elements
{
    using Enumerations;
    using Utilities;

    /// <summary>
    /// Represents a node in a dialogue system.
    /// </summary>
    public class DSNode : Node
    {
        /// <summary>
        /// The name of the dialogue.
        /// </summary>
        public string DialogueName { get; set; }

        /// <summary>
        /// The list of choices for the dialogue.
        /// </summary>
        public List<string> Choices { get; set; }

        /// <summary>
        /// The text of the dialogue.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// The type of the dialogue.
        /// </summary>
        public DSDialogueType DialogueType { get; set; }

        /// <summary>
        /// Initializes the DSNode with the specified position.
        /// </summary>
        /// <param name="position">The position of the DSNode.</param>
        public virtual void Initialize(Vector2 position)
        {
            DialogueName = "DialogueName";
            Choices = new List<string>();
            Text = "Dialogue text.";

            SetPosition(new Rect(position, Vector2.zero));

            mainContainer.AddToClassList("ds-node__main-container");
            extensionContainer.AddToClassList("ds-node__extension-container");
        }

        /// <summary>
        /// Draws the DSNode.
        /// </summary>
        public virtual void Draw()
        {
            // Title Container
            TextField dialogueNameTextField = DSElementUtility.CreateTextField(DialogueName);

            dialogueNameTextField.AddClasses(
                "ds-node__text-field",
                "ds-node__filename-text-field",
                "ds-node__textfield__hidden"
                );

            titleContainer.Insert(0, dialogueNameTextField);

            // Input Container
            Port inputPort = this.CreatePort("Dialogue Connection", direction: Direction.Input, capacity: Port.Capacity.Multi);
            inputPort.portName = "Dialogue Connection";
            inputContainer.Add(inputPort);

            // Extensions Container
            VisualElement customDataContainer = new VisualElement();
            customDataContainer.AddToClassList("ds-node__custom-data-container");

            Foldout textFoldout = DSElementUtility.CreateFoldout("Dialogue Text");

            TextField textTextField = DSElementUtility.CreateTextArea(Text);
            textTextField.AddClasses(
                "ds-node__text-field",
                "ds-node__quote-text-field"
            );

            textFoldout.Add(textTextField);
            customDataContainer.Add(textFoldout);
            extensionContainer.Add(customDataContainer);
        }
    }
}