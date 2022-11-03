using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ClientServerPrediction
{
    public class ServerState
    {
        public readonly uint bufferSize = 1024;
        public uint tick = 0;
        public Queue<InputMessage> inputMessageQueue = new Queue<InputMessage>();
        public Dictionary<uint, IInputful> serverInputMap = new Dictionary<uint, IInputful>();
        public Dictionary<uint, IStateful> serverStateMap = new Dictionary<uint, IStateful>();
        public Dictionary<uint, InputBuffer<Inputs>> serverInputBufferMap = new Dictionary<uint, InputBuffer<Inputs>>();

        public Dictionary<uint, State> initialStateMap = new Dictionary<uint, State>();
        public bool frozen = false;

        public void AddInputful(IInputful player, uint netId)
        {
            serverInputMap.Add(netId, player);
            InputBuffer<Inputs> inputBuffer = new InputBuffer<Inputs>(bufferSize);
            serverInputBufferMap.Add(netId, inputBuffer);
        }

        public void AddStateful(IStateful stateful, uint netId)
        {
            serverStateMap.Add(netId, stateful);
            initialStateMap.Add(netId, stateful.GetState());
        }

        public void DeleteInputful(uint netId)
        {
            serverInputMap.Remove(netId);
            serverInputBufferMap.Remove(netId);
        }

        public void DeleteStateful(uint netId)
        {
            serverStateMap.Remove(netId);
        }
        public StateMessage Tick(IRunnable runner, RunContext runContext)
        {
            ServerStateMachine.ProcessInputMessages(ref inputMessageQueue, ref serverInputBufferMap, bufferSize);
            ServerStateMachine.ApplyInput(ref serverInputBufferMap, ref serverInputMap, tick, frozen);

            runner.Run(runContext);
            tick++;
            return ServerStateMachine.CreateStateMessage(ref serverInputBufferMap, serverStateMap, tick, frozen);
        }

        public void ResetState(IRunnable runner, RunContext runContext)
        {
            StateMachine.SetState(ref serverStateMap, in initialStateMap);
        }

        public void Freeze(bool freeze)
        {
            frozen = freeze;
        }
    }
}
