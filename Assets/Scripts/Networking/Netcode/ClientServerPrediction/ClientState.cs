using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ClientServerPrediction
{
    public class ClientState
    {
        public readonly uint bufferSize = 1024;
        public uint tick = 1;
        public uint lastReceivedTick = 0;
        public Queue<StateMessage> stateMessageQueue = new Queue<StateMessage>();
        public Dictionary<uint, IInputful> inputMap = new Dictionary<uint, IInputful>();
        public Dictionary<uint, IStateful> stateMap = new Dictionary<uint, IStateful>();
        public Dictionary<uint, Inputs[]> inputBufferMap = new Dictionary<uint, Inputs[]>();
        public Dictionary<uint, State[]> stateBufferMap = new Dictionary<uint, State[]>();

        public uint? localNetId = null;
        public bool frozen = false;

        // Debug objects
        public Vector2 lastServerMessage = Vector2.zero;

        public void AddInputful(IInputful player, uint netId, bool isLocal=false)
        {
            if(isLocal)
            {
                localNetId = netId;
                inputMap.Add(netId, player);
                Inputs[] inputBuffer = new Inputs[bufferSize];
                inputBufferMap.Add(netId, inputBuffer);
            }
        }

        public void AddStateful(IStateful stateful, uint netId)
        {
            stateMap.Add(netId, stateful);
            State[] stateBuffer = new State[bufferSize];
            stateBufferMap.Add(netId, stateBuffer);
        }

        public void DeleteInputful(uint netId)
        {
            if(netId == localNetId)
            {
                stateMap.Remove(netId);
                stateBufferMap.Remove(netId);
            }
        }

        public void DeleteStateful(uint netId)
        {
            stateMap.Remove(netId);
            stateBufferMap.Remove(netId);
        }

        public InputMessage Tick(IRunnable runner, RunContext runContext, bool isClientOnly=true)
        {
            if(localNetId == null)
            {
                return null;
            }

            Dictionary<uint, State> statesBeforeCorrection = null;
            StateError stateError = new StateError { positionDiff = 0.01f, snapDistance = 5f };

            if (isClientOnly)
            {
                StateMessage lastestStateMessage = ClientStateMachine.GetLatestStateMessage(ref stateMessageQueue, (uint)localNetId);

                if (lastestStateMessage != null)
                {
                    frozen = lastestStateMessage.frozen;
                    if (frozen)
                    {
                        StateMachine.SetState(ref stateMap, lastestStateMessage.GetMap().ToDictionary(kp => kp.Key, kp => kp.Value.state));
                        runner.Run(runContext);
                        lastReceivedTick = lastestStateMessage.MessageClientTick((uint)localNetId);

                        frozen = lastestStateMessage.frozen;
                    } else
                    {
                        lastServerMessage = lastestStateMessage.GetMap()[(uint)localNetId].state.position;

                        

                        lastReceivedTick = ClientStateMachine.CorrectClient(
                            ref inputBufferMap,
                            ref stateBufferMap,
                            ref inputMap,
                            ref stateMap,
                            ref statesBeforeCorrection,
                            in lastestStateMessage,
                            in stateError,
                            runner,
                            runContext,
                            (uint)localNetId,
                            tick,
                            lastReceivedTick
                         );
                    }

                }
            }


            Dictionary<uint, Inputs> currentInputMap = ClientStateMachine.StoreInput(ref inputBufferMap, in inputMap, tick, frozen);

            if (isClientOnly)
            {
                StateMachine.Run(currentInputMap, ref inputMap, ref stateMap, runner, runContext);
            }

            InputMessage inputMessage = ClientStateMachine.CreateInputMessage(in inputBufferMap, lastReceivedTick, tick);
            tick++;
            ClientStateMachine.StoreState(ref stateBufferMap, in stateMap, tick);

            ClientStateMachine.SmoothState(ref stateMap, in statesBeforeCorrection, runContext, stateError);

            return inputMessage;
        }
    }
}
