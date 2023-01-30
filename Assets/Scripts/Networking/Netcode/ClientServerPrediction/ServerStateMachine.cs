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

                    if (inputContext.inputs.Count == 0)
                    {
                        continue;
                    }

                    if (!inputBufferMap.ContainsKey(inputContext.netId))
                    {
                        InputBuffer<Inputs> inputBuffer = new InputBuffer<Inputs>(bufferSize);
                        inputBufferMap.Add(inputContext.netId, inputBuffer);
                    }

                    uint newestTick = inputMessage.startTick + (uint)inputContext.inputs.Count - 1;
                    uint firstTick = inputMessage.startTick;

                    if (inputBufferMap[inputContext.netId].HasRecieved && inputBufferMap[inputContext.netId].LastRecieved().clientTick > firstTick)
                    {
                        firstTick = inputBufferMap[inputContext.netId].LastRecieved().clientTick + 1;
                    }

                    for (int index = (int)firstTick; index <= newestTick; index++)
                    {
                        int packageTick = index - (int)inputMessage.startTick;
                        inputBufferMap[inputContext.netId].Enqueue(inputContext.inputs[packageTick], (uint)index);
                    }
                }
            }
        }

        public static void ApplyInput(ref Dictionary<uint, InputBuffer<Inputs>> inputBufferMap,
                                      ref Dictionary<uint, IInputful> inputMap,
                                      uint serverTick,
                                      bool frozen=false)
        {
            //Debug.Log("Applying Tick");
            foreach(uint id in inputBufferMap.Keys)
            {
                if(inputMap.ContainsKey(id) && !inputBufferMap[id].IsEmpty)
                {
                    InputPacket<Inputs> inputPacket = inputBufferMap[id].Dequeue(serverTick);
                    if(!frozen)
                    {
                        inputMap[id].ApplyInput(inputPacket.input);
                    }
                }
            }
        }

        public static StateMessage CreateStateMessage(ref Dictionary<uint, InputBuffer<Inputs>> inputBufferMap,
                                                      in Dictionary<uint, IStateful> stateMap,
                                                      uint serverTick,
                                                      bool frozen=false)
        {
            StateMessage stateMessage = new StateMessage { serverTick = serverTick, stateContexts = new List<StateContext>(), frozen = frozen };

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

                if(!inputBufferMap.ContainsKey(netId) ||
                   inputBufferMap[netId].BeenProcessed)
                {
                    stateMessage.stateContexts.Add(stateContext);
                }
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