using System.Collections.Generic;

namespace ClientServerPrediction
{
    public static class StateMachine
    {
        public static void Run(in Dictionary<uint, Inputs> inputMap,
                               ref Dictionary<uint, IInputful> inputfulMap,
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

            runnable.Run(runContext);
        }
    }
}