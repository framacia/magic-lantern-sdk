using System.Collections;
using UnityEngine;

public class UVScroller : MonoBehaviour
{
    [SerializeField] Vector2 uvSpeed = new Vector2(0.0f, 0.0f);
    private Renderer renderer;

    private void Start()
    {
        renderer = GetComponent<Renderer>();
    }

    private void Update()
    {
        // Calculate new UV offset based on time and speed
        Vector2 offset = uvSpeed * Time.time;

        // Set the offset to the material
        renderer.material.SetTextureOffset("_BaseMap", offset);
    }
}
