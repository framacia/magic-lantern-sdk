using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Playables;

public class ChangeTextBehaviour : PlayableBehaviour
{
    public string textToChangeTo = null;
    public bool setEmptyAtTheEnd = true;

    string originalText;

    TextMeshProUGUI tMPro;

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        tMPro = playerData as TextMeshProUGUI;

        if (string.IsNullOrEmpty(originalText))
        {
            originalText = tMPro.text;
        }

        if (tMPro != null)
        {
            tMPro.text = textToChangeTo;
        }
    }

    //TODO Could not make this method work cause no reference to binding yet, that is done in ProcessFrame
    //public override void OnBehaviourPlay(Playable playable, FrameData info)
    //{


    //    if(tMPro != null)
    //    {
    //        tMPro.text = textToChangeTo;
    //    }

    //    // Execute your starting logic here, calling into a singleton for example
    //    Debug.Log("Clip started!");

    //}

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        // Only execute in Play mode
        if (Application.isPlaying)
        {
            var duration = playable.GetDuration();
            var time = playable.GetTime();
            var count = time + info.deltaTime;

            if ((info.effectivePlayState == PlayState.Paused && count > duration) || Mathf.Approximately((float)time, (float)duration))
            {
                // Execute your finishing logic here:
                if (setEmptyAtTheEnd)
                {
                    tMPro.text = "";
                }
            }
            return;
        }
    }
}
