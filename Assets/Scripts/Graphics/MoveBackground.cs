using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class MoveBackground : MonoBehaviour
{
    public float SPEED = 0.02f;
    public float MAX_OFFSET_HORIZONTAL = 0.007f;
    public float MAX_OFFSET_VERTICAL = 0.08f;


    private MeshRenderer meshRenderer;
    private float x_coord;
    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        x_coord = 100;
    }

    void Update()
    {
        x_coord += Time.deltaTime;
        float theta = Mathf.PerlinNoise(x_coord, x_coord + 9999f) * Mathf.PI * 2;
        float r = Mathf.PerlinNoise(x_coord + 351511f, x_coord + 235262f);
        Vector2 dir = new Vector2(r * Mathf.Cos(theta), r * Mathf.Sin(theta)) * SPEED * Time.deltaTime;
        Vector2 currentOffset = meshRenderer.material.mainTextureOffset;
        if (currentOffset.y >= MAX_OFFSET_HORIZONTAL)
        {
            dir.y = -Mathf.Abs(dir.y);
        }
        if (currentOffset.y <= -MAX_OFFSET_HORIZONTAL)
        {
            dir.y = Mathf.Abs(dir.y);
        }
        if (currentOffset.x >= MAX_OFFSET_VERTICAL)
        {
            dir.x = -Mathf.Abs(dir.x);
        }
        if (currentOffset.x <= -MAX_OFFSET_VERTICAL)
        {
            dir.x = Mathf.Abs(dir.x);
        }

        Vector2 newOffset = currentOffset + dir;
        meshRenderer.material.mainTextureOffset = newOffset;
    }
}
