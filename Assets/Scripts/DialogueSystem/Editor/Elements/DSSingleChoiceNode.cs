using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace DS.Elements
{
    using Enumerations;
    using Utilities;
    using Windows;

    public class DSSingleChoiceNode : DSNode
    {
        public override void Initialize(DSGraphView dsGraphView, Vector2 position)
        {
            base.Initialize(dsGraphView, position);

            DialogueType = DSDialogueType.SingleChoice;

            Choices.Add("Next Dialogue");
        }

        public override void Draw()
        {
            base.Draw();

            // Output Container
            foreach (string choice in Choices)
            {
                Port choicePort = this.CreatePort(choice);

                choicePort.name = choice;
                outputContainer.Add(choicePort);
            }

            RefreshExpandedState();
        }
    }
}
