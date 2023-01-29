using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Shockwave : MonoBehaviour
{

    public float RADIUS = 4.0f;
    public float DURATION = 0.3f;
    public float OPACITY = 0.5f;

    private SpriteRenderer spriteRenderer;
    private float usedRadius;
    private float usedDuration;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        transform.localScale = Vector3.zero;
        usedRadius = RADIUS;
        usedDuration = DURATION;
    }

    public void SetMagnitude(float magnitude)
    {
        usedRadius = RADIUS * magnitude;
        usedDuration = DURATION * magnitude;
    }

    public void SetColor(Color color)
    {
        spriteRenderer.color = color;
    }

    void Update()
    {
        Vector3 expansion = usedRadius / usedDuration * Vector3.one * Time.deltaTime;
        transform.localScale += expansion;
        if (transform.localScale.magnitude > usedRadius)
        {
            Destroy(gameObject);
        }
        Color curColor = spriteRenderer.color;
        curColor.a = OPACITY * (1.0f - Mathf.Sqrt(transform.localScale.magnitude / usedRadius));
        spriteRenderer.color = curColor;
    }
}