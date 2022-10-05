using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
using static NetcodePlayer;
using static NetcodeObject;
using System.Linq;
using TMPro;
using Unity.Profiling;

public class NetcodeManager : NetworkBehaviour
{
    #region Structs
    public struct State
    {
        public uint netId;
        public ClientState state;
    }

    public struct GlobalStateMessage
    {
        public StateMessage[] states;
    }

    #endregion

    #region Members

    // client specific
    public static readonly uint c_client_buffer_size = 1024;
    private Queue<GlobalStateMessage> client_state_msgs;

    // server specific
    public uint server_snapshot_rate; // Set by user (default 1)
    public uint server_tick_number;
    public static float server_timer;
    private uint desiredServerTickNumber;

    // TODO: Uptake larger buffer
    private uint serverInputBuffer = 1;

    #endregion

    #region Profiler Metrics

    // Ticks
    public static readonly ProfilerCategory NetcodeCategory = ProfilerCategory.Scripts;
    public static readonly ProfilerCounter<uint> ServerTickCounter =
        new ProfilerCounter<uint>(NetcodeCategory, "Server Tick", ProfilerMarkerDataUnit.Count);

    public static readonly ProfilerCounter<uint> ServerDesiredTickCounter =
    new ProfilerCounter<uint>(NetcodeCategory, "Server Desired Tick", ProfilerMarkerDataUnit.Count);

    public static readonly ProfilerCounter<uint> ClientTickCounter =
        new ProfilerCounter<uint>(NetcodeCategory, "Client Tick", ProfilerMarkerDataUnit.Count);

    public static readonly ProfilerCounter<uint> ClientLastReceivedTickCounter =
    new ProfilerCounter<uint>(NetcodeCategory, "Client Last Received Tick", ProfilerMarkerDataUnit.Count);

    public static readonly ProfilerCounter<uint> PlayerTickCounter1 =
        new ProfilerCounter<uint>(NetcodeCategory, "Player 1 Tick", ProfilerMarkerDataUnit.Count);

    public static readonly ProfilerCounter<uint> PlayerTickCounter2 =
        new ProfilerCounter<uint>(NetcodeCategory, "Player 2 Tick", ProfilerMarkerDataUnit.Count);

    public static readonly ProfilerCounter<uint>[] PlayerTickCounters = new ProfilerCounter<uint>[] { PlayerTickCounter1, PlayerTickCounter2 };
    public static List<(uint, ProfilerCounter<uint>)> NetIdToPlayerTickCounter = new List<(uint, ProfilerCounter<uint>)>();

    // Correction Metrics
    public static readonly ProfilerCounter<float> PostionErrorMetric =
        new ProfilerCounter<float>(NetcodeCategory, "Position Error", ProfilerMarkerDataUnit.Count);

    public static readonly ProfilerCounter<float> PostionCorrectionMetric =
        new ProfilerCounter<float>(NetcodeCategory, "Position Correction", ProfilerMarkerDataUnit.Count);

    public static readonly ProfilerCounter<float> PostionCorrectionTick =
        new ProfilerCounter<float>(NetcodeCategory, "Position Correction Tick", ProfilerMarkerDataUnit.Count);

    private int last_correction_tick;

    #endregion

    #region Start and Update
    private void Start()
    {
        client_state_msgs = new Queue<GlobalStateMessage>();
        server_tick_number = 0;
        server_timer = 0;
        desiredServerTickNumber = 0;
        

        last_correction_tick = 0;
    }

    private void Update()
    {
        float dt = Time.fixedDeltaTime;
        if (isClient)
        {
            UpdateClient(dt);
        }

        if (isServerOnly)
        {
            UpdateServer(dt);
        }

        SendProfilerMetrics();
    }

    #endregion

    private void SendProfilerMetrics()
    {
        ServerTickCounter.Sample(server_tick_number % NetcodeManager.c_client_buffer_size);
        ServerDesiredTickCounter.Sample(desiredServerTickNumber % NetcodeManager.c_client_buffer_size);

        NetcodePlayer localPlayer = NetcodePlayer.LocalPlayer;

        if(localPlayer)
        {
            // TODO: Move this metric to the client itself
            ClientTickCounter.Sample(localPlayer.client_tick_number % NetcodeManager.c_client_buffer_size);
            ClientLastReceivedTickCounter.Sample(localPlayer.ClientLastRecievedTick % NetcodeManager.c_client_buffer_size);
            PostionCorrectionTick.Sample(last_correction_tick);
        }

    }

    #region Client


    private void UpdateClient(float dt)
    {
        NetcodeObject[] netcodeObjects = FindObjectsOfType<NetcodeObject>();

        // Trigger player input
        foreach (NetcodeObject netcodeObject in netcodeObjects)
        {
            if (netcodeObject is NetcodePlayer)
            {
                NetcodePlayer player = (NetcodePlayer)netcodeObject;

                if (player.isLocalPlayer)
                {
                    player.UpdateClient(dt);
                }
            }
        }

        if (!isClientOnly) { return; }

        ClientCalculateCorrection(netcodeObjects, dt);
    }

    private void ClientCalculateCorrection(NetcodeObject[] netcodeObjects, float dt)
    {
        // Generate a mapping netId -> netcodeObject
        Dictionary<uint, NetcodeObject> netIdToNetcodeObject = new Dictionary<uint, NetcodeObject>();
        foreach (NetcodeObject netcodeObject in netcodeObjects)
        {
            netIdToNetcodeObject.Add(netcodeObject.netId, netcodeObject);
        }

        // If there are available messages
        if (client_state_msgs.Count > 0)
        {

            GlobalStateMessage state_msg = this.client_state_msgs.Dequeue();
            while (client_state_msgs.Count > 0) // make sure if there are any newer state messages available, we use those instead
            {
                state_msg = this.client_state_msgs.Dequeue();
            }

            NetcodePlayer.LocalPlayer.UpdateClientLastReceivedTick(state_msg);
            uint buffer_slot = NetcodePlayer.LocalPlayer.ClientLastRecievedTick % c_client_buffer_size;
            Dictionary<uint, State> netIdToState = new Dictionary<uint, State>();
            List<Vector2> position_errors = new List<Vector2>();
            List<float> rotation_errors = new List<float>();

            float playerPositionError = 0f;
            foreach (StateMessage state in state_msg.states)
            {
                netIdToState.Add(state.state.netId, state.state);
                if(!netIdToNetcodeObject.ContainsKey(state.state.netId))
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

            if (doCorrection)
            {
                //var offending = position_errors.Select((error, index) => new { error, index }).Where(error => error.error.sqrMagnitude > positionCorrectionMargin);
                //foreach (var offend in offending)
                //{
                //    Debug.Log(netIdToNetcodeObject[state_msg.states[offend.index].netId] + ": " + offend.error.sqrMagnitude);
                //}

                PostionErrorMetric.Sample(playerPositionError);
                last_correction_tick = (int)NetcodePlayer.LocalPlayer.client_tick_number;

                ClientPerformCorrection(netcodeObjects,
                                        buffer_slot,
                                        netIdToState,
                                        dt);
            }
            else
            {
                //Debug.Log("Not correcting");

            }

            // Smoothing
            foreach (NetcodeObject netcodeObject in netcodeObjects)
            {
                // TODO: Turn on smoothing
                //netcodeObject.client_error.position *= 0.9f;
                //netcodeObject.client_error.rotation *= 0.9f;

                // TODO: Use seperate object for the smoothed position
                //this.smoothed_client_player.transform.position = client_rigidbody.position + this.client_pos_error;
                //this.smoothed_client_player.transform.rotation = client_rigidbody.rotation * this.client_rot_error;

                //Rigidbody2D netcodeRigidbody2D = netcodeObject.GetComponent<Rigidbody2D>();

                //netcodeRigidbody2D.position = netcodeRigidbody2D.position + netcodeObject.client_error.position;
                //netcodeRigidbody2D.rotation = netcodeRigidbody2D.rotation + netcodeObject.client_error.rotation;
            }

        }

        PostionCorrectionTick.Sample(last_correction_tick);
    }

    private static void ClientPerformCorrection(NetcodeObject[] netcodeObjects, uint last_recieved_buffer_slot, Dictionary<uint, State> netIdToState, float dt)
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
            uint buffer_slot = rewind_buffer_slot % c_client_buffer_size;

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
                    PrePhysicsStep(netcodePlayer, netcodePlayer.client_input_buffer[buffer_slot]);
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

            if(netcodeObject.isLocalPlayer)
            {
                PostionCorrectionMetric.Sample((prev_pos - netcodeRigidbody2D.position).magnitude);
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

    #endregion

    #region Server
    private void UpdateServer(float dt)
    {
        //uint server_tick_number = this.server_tick_number;
        //uint server_tick_accumulator = this.server_tick_accumulator;

        NetcodeObject[] netcodeObjects = FindObjectsOfType<NetcodeObject>().Where(netObj => !(netObj is NetcodePlayer) || ((NetcodePlayer)netObj).server_input_buffer != null).ToArray();
        NetcodePlayer[] netcodePlayers = FindObjectsOfType<NetcodePlayer>().Where(netObj => netObj.server_input_buffer != null).ToArray();
        ServerUpdateInputBuffer(netcodePlayers);

        server_timer += Time.deltaTime;
        while (server_timer >= dt)
        {
            server_timer -= dt;
            ServerProgress(netcodePlayers, dt);
        }

        ServerSendState(netcodeObjects);
    }

    private void ServerUpdateInputBuffer(NetcodePlayer[] netcodePlayers)
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
                uint lastRecievedClientTick = netcodePlayer.server_input_buffer.Ready ? netcodePlayer.server_input_buffer.LastRecieved().clientTick: 0;
                if (max_tick >= lastRecievedClientTick)
                {
                    // there may be some inputs in the array that we've already had,
                    // so figure out where to start
                    uint start_i = lastRecievedClientTick > input_msg.start_tick_number ? (lastRecievedClientTick - input_msg.start_tick_number) : 0;

                    // run through all relevant inputs, and step player forward
                    for (int i = (int)start_i; i < input_msg.inputs.Count; ++i)
                    {
                        InputPacket<Inputs> inputPacket;
                        inputPacket.input = input_msg.inputs[i];
                        inputPacket.clientTick = input_msg.start_tick_number + (uint)i;
                        inputPacket.serverTick = 0;
                        netcodePlayer.server_input_buffer.Enqueue(inputPacket);
                    }
                }
            }

            SamplePlayerTick(netcodePlayer);
        }
    }

    private static void SamplePlayerTick(NetcodePlayer netcodePlayer)
    {
        ProfilerCounter<uint> playerTickMonitor;
        var playerTickMonitorIndex = NetIdToPlayerTickCounter.FindIndex(tickMonitor => tickMonitor.Item1 == netcodePlayer.netId);
        if (playerTickMonitorIndex == -1)
        {
            if (NetIdToPlayerTickCounter.Count >= PlayerTickCounters.Length)
            {
                // TODO: Handle the case that a player disconnected and then re-join with a new? netId
                Debug.LogWarning($"Cannot track player {netcodePlayer.netId}, monitor list is full");
            }

            playerTickMonitor = PlayerTickCounters[NetIdToPlayerTickCounter.Count];
            NetIdToPlayerTickCounter.Add((netcodePlayer.netId, playerTickMonitor));
        }
        else
        {
            playerTickMonitor = NetIdToPlayerTickCounter[playerTickMonitorIndex].Item2;
        }

        if(netcodePlayer.server_input_buffer != null && netcodePlayer.server_input_buffer.Ready)
        {
            playerTickMonitor.Sample(netcodePlayer.server_input_buffer.LastRecieved().clientTick % NetcodeManager.c_client_buffer_size);
        }
    }

    private void ServerProgress(NetcodePlayer[] netcodePlayers, float dt)
    {
        desiredServerTickNumber++;
        if(desiredServerTickNumber - server_tick_number < serverInputBuffer)
        {
            return;
        }

        for (int i = (int)server_tick_number; i < desiredServerTickNumber; ++i)
        {

            foreach (NetcodePlayer netcodePlayer in netcodePlayers)
            {
                if (netcodePlayer.server_input_buffer.Count > 0)
                {
                    PrePhysicsStep(netcodePlayer, netcodePlayer.server_input_buffer.Dequeue(server_tick_number).input);
                }
            }

            //Debug.Log("Running physics");
            Physics.Simulate(dt);
            server_tick_number++;
        }

        // TODO: Seperate server display
        //this.server_display_player.transform.position = server_rigidbody.position;
        //this.server_display_player.transform.rotation = server_rigidbody.rotation;
        
    }

    public void ServerSendState(NetcodeObject[] netcodeObjects)
    {

        if(netcodeObjects.Length == 0)
        {
            return;
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
            stateMessage.serverTick = server_tick_number;
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
                } else
                {
                    // TODO: Send to individual players
                    return;
                }
            } else
            // Ignored if not a player
            {
                stateMessage.lastProcessedClientTick = 0;
                stateMessage.lastProcessedServerTick = 0;
            }
            state_msg.states[i] = stateMessage;

        }

        RpcQueueClientState(state_msg);
    }

    [ClientRpc]
    void RpcQueueClientState(GlobalStateMessage state_msg)
    {
        //Debug.Log($"Received state {state_msg.tick_number}");
        client_state_msgs?.Enqueue(state_msg);
    }

    #endregion

    public static void PrePhysicsStep(NetcodePlayer player, Inputs inputs)
    {
        player.GetComponent<PlayerPlanetController>().Move(inputs.movement);
    }
}
