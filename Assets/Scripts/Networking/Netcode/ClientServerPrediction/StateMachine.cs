using System.Collections.Generic;

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

        public static void SetState(ref Dictionary<uint, IStateful> statefulMap, in Dictionary<uint, State> stateMap)
        {
            foreach (uint netId in statefulMap.Keys)
            {
                if (stateMap.ContainsKey(netId))
                {
                    // TODO: Fix - use the state passed
                    State state = new State { position = stateMap[netId].position, playerState = new PlayerState() };
                    statefulMap[netId].SetState(state);
                }
            }
        }
    }
}
