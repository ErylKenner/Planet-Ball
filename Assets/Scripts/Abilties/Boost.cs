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
        /*
        if (player.Speed > player.MaxSpeed)
        {
            float newSpeed = player.Speed - overSpeedDeceleration * Time.fixedDeltaTime;
            newSpeed = Mathf.Clamp(newSpeed, player.MaxSpeed, Mathf.Infinity);
            player.Speed = newSpeed;
        }
        */
    }

    protected override void Function()
    {
        player.Speed += speedIncrease;
    }
}
