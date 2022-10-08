
using System.Collections.Generic;
using UnityEngine;

namespace NetcodeData
{

    public struct ClientState
    {
        public Vector2 position;
        public Vector2 velocity;
        public float rotation;
        public float angularVelocity;
    }
    public struct InputMessage
    {
        public uint start_tick_number;
        public List<Inputs> inputs;
    }

    public struct Inputs
    {
        public Vector2 movement;
    }
    public struct State
    {
        public uint netId;
        public ClientState state;
    }

    public struct GlobalStateMessage
    {
        public StateMessage[] states;
    }

    public struct StateMessage
    {
        public uint lastProcessedClientTick;
        public uint lastProcessedServerTick;
        public uint serverTick;
        public State state;

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

}
