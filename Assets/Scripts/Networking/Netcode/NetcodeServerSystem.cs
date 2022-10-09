using NetcodeData;
using UnityEngine;

public static class NetcodeServerSystem
{
    public static void ServerUpdateInputBuffer(NetcodePlayer[] netcodePlayers)
    {
        foreach (NetcodePlayer netcodePlayer in netcodePlayers)
        {
            while (netcodePlayer.server_input_msgs?.Count > 0)
            {
                InputMessage input_msg = netcodePlayer.server_input_msgs.Dequeue();

                // message contains an array of inputs, calculate what tick the final one is
                uint max_tick = input_msg.start_tick_number + (uint)input_msg.inputs.Count - 1;

                // if that tick is greater than or equal to the current tick we're on, then it
                // has inputs which are new
                uint lastRecievedClientTick = netcodePlayer.server_input_buffer.Ready ? netcodePlayer.server_input_buffer.LastRecieved().clientTick : 0;
                if (max_tick >= lastRecievedClientTick)
                {
                    // there may be some inputs in the array that we've already had,
                    // so figure out where to start
                    uint start_i = lastRecievedClientTick > input_msg.start_tick_number ? (lastRecievedClientTick - input_msg.start_tick_number) : 0;

                    // run through all relevant inputs, and step player forward
                    for (int i = (int)start_i; i < input_msg.inputs.Count; ++i)
                    {
                        //InputPacket<Inputs> inputPacket;
                        //inputPacket.input = input_msg.inputs[i];
                        //inputPacket.clientTick = input_msg.start_tick_number + (uint)i;
                        //inputPacket.serverTick = 0;
                        //netcodePlayer.server_input_buffer.Enqueue(inputPacket);
                    }
                }
            }

            //SamplePlayerTick(netcodePlayer);
        }
    }

    public static bool ServerDesireProgression(uint serverTickNumber, uint serverInputBuffer, ref uint desiredTickNumber)
    {
        desiredTickNumber++;
        return desiredTickNumber - serverTickNumber < serverInputBuffer;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="netcodePlayers"></param>
    /// <param name="serverTickNumber"></param>
    /// <param name="desiredServerTickNumber"></param>
    /// <param name="dt"></param>
    /// <returns>Server tick delta after progression</returns>
    public static uint ServerProgress(NetcodePlayer[] netcodePlayers, uint serverTickNumber, uint desiredServerTickNumber, float dt)
    {

        uint serverTickNumberDelta = 0;

        for (int i = (int)serverTickNumber; i < desiredServerTickNumber; ++i)
        {

            foreach (NetcodePlayer netcodePlayer in netcodePlayers)
            {
                if (netcodePlayer.server_input_buffer.Count > 0)
                {
                    NetcodeSystem.PrePhysicsStep(netcodePlayer, netcodePlayer.server_input_buffer.Dequeue(serverTickNumber).input);
                }
            }

            Physics.Simulate(dt);
            serverTickNumberDelta++;
        }


        return serverTickNumberDelta;
    }

    public static GlobalStateMessage? ServerSendState(NetcodeObject[] netcodeObjects, uint serverTickNumber)
    {

        if (netcodeObjects.Length == 0)
        {
            return null;
        }

        GlobalStateMessage state_msg;
        state_msg.states = new StateMessage[netcodeObjects.Length];

        for (int i = 0; i < netcodeObjects.Length; i++)
        {
            State state;
            Rigidbody2D rigidbody = netcodeObjects[i].GetComponent<Rigidbody2D>();
            state.netId = netcodeObjects[i].netId;
            state.state.position = rigidbody.position;
            state.state.rotation = rigidbody.rotation;
            state.state.velocity = rigidbody.velocity;
            state.state.angularVelocity = rigidbody.angularVelocity;

            StateMessage stateMessage;
            stateMessage.serverTick = serverTickNumber;
            stateMessage.state = state;

            // Attached the last processed server and client tick for each player
            // TODO: Send individual messages to each player instead of using GlobalStateMessage
            if (netcodeObjects[i] is NetcodePlayer)
            {
                NetcodePlayer player = (NetcodePlayer)netcodeObjects[i];

                if (player.server_input_buffer.Ready)
                {
                    InputPacket<Inputs> inputPacket = player.server_input_buffer.LastProcessed();
                    stateMessage.lastProcessedClientTick = inputPacket.clientTick;
                    stateMessage.lastProcessedServerTick = inputPacket.serverTick;
                }
                else
                {
                    // TODO: Send to individual players
                    return null;
                }
            }
            else
            // Ignored if not a player
            {
                stateMessage.lastProcessedClientTick = 0;
                stateMessage.lastProcessedServerTick = 0;
            }
            state_msg.states[i] = stateMessage;

        }

        return state_msg;
    }
}
