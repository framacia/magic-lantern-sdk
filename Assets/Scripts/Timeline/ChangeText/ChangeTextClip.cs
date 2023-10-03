using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;

public class ChangeTextClip : PlayableAsset
{
    public string textToChangeTo;
    public bool setEmptyAtTheEnd = true;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<ChangeTextBehaviour>.Create(graph);

        var behaviour = playable.GetBehaviour();
        behaviour.textToChangeTo = textToChangeTo;
        behaviour.setEmptyAtTheEnd = setEmptyAtTheEnd;

        return playable;
    }
}
