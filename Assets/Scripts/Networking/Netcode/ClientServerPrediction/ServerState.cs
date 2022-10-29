using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ClientServerPrediction
{
    public class ServerState
    {
        public readonly uint bufferSize = 64;
        public uint tick = 0;
        public Queue<InputMessage> inputMessageQueue = new Queue<InputMessage>();
        public Dictionary<uint, IInputful> serverInputMap = new Dictionary<uint, IInputful>();
        public Dictionary<uint, IStateful> serverStateMap = new Dictionary<uint, IStateful>();
        public Dictionary<uint, InputBuffer<Inputs>> serverInputBufferMap = new Dictionary<uint, InputBuffer<Inputs>>();

        public void AddPlayer(IPlayerful player, uint netId)
        {
            serverInputMap.Add(netId, player);
            AddObject(player, netId);
        }

        public void AddObject(IStateful stateful, uint netId)
        {
            serverStateMap.Add(netId, stateful);
        }
        public StateMessage Tick(IRunnable runner, RunContext runContext)
        {
            ServerStateMachine.ProcessInputMessages(ref inputMessageQueue, ref serverInputBufferMap, bufferSize);
            ServerStateMachine.ApplyInput(ref serverInputBufferMap, ref serverInputMap, tick);
            runner.Run(runContext);
            tick++;
            return ServerStateMachine.CreateStateMessage(ref serverInputBufferMap, serverStateMap, tick);
        }
    }
}
