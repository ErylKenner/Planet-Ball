using NetcodeData;
public static class NetcodeSystem
{
    public static void PrePhysicsStep(NetcodePlayer player, Inputs inputs)
    {
        player.GetComponent<PlayerPlanetController>().Move(inputs.movement);
    }
}