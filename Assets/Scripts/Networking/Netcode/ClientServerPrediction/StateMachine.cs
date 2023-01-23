using System.Collections.Generic;
using UnityEngine;

namespace ClientServerPrediction
{
    public static class StateMachine
    {
        public static void Run(in Dictionary<uint, Inputs> inputMap,
                               ref Dictionary<uint, IInputful> inputfulMap,
                               ref Dictionary<uint, IStateful> statefulMap,
                               IRunnable runnable,
                               RunContext runContext)
        {
            foreach(uint netId in inputfulMap.Keys)
            {
                if(inputMap.ContainsKey(netId))
                {
                    inputfulMap[netId].ApplyInput(inputMap[netId]);
                }
            }

            foreach(uint netId in statefulMap.Keys)
            {
                if(!inputfulMap.ContainsKey(netId))
                {
                    statefulMap[netId].PredictState(statefulMap[netId].GetState());
                }
            }

            runnable.Run(runContext);
        }

        public static void SetState(ref Dictionary<uint, IStateful> statefulMap, in Dictionary<uint, State> stateMap, in Dictionary<uint, Vector2> overridePositions = null)
        {
            foreach (uint netId in statefulMap.Keys)
            {
                if (stateMap.ContainsKey(netId))
                {
                    Vector2 position;
                    // TODO: Create a ball spawn position
                    // TODO: Override positions doesn't work for shit
                    if (overridePositions != null && overridePositions.ContainsKey(netId))
                    {
                        position = overridePositions[netId];
                    } else
                    {
                        position = stateMap[netId].position;
                    }
                    // TODO: Fix - use the state passed
                    State state = new State { position = position, playerState = new PlayerState() };
                    statefulMap[netId].SetState(state);
                }
            }
        }
    }
}
