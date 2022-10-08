using NetcodeData;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class NetcodeClientSystem
{

    public static void ClientProgress(NetcodePlayer player,
                                      NetcodeObject[] netcodeObjects,
                                      Inputs[] clientInputBuffer,
                                      uint bufferSlot,
                                      float dt)
    {



        // store state for this tick, then use current state + input to step simulation
        foreach (NetcodeObject netcodeObject in netcodeObjects)
        {
            netcodeObject.StoreClientState(bufferSlot);
        }

        NetcodeSystem.PrePhysicsStep(player, clientInputBuffer[bufferSlot]);
        Physics.Simulate(dt);



    }

    public static InputMessage GenerateClientInputMessage(int clientLastRecievedTick,
                                                          uint clientTickNumber,
                                                          Inputs[] clientInputBuffer,
                                                          uint clientBufferSize
                                                          )
    {
        // send input packet to server

        InputMessage input_msg;
        input_msg.start_tick_number = (uint)(clientLastRecievedTick == -1 ? 0 : clientLastRecievedTick);
        input_msg.inputs = new List<Inputs>();

        for (uint tick = input_msg.start_tick_number; tick <= clientTickNumber; ++tick)
        {
            input_msg.inputs.Add(clientInputBuffer[tick % clientBufferSize]);
        }

        return input_msg;
    }

    public static bool ClientCalculateCorrection(Dictionary<uint, NetcodeObject> netIdToNetcodeObject,
                                                 Queue<GlobalStateMessage> clientStateMessages,
                                                 uint clientBufferSize,
                                                 float dt,
                                                 out Dictionary<uint, State> netIdToState)
    {
        // If there are available messages
        if (clientStateMessages.Count == 0)
        {
            netIdToState = null;
            return false;
        }

        GlobalStateMessage state_msg = clientStateMessages.Dequeue();
        while (clientStateMessages.Count > 0) // make sure if there are any newer state messages available, we use those instead
        {
            state_msg = clientStateMessages.Dequeue();
        }

        NetcodePlayer.LocalPlayer.UpdateClientLastReceivedTick(state_msg);
        uint buffer_slot = NetcodePlayer.LocalPlayer.ClientLastRecievedTick % clientBufferSize;
        netIdToState = new Dictionary<uint, State>();
        List<Vector2> position_errors = new List<Vector2>();
        List<float> rotation_errors = new List<float>();

        float playerPositionError = 0f;
        foreach (StateMessage state in state_msg.states)
        {
            netIdToState.Add(state.state.netId, state.state);
            if (!netIdToNetcodeObject.ContainsKey(state.state.netId))
            {
                continue;
            }

            NetcodeObject currentObject = netIdToNetcodeObject[state.state.netId];
            //Debug.Log(currentObject.gameObject.name);

            Vector2 position_error = state.state.state.position - currentObject.client_state_buffer[buffer_slot].position;

            if (currentObject.isLocalPlayer)
            {
                playerPositionError = position_error.magnitude;
            }

            //Debug.Log(position_error);
            position_errors.Add(position_error);
            float rotation_error = state.state.state.rotation - currentObject.client_state_buffer[buffer_slot].rotation;
            rotation_errors.Add(rotation_error);
        }

        const float positionCorrectionMargin = 0.01f;

        bool doCorrection = (position_errors.Any(error => error.sqrMagnitude > positionCorrectionMargin)); //||
                                                                                                            //rotation_errors.Any(error => Mathf.Abs(error) > 0.00001f));

        // Perform correction on a client only

        if(doCorrection)
        {
            //PostionErrorMetric.Sample(playerPositionError);
            //last_correction_tick = (int)NetcodePlayer.LocalPlayer.client_tick_number;
        }

        return doCorrection;

        //PostionCorrectionTick.Sample(last_correction_tick);
    }

    public static void ClientPerformCorrection(NetcodeObject[] netcodeObjects,
                                                uint last_recieved_buffer_slot,
                                                Dictionary<uint, State> netIdToState,
                                                uint clientBufferSize,
                                                float dt)
    {
        //Debug.Log("Correcting for error at tick " + state_msg.tick_number + " (rewinding " + (client_tick_number - state_msg.tick_number) + " ticks)");
        Dictionary<NetcodeObject, Vector2> previousPositions = new Dictionary<NetcodeObject, Vector2>();
        Dictionary<NetcodeObject, float> previousRotations = new Dictionary<NetcodeObject, float>();

        foreach (NetcodeObject netcodeObject in netcodeObjects)
        {
            Rigidbody2D netcodeRigidbody2D = netcodeObject.GetComponent<Rigidbody2D>();
            // capture the current predicted pos for smoothing

            // TODO: Add back error values when smoothing is implemented
            //Vector2 prev_pos = netcodeRigidbody2D.position + netcodeObject.client_error.position;
            Vector2 prev_pos = netcodeRigidbody2D.position;
            previousPositions.Add(netcodeObject, prev_pos);
            //float prev_rot = netcodeRigidbody2D.rotation + netcodeObject.client_error.rotation;
            float prev_rot = netcodeRigidbody2D.rotation;
            previousRotations.Add(netcodeObject, prev_rot);
            State currentState = netIdToState[netcodeObject.netId];

            // rewind & replay
            netcodeRigidbody2D.position = currentState.state.position;
            netcodeRigidbody2D.rotation = currentState.state.rotation;
            netcodeRigidbody2D.velocity = currentState.state.velocity;
            netcodeRigidbody2D.angularVelocity = currentState.state.angularVelocity;

        }

        uint rewind_buffer_slot = last_recieved_buffer_slot;
        while (rewind_buffer_slot < NetcodePlayer.LocalPlayer.client_tick_number)
        {
            uint buffer_slot = rewind_buffer_slot % clientBufferSize;

            foreach (NetcodeObject netcodeObject in netcodeObjects)
            {
                netcodeObject.StoreClientState(buffer_slot);
                if (netcodeObject is NetcodePlayer)
                {
                    NetcodePlayer netcodePlayer = (NetcodePlayer)netcodeObject;
                    // TODO: When we send foreign player inputs, we cannot apply those to the replay
                    // I don't think it will help to send foreign inputs
                    // The server generates the state based on the inputs
                    // This client can only consider the input it makes because those are the only inputs in the future of the server
                    // None of this explains why having two player is COMPLETELY broken right now
                    // TODO: It's broken because the client starts at tick 0 by default and sends information as if it's in the past
                    NetcodeSystem.PrePhysicsStep(netcodePlayer, netcodePlayer.client_input_buffer[buffer_slot]);
                }
            }
            Physics.Simulate(dt);

            ++rewind_buffer_slot;
        }

        foreach (NetcodeObject netcodeObject in netcodeObjects)
        {
            Rigidbody2D netcodeRigidbody2D = netcodeObject.GetComponent<Rigidbody2D>();
            Vector2 prev_pos = previousPositions[netcodeObject];
            float prev_rot = previousRotations[netcodeObject];

            if (netcodeObject.isLocalPlayer)
            {
                //PostionCorrectionMetric.Sample((prev_pos - netcodeRigidbody2D.position).magnitude);
            }

            // if more than 2ms apart, just snap
            if ((prev_pos - netcodeRigidbody2D.position).sqrMagnitude >= 4.0f)
            {
                netcodeObject.client_error.position = Vector2.zero;
                netcodeObject.client_error.rotation = 0f;
            }
            else
            {
                netcodeObject.client_error.position = prev_pos - netcodeRigidbody2D.position;
                netcodeObject.client_error.rotation = prev_rot - netcodeRigidbody2D.rotation;
            }
        }
    }
}
