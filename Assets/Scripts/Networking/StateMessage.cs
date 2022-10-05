public struct StateMessage
{
    public uint lastProcessedClientTick;
    public uint lastProcessedServerTick;
    public uint serverTick;
    public NetcodeManager.State state;

    public uint missingInputs
    {
        get
        {
            return serverTick - lastProcessedServerTick;
        }
    }

    public uint clientTick
    {
        get
        {
            return lastProcessedClientTick + missingInputs;
        }
    }
}
