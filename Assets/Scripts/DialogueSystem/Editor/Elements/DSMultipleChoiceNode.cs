using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DS.Elements
{
    using Enumerations;
    using Utilities;
    using Windows;

    public class DSMultipleChoiceNode : DSNode
    {
        public override void Initialize(DSGraphView dsGraphView, Vector2 position)
        {
            base.Initialize(dsGraphView, position);

            DialogueType = DSDialogueType.MultipleChoice;

            Choices.Add("New Choice");
        }

        public override void Draw()
        {
            base.Draw();

            // Main Container
            Button addChoiceButton = DSElementUtility.CreateButton("Add Choice", () =>
            {
                Port choicePort = CreateChoicePort("New Choice");

                Choices.Add("New Choice");

                outputContainer.Add(choicePort);
            });

            addChoiceButton.AddToClassList("ds-node__button");

            mainContainer.Insert(1, addChoiceButton);

            // Output Container
            foreach (string choice in Choices)
            {
                Port choicePort = CreateChoicePort(choice);
                outputContainer.Add(choicePort);
            }

            RefreshExpandedState();
        }

        #region Element Creation
        private Port CreateChoicePort(string choice)
        {
            Port choicePort = this.CreatePort();

            choicePort.name = "";

            Button deleteChoiceButton = DSElementUtility.CreateButton("X");
            deleteChoiceButton.AddToClassList("ds-node__button");

            TextField choiceTextField = DSElementUtility.CreateTextField(choice);

            choiceTextField.AddClasses(
                "ds-node__text-field",
                "ds-node__choice-text-field",
                "ds-node__textfield__hidden"
            );

            choicePort.Add(choiceTextField);
            choicePort.Add(deleteChoiceButton);
            return choicePort;
        }
        #endregion
    }
}