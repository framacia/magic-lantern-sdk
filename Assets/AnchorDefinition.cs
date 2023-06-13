using UnityEngine;

public class AnchorDefinition : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    void Update()
    {
        if (_dirty)
        {

            _dirty = false;
        }
    }

    [SerializeField, SerializeProperty("filterProximity")]
    private float _filterProximity;

    private bool _dirty = true;

    public string anchorId;
    public float volumeSize;

    public float filterProximity
    {
        get
        {
            return _filterProximity;
        }
        set
        {
            Debug.Log("Set filterProximity");
            _filterProximity = value;
            BoundaryCylinder.transform.localScale = new Vector3(
                _filterProximity,
                BoundaryCylinder.transform.localScale.y,
                _filterProximity
            );
            _dirty = true;
        }
    }

    public GameObject BoundaryCylinder;
}