using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boost : Ability
{



    public float speedIncrease;

    public float overSpeedDeceleration;

    protected override void DerivedUpdate()
    {
    }

    private void FixedUpdate()
    {
        if (player.speed > player.maxSpeed)
        {
            float newSpeed = player.speed - overSpeedDeceleration * Time.fixedDeltaTime;
            newSpeed = Mathf.Clamp(newSpeed, player.maxSpeed, Mathf.Infinity);
            player.speed = newSpeed;
        }
    }

    protected override void Function()
    {
        player.speed += speedIncrease;
    }
}
