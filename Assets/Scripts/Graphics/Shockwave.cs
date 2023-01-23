using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Shockwave : MonoBehaviour
{
    public float EXPANSION_SPEED = 30.0f;
    public float MAX_RADIUS = 40.0f;
    public float OPACITY = 0.9f;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        transform.localScale = Vector3.zero;
    }

    void Update()
    {
        Vector3 expansion = Vector3.one * EXPANSION_SPEED * Time.deltaTime;
        transform.localScale += expansion;
        if (transform.localScale.magnitude > MAX_RADIUS)
        {
            Destroy(gameObject);
        }
        Color curColor = spriteRenderer.color;
        curColor.a = OPACITY * (1.0f - Mathf.Sqrt(transform.localScale.magnitude / MAX_RADIUS));
        spriteRenderer.color = curColor;
    }
}