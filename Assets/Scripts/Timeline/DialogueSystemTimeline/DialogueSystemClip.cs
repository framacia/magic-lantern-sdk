using DS.ScriptableObjects;
using UnityEngine;
using UnityEngine.Playables;

public class DialogueSystemClip : PlayableAsset
{
    public bool changeDialogue = false;
    public DSDialogueContainerSO newDialogueContainerSO;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<DialogueSystemBehaviour>.Create(graph);

        var behaviour = playable.GetBehaviour();
        behaviour.changeDialogue = changeDialogue;
        behaviour.newDialogueContainerSO = newDialogueContainerSO;

        return playable;
    }
}
