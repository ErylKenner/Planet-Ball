
using System.Collections.Generic;

namespace ClientServerPrediction
{
    public static class ClientStateMachine
    {
        public static StateMessage GetLatestStateMessage(ref Queue<StateMessage> stateQueue)
        {
            if(stateQueue.Count == 0)
            {
                return null;
            }

            StateMessage stateMessage = stateQueue.Dequeue();
            while(stateQueue.Count > 0)
            {
                StateMessage currentStateMessage = stateQueue.Dequeue();
                if(currentStateMessage.serverTick > stateMessage.serverTick)
                {
                    stateMessage = currentStateMessage;
                }
            }

            return stateMessage;
        }
        public static void CorrectClient(ref Dictionary<uint, Inputs[]> inputBufferMap,
                                         ref Dictionary<uint, State[]> stateBufferMap,
                                         ref Dictionary<uint, IStateful> stateMap,
                                         ref Dictionary<uint, IInputful> inputMap,
                                         in StateMessage stateMessage)
        {

        }

        public static void StoreState(ref Dictionary<uint, State[]> stateBufferMap,
                                      in Dictionary<uint, IStateful> stateMap,
                                      uint bufferSlot)
        {
            foreach (uint id in stateMap.Keys)
            {
                if (stateBufferMap.ContainsKey(id))
                {
                    if(bufferSlot >= stateBufferMap[id].Length)
                    {
                        throw new System.IndexOutOfRangeException($"Slot {bufferSlot} is out of range of {id}'s buffer of length {stateBufferMap[id].Length}");
                    }

                    stateBufferMap[id][bufferSlot] = stateMap[id].GetState();
                }
            }
        }

        public static void StoreInput(ref Dictionary<uint, Inputs[]> inputBufferMap,
                                      in Dictionary<uint, IInputful> inputMap,
                                      uint bufferSlot)
        {
            foreach (uint id in inputMap.Keys)
            {
                if (inputBufferMap.ContainsKey(id))
                {
                    if (bufferSlot >= inputBufferMap[id].Length)
                    {
                        throw new System.IndexOutOfRangeException($"Slot {bufferSlot} is out of range of {id}'s buffer of length {inputBufferMap[id].Length}");
                    }

                    inputBufferMap[id][bufferSlot] = inputMap[id].GetInput();
                }
            }
        }

        public static InputMessage CreateInputMessage(in Dictionary<uint, Inputs[]> inputBufferMap, uint lastReceivedTick, uint clientTick)
        {
            uint startTick = lastReceivedTick + 1;
            InputMessage inputMessage = new InputMessage { startTick = startTick };

            foreach(uint id in inputBufferMap.Keys)
            {
                InputContext inputContext = new InputContext { netId = id };
                for(int index = (int)startTick; index <= clientTick; index++)
                {
                    inputContext.inputs.Add(inputBufferMap[id][index % inputBufferMap[id].Length]);
                }
                inputMessage.inputContexts.Add(inputContext);
            }

            return inputMessage;
        }

        public static void SendInputMessage(InputMessage inputMessage,
                                            ref Queue<InputMessage> inputMessageQueue)
        {

        }
    }
}