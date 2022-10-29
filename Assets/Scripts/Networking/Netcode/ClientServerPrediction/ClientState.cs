using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ClientServerPrediction
{
    public class ClientState
    {
        public readonly uint bufferSize = 64;
        public uint tick = 1;
        public uint lastReceivedTick = 0;
        public Queue<StateMessage> stateMessageQueue = new Queue<StateMessage>();
        public Dictionary<uint, IInputful> inputMap = new Dictionary<uint, IInputful>();
        public Dictionary<uint, IStateful> stateMap = new Dictionary<uint, IStateful>();
        public Dictionary<uint, Inputs[]> inputBufferMap = new Dictionary<uint, Inputs[]>();
        public Dictionary<uint, State[]> stateBufferMap = new Dictionary<uint, State[]>();


        public void AddPlayer(IPlayerful player, uint netId)
        {
            inputMap.Add(netId, player);
            Inputs[] inputBuffer = new Inputs[bufferSize];
            inputBufferMap.Add(netId, inputBuffer);

            AddObject(player, netId);
        }

        public void AddObject(IStateful stateful, uint netId)
        {
            stateMap.Add(netId, stateful);
            State[] stateBuffer = new State[bufferSize];
            stateBufferMap.Add(netId, stateBuffer);
        }

        public InputMessage Tick(IRunnable runner, RunContext runContext, uint playerNetId)
        {
            StateMessage lastestStateMessage = ClientStateMachine.GetLatestStateMessage(ref stateMessageQueue);
            if(lastestStateMessage != null)
            {
                StateError stateError = new StateError { positionDiff = 0.1f };

                uint lastReceivedTick = ClientStateMachine.CorrectClient(
                    ref inputBufferMap,
                    ref stateBufferMap,
                    ref inputMap,
                    ref stateMap,
                    in lastestStateMessage,
                    in stateError,
                    runner,
                    runContext,
                    playerNetId,
                    tick
                 );
            }

            Dictionary<uint, Inputs> currentInputMap = ClientStateMachine.StoreInput(ref inputBufferMap, in inputMap, tick);
            StateMachine.Run(currentInputMap, ref inputMap, runner, runContext);
            InputMessage inputMessage = ClientStateMachine.CreateInputMessage(inputBufferMap, lastReceivedTick, tick);
            tick++;
            ClientStateMachine.StoreState(ref stateBufferMap, in stateMap, tick);
            return inputMessage;
        }
    }
}
