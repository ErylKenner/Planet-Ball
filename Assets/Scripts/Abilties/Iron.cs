using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Iron : Ability
{

    public float Duration = 2f;
    public float IncreasedMass = 10f;
    //public Color IronColor;
    public Sprite IronSprite;
    public Color IronColor;

    private Sprite originalSprite;
    private Color originalColor;

    private float orignalMass;

    protected override void DerivedUpdate() { }

    protected override void Function()
    {
        StartCoroutine(SetIron());
    }

    IEnumerator SetIron()
    {
        originalSprite = player.GetComponent<SpriteRenderer>().sprite;
        originalColor = player.GetComponent<SpriteRenderer>().color;
        player.GetComponent<SpriteRenderer>().sprite = IronSprite;
        player.GetComponent<SpriteRenderer>().color = IronColor;
        orignalMass = player.Body.mass;
        player.Body.mass = IncreasedMass;
        yield return new WaitForSeconds(Duration);
        player.Body.mass = orignalMass;
        player.GetComponent<SpriteRenderer>().sprite = originalSprite;
        player.GetComponent<SpriteRenderer>().color = originalColor;
    }
}
