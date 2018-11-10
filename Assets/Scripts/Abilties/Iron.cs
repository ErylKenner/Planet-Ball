using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Iron : Ability {

    public float Duration = 1f;
    public float IncreasedMass = 10f;

    private float orignalMass;

    protected override void DerivedUpdate() {}

    protected override void Function()
    {
        StartCoroutine(SetIron());
    }

    IEnumerator SetIron()
    {
        orignalMass = player.Body.mass;
        player.Body.mass = IncreasedMass;
        yield return new WaitForSeconds(Duration);
        player.Body.mass = orignalMass;
    }
}
