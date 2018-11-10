using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boost : Ability {

    protected override void DerivedUpdate() { }

    public float speedIncrease;

    protected override void Function()
    {
        player.speed += speedIncrease;
    }
}
