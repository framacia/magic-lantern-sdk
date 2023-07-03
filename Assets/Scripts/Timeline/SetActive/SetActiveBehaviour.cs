using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Playables;

public class SetActiveBehaviour : PlayableBehaviour
{
    public bool setActive = true;

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (Application.isPlaying)
        {
            GameObject go = playerData as GameObject;

            go.SetActive(setActive);
        }
    }
}
