using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;

public class SetActiveClip : PlayableAsset
{
    public bool setActive;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<SetActiveBehaviour>.Create(graph);

        var behaviour = playable.GetBehaviour();
        behaviour.setActive = setActive;

        return playable;
    }
}
