using System.Collections;
using UnityEngine;

public class Iron : Ability
{
    public float Duration = 1f;
    public float IncreasedMass = 15f;
    public Sprite IronSprite;
    public Color IronColor = new Color32(167, 167, 167, 255);

    protected override void DerivedUpdate() { }

    protected override void UseAbility(Player player)
    {
        StartCoroutine(SetIron(player));
    }

    protected override void DerivedStart()
    {
        /*spriteRenderer = player.GetComponent<SpriteRenderer>();
        orignalMass = player.Body.mass;
        originalSprite = spriteRenderer.sprite;
        originalColor = spriteRenderer.color;
        */
    }

    IEnumerator SetIron(Player player)
    {
        SpriteRenderer spriteRenderer = player.GetComponent<SpriteRenderer>();
        // potential issue if player somehow triggers during Iron
        float orignalMass = player.Body.mass;
        Sprite originalSprite = spriteRenderer.sprite;
        Color originalColor = spriteRenderer.color;

        spriteRenderer.sprite = IronSprite;
        spriteRenderer.color = IronColor;
        player.Body.mass = IncreasedMass;
        yield return new WaitForSeconds(Duration);
        player.Body.mass = orignalMass;
        spriteRenderer.sprite = originalSprite;
        spriteRenderer.color = originalColor;
    }
}
