using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Shockwave : MonoBehaviour
{
    public float ExpansionSpeed = 30.0f;
    public float MaxRadius = 40.0f;
    public float Opacity = 0.9f;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        transform.localScale = Vector3.zero;
    }

    void Update()
    {
        Vector3 expansion = Vector3.one * ExpansionSpeed * Time.deltaTime;
        transform.localScale += expansion;
        if (transform.localScale.magnitude > MaxRadius)
        {
            Destroy(gameObject);
        }
        Color curColor = spriteRenderer.color;
        curColor.a = Opacity * (1.0f - Mathf.Sqrt(transform.localScale.magnitude / MaxRadius));
        spriteRenderer.color = curColor;
    }
}
