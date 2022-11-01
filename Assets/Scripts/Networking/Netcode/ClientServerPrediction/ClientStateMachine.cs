
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ClientServerPrediction
{
    public static class ClientStateMachine
    {
        public static StateMessage GetLatestStateMessage(ref Queue<StateMessage> stateQueue, uint netId)
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

            if(!stateMessage.GetMap().ContainsKey(netId))
            {
                return null;
            }

            return stateMessage;
        }
        public static uint CorrectClient(ref Dictionary<uint, Inputs[]> inputBufferMap,
                                         ref Dictionary<uint, State[]> stateBufferMap,
                                         ref Dictionary<uint, IInputful> inputMap,
                                         ref Dictionary<uint, IStateful> stateMap,
                                         in StateMessage stateMessage,
                                         in StateError stateError,
                                         IRunnable simulation,
                                         RunContext runContext,
                                         uint netId,
                                         uint clientTick,
                                         uint lastReceivedTick)
        {
            Dictionary<uint, StateContext> messageMap = stateMessage.GetMap();

            if(!messageMap.ContainsKey(netId))
            {
                throw new System.InvalidOperationException($"stateMessage must contain a Context with the player NetId");
            }

            // Grab the TickSync from the client netId
            TickSync tickSync = messageMap[netId].tickSync;

            if(tickSync == null)
            {
                throw new System.InvalidOperationException($"stateMessage must contain a Context with the player NetId and a non-null tickSync");
            }

            //if(tickSync == null)
            //{
            //    return 0;
            //}

            //if (tickSync.lastProcessedClientTick <= tickSync.lastProcessedServerTick)
            //{
            //    throw new System.InvalidOperationException($"Last processed client tick {tickSync.lastProcessedClientTick} must be greater than last processed server tick {tickSync.lastProcessedServerTick}");
            //}

            // Client offset = lastProcessedClientTick - lastProcessedServerTick
            // Message Client tick = serverTick + clientOffset
            uint messageClientTick = stateMessage.MessageClientTick(netId);
            // Assert serverTick > lastProcessedServerTick
            if(stateMessage.serverTick <= tickSync.lastProcessedServerTick)
            {
                throw new System.InvalidOperationException($"Server tick {stateMessage.serverTick} must be greater than last processed {tickSync.lastProcessedServerTick}");
            }
            uint inputLoss = stateMessage.serverTick - tickSync.lastProcessedServerTick - 1;

            Dictionary<uint, float> errors = new Dictionary<uint, float>();

            foreach(uint currentNetId in messageMap.Keys)
            {
                State[] stateBuffer = stateBufferMap[currentNetId];
                int stateBufferIndex = (int)messageClientTick % stateBuffer.Length;

                Vector2 stateBufferPosition = stateBuffer[stateBufferIndex].position;
                State messageState = messageMap[currentNetId].state;
                Vector2 messagePosition = messageState.position;
                Vector2 diffVector = stateBufferPosition - messagePosition;
                errors.Add(currentNetId, diffVector.magnitude);
            }

            float positionDiff = stateError.positionDiff;

            List<uint> keyList = errors.Keys.ToList();
            int offendingNetIdIndex = keyList.FindIndex(key => errors[key] > positionDiff);

            // Compare state recieved with predicted state
            if (offendingNetIdIndex != -1) {
                Debug.Log($"Correcting {messageClientTick} to {clientTick} (Loss: {inputLoss}) (Offset: {errors[keyList[offendingNetIdIndex]]})");
                // Do correction
                // foreach StateContext
                foreach (StateContext stateContext in stateMessage.stateContexts)
                {
                    // Update the state buffer
                    stateBufferMap[stateContext.netId][messageClientTick % stateBufferMap[netId].Length] = stateContext.state;
                    // Set the Stateful state
                    stateMap[stateContext.netId].SetState(stateContext.state);
                }


                // from messageClientTick -> clientTick
                for (int i = (int)messageClientTick; i < clientTick; i++) {

                    if (!stateMessage.frozen)
                    {
                        // foreach StateContext
                        foreach (StateContext stateContext in stateMessage.stateContexts)
                        {
                            // Only apply future input to the player
                            if (stateContext.netId == netId)
                            {
                                // Apply input from input buffer
                                inputMap[stateContext.netId].ApplyInput(inputBufferMap[stateContext.netId][i % inputBufferMap[stateContext.netId].Length]);
                            }
                        }
                    }

                    // Run simulation
                    simulation.Run(runContext);

                    // Store state
                    foreach (StateContext stateContext in stateMessage.stateContexts)
                    {
                        // Update the state buffer
                        stateBufferMap[stateContext.netId][(i + 1) % stateBufferMap[netId].Length] = stateMap[stateContext.netId].GetState();
                    }

                    }
            }

            return messageClientTick - 1;
        }

        public static void StoreState(ref Dictionary<uint, State[]> stateBufferMap,
                                      in Dictionary<uint, IStateful> stateMap,
                                      uint clientTick)
        {
            foreach (uint id in stateMap.Keys)
            {
                if (stateBufferMap.ContainsKey(id))
                {
                    stateBufferMap[id][clientTick % stateBufferMap[id].Length] = stateMap[id].GetState();
                }
            }
        }

        public static Dictionary<uint, Inputs> StoreInput(ref Dictionary<uint, Inputs[]> inputBufferMap,
                                      in Dictionary<uint, IInputful> inputMap,
                                      uint clientTick,
                                      bool frozen=false)
        {
            Dictionary<uint, Inputs> currentInputMap = new Dictionary<uint, Inputs>();
            foreach (uint id in inputMap.Keys)
            {
                if (inputBufferMap.ContainsKey(id))
                {
                    Inputs currentInput = frozen ? new Inputs() : inputMap[id].GetInput();
                    inputBufferMap[id][clientTick % inputBufferMap[id].Length] = currentInput;
                    currentInputMap.Add(id, currentInput);
                }
            }

            return currentInputMap;
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
            inputMessageQueue.Enqueue(inputMessage);
        }
    }
}