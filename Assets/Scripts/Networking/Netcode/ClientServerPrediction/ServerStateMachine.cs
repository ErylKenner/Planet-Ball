using System.Collections.Generic;

namespace ClientServerPrediction
{
    public static class ServerStateMachine
    {
        public static void ProcessInputMessages(ref Queue<InputMessage> inputQueue,
                                                ref Dictionary<uint, InputBuffer<Inputs>> inputBufferMap,
                                                uint bufferSize)
        {
            while(inputQueue.Count > 0)
            {
                InputMessage inputMessage = inputQueue.Dequeue();

                if(!inputBufferMap.ContainsKey(inputMessage.netId))
                {
                    InputBuffer<Inputs> inputBuffer = new InputBuffer<Inputs>(bufferSize);
                    inputBufferMap.Add(inputMessage.netId, inputBuffer);
                }

                uint newestTick = inputMessage.startTick + (uint)inputMessage.inputs.Count - 1;
                uint expectedTick = inputBufferMap[inputMessage.netId].Ready ? inputBufferMap[inputMessage.netId].LastRecieved().clientTick : newestTick;
                

                if(newestTick >= expectedTick)
                {
                    for(int index = (int)expectedTick; index <= newestTick; index++ )
                    {
                        inputBufferMap[inputMessage.netId].Enqueue(inputMessage.inputs[index - (int)inputMessage.startTick], (uint)index);
                    }
                }
            }
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