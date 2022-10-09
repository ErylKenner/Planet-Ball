using System.Collections.Generic;

namespace ClientServerPrediction
{
    public static class ServerStateMachine
    {
        public static void ProcessInputMessages(ref Queue<InputMessage> inputQueue,
                                                ref Dictionary<uint, InputBuffer<Inputs>> inputBufferMap)
        {

        }

        public static void ApplyInput(ref Dictionary<uint, InputBuffer<Inputs>> inputBufferMap,
                                      ref Dictionary<uint, IInputful> inputMap)
        {

        }

        public static StateMessage CreateStateMessage(in Dictionary<uint, IStateful> stateMap,
                                                      ref Dictionary<uint, InputBuffer<Inputs>> inputBufferMap)
        {
            return new StateMessage();
        }

        public static void SendStateMessage(in StateMessage stateMessage,
                                            ref Queue<StateMessage> stateQueue)
        {

        }
    }
}