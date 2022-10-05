using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMessage<S, I>
{
    public uint lastProcessedClientTick;
    public uint lastProcessedServerTick;
    public uint serverTick;
    public S state;

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

    public StateMessage(InputPacket<I> inputPacket, uint serverTick, S state)
    {
        lastProcessedClientTick = inputPacket.clientTick;
        lastProcessedServerTick = inputPacket.serverTick;
        this.serverTick = serverTick;
        this.state = state;
    }
}
