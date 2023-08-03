using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TransformLerpClip : PlayableAsset, IPropertyPreview
{
    public Vector3 targetPositionOffset;
    public Vector3 targetRotationOffset;
    public Vector3 targetScale = Vector3.one;

    public AnimationCurve animCurve = AnimationCurve.Linear(0, 0, 1, 1);

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<TransformLerpBehaviour>.Create(graph);

        var behaviour = playable.GetBehaviour();
        behaviour.targetPositionOffset = targetPositionOffset;
        behaviour.targetRotationOffset = targetRotationOffset;
        behaviour.targetScaleAbsolute = targetScale;

        behaviour.animCurve = animCurve;

        return playable;
    }

    public void GatherProperties(PlayableDirector director, IPropertyCollector driver)
    {
#if UNITY_EDITOR
        const string kLocalPosition = "m_LocalPosition";

        driver.AddFromName<Transform>(kLocalPosition + ".x");
        driver.AddFromName<Transform>(kLocalPosition + ".y");
        driver.AddFromName<Transform>(kLocalPosition + ".z");
#endif
    }
}
