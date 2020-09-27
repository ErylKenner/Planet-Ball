using UnityEngine;

public class Boost : Ability
{
    public float BooostSpeed = 500f;
    public float BoostDeceleration = 250f;

    protected override void DerivedUpdate() { }

    private void FixedUpdate()
    {
        if (player != null && player.Speed > player.DefaultSpeed)
        {
            player.Speed -= BoostDeceleration * Time.deltaTime;
        }
    }

    protected override void DerivedStart() { }

    protected override void UseAbility(Player player)
    {
        player.Speed = BooostSpeed;
    }
}
