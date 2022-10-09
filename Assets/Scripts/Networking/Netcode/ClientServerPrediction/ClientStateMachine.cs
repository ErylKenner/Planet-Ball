
using System.Collections.Generic;

namespace ClientServerPrediction
{
    public static class ClientStateMachine
    {
        public static void CorrectClient(ref Dictionary<uint, Input[]> inputBufferMap,
                                         ref Dictionary<uint, State[]> stateBufferMap,
                                         in Dictionary<uint, IStateful> stateMap,
                                         in Dictionary<uint, IInputful> inputMap,
                                         in StateMessage stateMessage)
        {

        }

        public static void StoreState(ref Dictionary<uint, State[]> stateBufferMap,
                                      in Dictionary<uint, IStateful> stateMap)
        {

        }

        public static void StoreInput(ref Dictionary<uint, Input[]> inputBufferMap,
                                      in Dictionary<uint, IInputful> inputMap)
        {

        }

        public static void SendInputMessage(in Dictionary<uint, Input[]> inputBufferMap,
                                            ref Queue<InputMessage> inputMessageQueue)
        {

        }
    }
}