using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace ClientServerPrediction
{
    public class Inputs
    {
        public Vector2 movement = Vector2.zero;
        public bool AttachTether = false;
        public float WindTether = 0f;
        public bool SpeedBoost = false;
        public bool Kick = false;

        public static Inputs FromState(State state)
        {
            if (state.playerState == null)
            {
                return null;
            }

            return new Inputs
            {
                AttachTether = state.playerState.IsTethered,
                WindTether = state.playerState.IsWinding ? 1f : (state.playerState.IsUnwinding ? -1f : 0f),
                SpeedBoost = state.playerState.IsSpeedBoost,
                Kick = state.playerState.IsKick,
            };
        }
    }

    [System.Serializable]
    public class PlayerState
    {
        // Tethering
        public float OrbitRadius = 0f;
        public Vector2 CenterPoint = Vector2.zero;
        public float TetherDisabledDuration = 0f;
        public bool IsTethered = false;
        public bool IsWinding = false;
        public bool IsUnwinding = false;

        // Speed boost
        public float Speed = 0f;
        public float CurGas = 0f;
        public float CurSpeedBoostCooldown = 0f;
        public bool IsSpeedBoost = false;

        // Kick
        public float CurKickCooldown = 0f;
        public bool IsKick = false;

        public Vector2 CurPosition = Vector2.zero;

        public PlayerState() { }
        public PlayerState(PlayerState other)
        {
            OrbitRadius = other.OrbitRadius;
            CenterPoint = other.CenterPoint;
            TetherDisabledDuration = other.TetherDisabledDuration;
            IsTethered = other.IsTethered;
            IsWinding = other.IsWinding;
            IsUnwinding = other.IsUnwinding;

            Speed = other.Speed;
            CurGas = other.CurGas;
            CurSpeedBoostCooldown = other.CurSpeedBoostCooldown;
            IsSpeedBoost = other.IsSpeedBoost;

            CurKickCooldown = other.CurKickCooldown;
            IsKick = other.IsKick;

            CurPosition = other.CurPosition;
        }

        public bool Equals(PlayerState playerState)
        {
            if (playerState is null)
            {
                return false;
            }

            if (Object.ReferenceEquals(this, playerState))
            {
                return true;
            }

            if (this.GetType() != playerState.GetType())
            {
                return false;
            }

            return (
                IsTethered == playerState.IsTethered &&
                IsWinding == playerState.IsWinding &&
                IsUnwinding == playerState.IsUnwinding &&
                IsSpeedBoost == playerState.IsSpeedBoost &&
                IsKick == playerState.IsKick &&
                CurPosition == playerState.CurPosition &&
                CenterPoint == playerState.CenterPoint &&
                Mathf.Approximately(OrbitRadius, playerState.OrbitRadius) &&
                Mathf.Approximately(TetherDisabledDuration, playerState.TetherDisabledDuration) &&
                Mathf.Approximately(Speed, playerState.Speed) &&
                Mathf.Approximately(CurGas, playerState.CurGas) &&
                Mathf.Approximately(CurSpeedBoostCooldown, playerState.CurSpeedBoostCooldown) &&
                Mathf.Approximately(CurKickCooldown, playerState.CurKickCooldown)
            );
        }
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
        public float positionDiff = 0.01f;
        public float allowedRadiusDiff = 0.1f;

        public float snapDistance = 10f;

        public bool NeedsCorrection(State currentState, State desiredState)
        {
            if (desiredState.playerState != null && currentState.playerState != null)
            {

                if (!currentState.playerState.Equals(desiredState.playerState))
                {
                    return true;
                }

                if (currentState.playerState.IsTethered && desiredState.playerState.IsTethered)
                {
                    float radiusDifference = Mathf.Abs(desiredState.playerState.OrbitRadius - currentState.playerState.OrbitRadius);
                    if (radiusDifference > allowedRadiusDiff)
                    {
                        return true;
                    }
                }

            }

            float positionDifference = Vector2.Distance(currentState.position, desiredState.position);
            return positionDifference > positionDiff;
        }
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
            foreach (StateContext stateContext in stateContexts)
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
