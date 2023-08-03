using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TransformLerpBehaviour : PlayableBehaviour
{
    public Vector3 targetPositionOffset;
    public Vector3 targetRotationOffset;
    public Vector3 targetScaleAbsolute;
    public AnimationCurve animCurve;

    private Transform targetTransform;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Vector3 initialScale = Vector3.one;

    private bool firstFrameHappened;
    private bool isCurrentClip;


    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        // Get the target transform from the playerData object (assuming it's a GameObject with a Transform component).
        targetTransform = playerData as Transform;

        if (targetTransform == null)
            return;

        if (!firstFrameHappened)
        {
            initialPosition = targetTransform.localPosition;
            initialRotation = targetTransform.rotation;
            initialScale = targetTransform.localScale;

            //If no animation curve specified, default to linear
            if (animCurve == null)
            {
                animCurve = new AnimationCurve();
                animCurve = AnimationCurve.Linear(0, 0, 1, 1);
            }

            firstFrameHappened = true;
        }

        // Calculate the progress based on the current time of the TransformLerpClip.
        float clipTime = (float)(playable.GetTime() / playable.GetDuration());

        // Lerp position if enabled.
        if (targetPositionOffset != Vector3.zero)
        {
            Vector3 lerpedPosition = Vector3.Lerp(initialPosition, initialPosition + targetPositionOffset, animCurve.Evaluate(clipTime));
            targetTransform.localPosition = lerpedPosition;
        }

        // Lerp rotation if enabled.
        if (targetRotationOffset != Vector3.zero)
        {
            Quaternion lerpedRotation = Quaternion.Slerp(initialRotation, initialRotation * Quaternion.Euler(targetRotationOffset), animCurve.Evaluate(clipTime));
            targetTransform.rotation = lerpedRotation;
        }

        // Lerp scale if enabled.
        if (targetScaleAbsolute != initialScale)
        {
            Vector3 lerpedScale = Vector3.Lerp(initialScale, targetScaleAbsolute, clipTime);
            targetTransform.localScale = lerpedScale;
        }
    }


    //Trying to set initial position when scrolling through timeline - not necessary
    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        //Access the current time of the PlayableDirector.
        double currentDirectorTime = playable.GetGraph().GetRootPlayable(0).GetTime();
        double clipStartTime = playable.GetTime();
        double clipEndTime = clipStartTime + playable.GetDuration();


        //if (currentDirectorTime < clipStartTime)
        //    targetTransform.localPosition = initialPosition;
        //else if (currentDirectorTime > clipEndTime)
        //    targetTransform.localPosition = initialPosition + targetPositionOffset;

        isCurrentClip = false;

        //var timeline = playableDirector.playableAsset as TimelineAsset;
        //foreach (var track in timeline.GetOutputTracks())
        //{
        //    foreach (var clip in track.GetClips())
        //        Debug.Log(clip.start);

        //}
    }

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        isCurrentClip = true;
    }

}
