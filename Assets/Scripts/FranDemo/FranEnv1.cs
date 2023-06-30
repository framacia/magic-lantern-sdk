using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FranEnv1 : Env
{
    [SerializeField]
    private AudioClip[] positiveFeedback;
    [SerializeField]
    private Sprite[] images;
    [SerializeField]
    private SpriteRenderer sr;
    private Material mat;

    public CameraPointedObject cube;

    protected override void Start()
    {
        //mat = sr.material;
        //StartCoroutine(Sequence0());
        cube.iTimer.OnFinishInteraction += OnCubeInteracted;
    }

    protected override void Update()
    {
        //Does nothing atm but must be implemented
    }

    private void OnCubeInteracted()
    {
        Destroy(cube.gameObject);
    }
}
