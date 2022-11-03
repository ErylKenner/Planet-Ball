using System.Collections.Generic;
using UnityEngine;

namespace ClientServerPrediction
{
    public class Inputs
    {
        public Vector2 movement = Vector2.zero;
        public bool AttachTether = false;
        public bool WindTether = false;
        public bool UnwindTether = false;
        public bool SpeedBoost = false;
        public bool Kick = false;
    }

    [System.Serializable]
    public class PlayerState
    {
        public bool InputIsTethered = false;
        public bool InputIsWindTether = false;
        public bool InputIsUnwindTether = false;
        public bool InputIsSpeedBoost = false;
        public bool InputIsKick = false;
        public float OrbitRadius = 0;
        public Vector2 CenterPoint = Vector2.zero;
        public float Speed = 12.0f;
        public float CurSpeedBoostCooldown = 0f;
        public float CurKickCooldown = 0f;
        public float CurGas = 0f;
        public bool IsSpeedBoost = false;
        public bool IsKick = false;
    }

    public class State
    {
        public Vector2 position = Vector2.zero;
        public Vector2 velocity = Vector2.zero;
        public float rotation = 0f;
        public float angularVelocity = 0f;
        public PlayerState playerState = null;
    }

    public class StateError
    {
        public float positionDiff;
    }

    public class InputContext
    {
        public uint netId;
        public List<Inputs> inputs = new List<Inputs>();
    }

    public class InputMessage
    {
        public List<InputContext> inputContexts = new List<InputContext>();
        public uint startTick;

        public Dictionary<uint, InputContext> GetMap()
        {
            Dictionary<uint, InputContext> map = new Dictionary<uint, InputContext>();
            foreach (InputContext inputContext in inputContexts)
            {
                map.Add(inputContext.netId, inputContext);
            }

            return map;
        }
    }

    public class TickSync
    {
        // Client tick n associated with input n
        public uint lastProcessedClientTick = 0;
        // Server tick m associated with input m
        public uint lastProcessedServerTick = 0;

        public int ClientSyncOffset()
        {
            return (int)lastProcessedClientTick - (int)lastProcessedServerTick;
        }
    }

    public class StateContext
    {
        public uint netId;

        public TickSync tickSync = null;

        // This is the state of client state n + 1
        public State state;
    }
    public class StateMessage
    {
        // This is the server tick m + 1
        public uint serverTick;
        public List<StateContext> stateContexts = new List<StateContext>();
        public bool frozen = false;

        public Dictionary<uint, StateContext> GetMap()
        {
            Dictionary<uint, StateContext> map = new Dictionary<uint, StateContext>();
            foreach(StateContext stateContext in stateContexts)
            {
                map.Add(stateContext.netId, stateContext);
            }

            return map;
        }

        public uint MessageClientTick(uint netId)
        {
            return (uint)((int)serverTick + GetMap()[netId].tickSync?.ClientSyncOffset());
        }
    }

    public class RunContext
    {
        public float dt = 0;
    }
}
