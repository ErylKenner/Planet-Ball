using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Shadow : MonoBehaviour {

    public Vector2 offset = new Vector2(-50, -50);
    public Color shadowColor;
    public Material shadowMaterial;

    SpriteRenderer sprite;
    SpriteRenderer shadow;

    private void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        GameObject obj = new GameObject();
        shadow = obj.AddComponent<SpriteRenderer>();
        shadow.transform.parent = sprite.transform;
        shadow.transform.localScale = Vector3.one;
        shadow.name = "Shadow";
        shadow.transform.localRotation = Quaternion.identity;
        shadow.sprite = sprite.sprite;
        shadow.material = shadowMaterial;
        shadow.color = shadowColor;
        shadow.sortingLayerName = "Shadow";
    }

    private void LateUpdate()
    {
        shadow.transform.position = sprite.transform.position + (Vector3)offset;
    }
}
