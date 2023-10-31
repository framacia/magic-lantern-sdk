using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace DS.Elements
{
    using Enumerations;
    using UnityEditor.Experimental.GraphView;

    public class DSMultipleChoiceNode : DSNode
    {
        public override void Initialize(Vector2 position)
        {
            base.Initialize(position);

            DialogueType = DSDialogueType.MultipleChoice;

            Choices.Add("New Choice");
        }

        public override void Draw()
        {
            base.Draw();

            // Main Container
            Button addChoiceButton = new Button()
            {
                text = "Add Choice"
            };

            addChoiceButton.AddToClassList("ds-node__button");

            mainContainer.Insert(1, addChoiceButton);

            // Output Container
            foreach (string choice in Choices)
            {
                Port choicePort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));

                choicePort.name = "";

                Button deleteChoiceButton = new Button()
                {
                    text = "X"
                };
                deleteChoiceButton.AddToClassList("ds-node__button");

                TextField choiceTextField = new TextField()
                {
                    value = choice,
                };
                choiceTextField.AddToClassList("ds-node__textfield");
                choiceTextField.AddToClassList("ds-node__choice-textfield");
                choiceTextField.AddToClassList("ds-node__textfield__hiden");

                choicePort.Add(choiceTextField);
                choicePort.Add(deleteChoiceButton);

                outputContainer.Add(choicePort);
            }

            RefreshExpandedState();
        }
    }
}