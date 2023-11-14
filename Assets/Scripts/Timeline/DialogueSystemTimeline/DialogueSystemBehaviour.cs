using DS.ScriptableObjects;
using DS;
using UnityEngine;
using UnityEngine.Playables;

public class DialogueSystemBehaviour : PlayableBehaviour
{
    public bool changeDialogue = true;
    public DSDialogueContainerSO newDialogueContainerSO;

    private bool alreadyDone; //TODO May have to change this so it works per-clip

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (Application.isPlaying)
        {
            if (alreadyDone)
                return;

            DSDialogue dsDialogue = playerData as DSDialogue;

            if (newDialogueContainerSO != null)
            {
                //dsDialogue //Still have to decide how to choose starting dialogue from container
                //Maybe we can foreach all the grouped and ungrouped dialogues until we find one that is a starting dialogue
            }

            dsDialogue.RestartDialogue();

            alreadyDone = true;
        }
    }
}
