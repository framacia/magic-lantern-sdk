using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DS.Elements
{
    using Enumerations;
    using UnityEditor.Experimental.GraphView;

    public class DSSingleChoiceNode : DSNode
    {
        public override void Initialize(Vector2 position)
        {
            base.Initialize(position);

            DialogueType = DSDialogueType.SingleChoice;

            Choices.Add("Next Dialogue");
        }

        public override void Draw()
        {
            base.Draw();

            // Output Container
            foreach (string choice in Choices)
            {
                Port choicePort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));

                choicePort.name = choice;
                outputContainer.Add(choicePort);
            }

            RefreshExpandedState();
        }
    }
}
