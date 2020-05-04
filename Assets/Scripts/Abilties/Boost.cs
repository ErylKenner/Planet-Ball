using UnityEngine;

public class Boost : Ability
{
    public float BooostSpeed;
    public float BoostDeceleration;

    protected override void DerivedUpdate() { }

    private void FixedUpdate()
    {
        if (player.Speed > player.DefaultSpeed)
        {
            player.Speed -= BoostDeceleration * Time.deltaTime;
        }
    }

    protected override void DerivedStart() { }

    protected override void UseAbility()
    {
        player.Speed = BooostSpeed;
    }
}
