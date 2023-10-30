using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DS.Elements
{
    using Enumerations;

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
        }

        /// <summary>
        /// Draws the DSNode.
        /// </summary>
        public virtual void Draw()
        {
            // Title Container
            TextField dialogueNameTextField = new TextField()
            {
                value = DialogueName
            };
            titleContainer.Insert(0, dialogueNameTextField);

            // Input Container
            Port inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
            inputPort.portName = "Dialogue Connection";
            inputContainer.Add(inputPort);

            // Extensions Container
            VisualElement customDataContainer = new VisualElement();
            Foldout textFoldout = new Foldout()
            {
                text = "Dialogue Text"
            };
            TextField textTextField = new TextField()
            {
                value = Text
            };
            textFoldout.Add(textTextField);
            customDataContainer.Add(textFoldout);
            extensionContainer.Add(customDataContainer);
        }
    }
}