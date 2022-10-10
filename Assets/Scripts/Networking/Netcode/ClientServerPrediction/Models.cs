using System.Collections.Generic;
using UnityEngine;

namespace ClientServerPrediction
{
    public class Inputs
    {
        public Vector2 movement = Vector2.zero;
    }

    public class State
    {
        public Vector2 position = Vector2.zero;
        public Vector2 velocity = Vector2.zero;
        public float rotation = 0f;
        public float angularVelocity = 0f;
    }

    public class InputMessage
    {
        public uint netId;
        public uint startTick;
        public List<Inputs> inputs;
    }

    public class StateContext
    {
        public uint netId;

        // Client tick n associated with input n
        public uint lastProcessedClientTick;
        // Server tick m associated with input m
        public uint lastProcessedServerTick;


        // This is the state of client state n + 1
        public State state;
    }
    public class StateMessage
    {
        // This is the server tick m + 1
        public uint serverTick;
        public List<StateContext> stateContexts;

        public Dictionary<uint, StateContext> GetMap()
        {
            Dictionary<uint, StateContext> map = new Dictionary<uint, StateContext>();
            foreach(StateContext stateContext in stateContexts)
            {
                map.Add(stateContext.netId, stateContext);
            }

            return map;
        }
    }

    public class RunContext
    {

    }
}
