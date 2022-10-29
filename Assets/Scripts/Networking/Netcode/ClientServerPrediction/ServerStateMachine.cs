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

                foreach (InputContext inputContext in inputMessage.inputContexts)
                {

                    if (!inputBufferMap.ContainsKey(inputContext.netId))
                    {
                        InputBuffer<Inputs> inputBuffer = new InputBuffer<Inputs>(bufferSize);
                        inputBufferMap.Add(inputContext.netId, inputBuffer);
                    }

                    uint newestTick = inputMessage.startTick + (uint)inputContext.inputs.Count - 1;
                    uint expectedTick = inputBufferMap[inputContext.netId].BeenProcessed ? inputBufferMap[inputContext.netId].LastRecieved().clientTick : newestTick;


                    if (newestTick >= expectedTick)
                    {
                        for (int index = (int)expectedTick; index <= newestTick; index++)
                        {
                            inputBufferMap[inputContext.netId].Enqueue(inputContext.inputs[index - (int)inputMessage.startTick], (uint)index);
                        }
                    }
                }
            }
        }

        public static void ApplyInput(ref Dictionary<uint, InputBuffer<Inputs>> inputBufferMap,
                                      ref Dictionary<uint, IInputful> inputMap,
                                      uint serverTick)
        {
            foreach(uint id in inputBufferMap.Keys)
            {
                if(inputMap.ContainsKey(id) && inputBufferMap[id].Count > 0)
                {
                    InputPacket<Inputs> inputPacket = inputBufferMap[id].Dequeue(serverTick);
                    // TODO: Integrate with StateMachine.Run?
                    inputMap[id].ApplyInput(inputPacket.input);
                }
            }
        }

        public static StateMessage CreateStateMessage(ref Dictionary<uint, InputBuffer<Inputs>> inputBufferMap,
                                                      in Dictionary<uint, IStateful> stateMap,
                                                      uint serverTick)
        {
            StateMessage stateMessage = new StateMessage { serverTick = serverTick, stateContexts = new List<StateContext>() };

            foreach (uint netId in stateMap.Keys)
            {
                StateContext stateContext = new StateContext
                {
                    netId = netId,
                    state = stateMap[netId].GetState()
                };

                if (inputBufferMap.ContainsKey(netId) &&
                   inputBufferMap[netId].BeenProcessed)
                {
                    TickSync tickSync = new TickSync
                    {
                        lastProcessedClientTick = inputBufferMap[netId].LastProcessed().clientTick,
                        lastProcessedServerTick = inputBufferMap[netId].LastProcessed().serverTick
                    };
                    stateContext.tickSync = tickSync;
                }

                stateMessage.stateContexts.Add(stateContext);
            }

            return stateMessage;
        }

        public static void SendStateMessage(in StateMessage stateMessage,
                                            ref Queue<StateMessage> stateQueue)
        {
            stateQueue.Enqueue(stateMessage);
        }
    }
}