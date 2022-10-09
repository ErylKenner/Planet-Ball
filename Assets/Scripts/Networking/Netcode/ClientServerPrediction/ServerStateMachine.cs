using System.Collections.Generic;

namespace ClientServerPrediction
{
    public static class ServerStateMachine
    {
        public static void ProcessInputMessages(ref Queue<InputMessage> inputQueue,
                                                ref Dictionary<uint, InputBuffer<Input>> inputBufferMap)
        {

        }

        public static void ApplyInput(ref Dictionary<uint, InputBuffer<Input>> inputBufferMap,
                                      ref Dictionary<uint, IInputful> inputMap)
        {

        }

        public static StateMessage CreateStateMessage(in Dictionary<uint, IStateful> stateMap,
                                                      ref Dictionary<uint, InputBuffer<Input>> inputBufferMap)
        {
            StateMessage stateMessage;
            return stateMessage;
        }

        public static void SendStateMessage(in StateMessage stateMessage,
                                            ref Queue<StateMessage> stateQueue)
        {

        }
    }
}