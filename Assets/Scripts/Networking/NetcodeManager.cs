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
    public struct StateMessage
    {
        public uint tick_number;
        public float time;
        public State[] states;
    }

    #endregion

    #region Members

    // client specific
    public static readonly uint c_client_buffer_size = 1024;

    public static float client_timer;
    public static uint client_tick_number;
    public static uint client_last_received_state_tick;
    private Queue<StateMessage> client_state_msgs;
    private bool clientStarted = false;

    int last_correction_tick;

    // server specific
    public uint server_snapshot_rate; // Set by user (default 1)
    [SerializeField]

    // TODO: Handle state syncing in batching instead of min input
    // Override the client_tick_number if it is less than the state tick
    //[SyncVar]
    public uint server_tick_number;
    public static float server_timer;
    private uint server_tick_accumulator;
    private uint desiredServerTickNumber;
    public static readonly uint serverInputBuffer = 64;

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
    #endregion

    #region Start and Update
    private void Start()
    {
        client_timer = 0;
        client_last_received_state_tick = 0;
        client_state_msgs = new Queue<StateMessage>();

        server_tick_accumulator = 0;

        server_tick_number = 0;
        client_tick_number = 0;
    }

    private void Update()
    {
        float dt = Time.fixedDeltaTime;
        if (isClient)
        {
            UpdateClient(dt);
        }

        if (isServer)
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
        ClientTickCounter.Sample(client_tick_number % NetcodeManager.c_client_buffer_size);
        ClientLastReceivedTickCounter.Sample(client_last_received_state_tick % NetcodeManager.c_client_buffer_size);
    }

    #region Client


    private void UpdateClient(float dt)
    {
        NetcodeObject[] netcodeObjects = FindObjectsOfType<NetcodeObject>();

        // Do not send input messages until the server has sent the first state
        if (clientStarted)
        {
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
        }

        Dictionary<uint, NetcodeObject> netIdToNetcodeObject = new Dictionary<uint, NetcodeObject>();

        foreach (NetcodeObject netcodeObject in netcodeObjects)
        {
            netIdToNetcodeObject.Add(netcodeObject.netId, netcodeObject);
        }


        if (client_state_msgs.Count > 0)
        {

            StateMessage state_msg = this.client_state_msgs.Dequeue();
            while (client_state_msgs.Count > 0) // make sure if there are any newer state messages available, we use those instead
            {
                state_msg = this.client_state_msgs.Dequeue();
            }

            if(!clientStarted)
            {
                client_tick_number = state_msg.tick_number + (uint)((Time.time - state_msg.time) * 2 * dt);
                client_timer = dt;
                clientStarted = true;
            }

            Dictionary<uint, State> netIdToState = new Dictionary<uint, State>();

            client_last_received_state_tick = state_msg.tick_number;

            uint buffer_slot = state_msg.tick_number % c_client_buffer_size;

            // TODO: Smartly correct instead of correction on any
            List<Vector2> position_errors = new List<Vector2>();
            List<float> rotation_errors = new List<float>();

            float playerPositionError = 0f;
            foreach (State state in state_msg.states)
            {
                netIdToState.Add(state.netId, state);
                NetcodeObject currentObject = netIdToNetcodeObject[state.netId];
                //Debug.Log(currentObject.gameObject.name);

                Vector2 position_error = state.state.position - currentObject.client_state_buffer[buffer_slot].position;

                if(currentObject.isLocalPlayer)
                {
                    playerPositionError = position_error.magnitude;
                }

                //Debug.Log(position_error);
                position_errors.Add(position_error);
                float rotation_error = state.state.rotation - currentObject.client_state_buffer[buffer_slot].rotation;
                rotation_errors.Add(rotation_error);
            }

            const float positionCorrectionMargin = 0.01f;

            bool doCorrection = (position_errors.Any(error => error.sqrMagnitude > positionCorrectionMargin)); //||
                                                                                                               //rotation_errors.Any(error => Mathf.Abs(error) > 0.00001f));

            // Perform correction on a client only
            if (!isClientOnly) { return; }

            if (doCorrection)
            {
                //var offending = position_errors.Select((error, index) => new { error, index }).Where(error => error.error.sqrMagnitude > positionCorrectionMargin);
                //foreach (var offend in offending)
                //{
                //    Debug.Log(netIdToNetcodeObject[state_msg.states[offend.index].netId] + ": " + offend.error.sqrMagnitude);
                //}

                PostionErrorMetric.Sample(playerPositionError);
                last_correction_tick = (int)client_tick_number;

                ClientPerformCorrection(netcodeObjects,
                                        state_msg,
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

    private static void ClientPerformCorrection(NetcodeObject[] netcodeObjects, StateMessage state_msg, Dictionary<uint, State> netIdToState, float dt)
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

        uint rewind_tick_number = state_msg.tick_number;
        while (rewind_tick_number < client_tick_number)
        {
            uint buffer_slot = rewind_tick_number % c_client_buffer_size;

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

            ++rewind_tick_number;
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

        NetcodePlayer[] netcodePlayers = FindObjectsOfType<NetcodePlayer>();
        ServerUpdateInputBuffer(netcodePlayers);

        server_timer += Time.deltaTime;
        while (server_timer >= dt)
        {
            server_timer -= dt;
            ServerProgress(netcodePlayers, dt);
        }

        if (server_tick_accumulator >= this.server_snapshot_rate)
        {
            ServerSendState();
        }
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
                if (max_tick >= netcodePlayer.server_tick_number)
                {
                    // there may be some inputs in the array that we've already had,
                    // so figure out where to start
                    uint start_i = netcodePlayer.server_tick_number > input_msg.start_tick_number ? (netcodePlayer.server_tick_number - input_msg.start_tick_number) : 0;

                    // run through all relevant inputs, and step player forward
                    for (int i = (int)start_i; i < input_msg.inputs.Count; ++i)
                    {
                        InputAck inputAck;
                        inputAck.received = true;
                        inputAck.inputs = input_msg.inputs[i];
                        netcodePlayer.server_input_buffer[netcodePlayer.server_tick_number % serverInputBuffer] = inputAck;
                        ++netcodePlayer.server_tick_number;
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

        playerTickMonitor.Sample(netcodePlayer.server_tick_number % NetcodeManager.c_client_buffer_size);
    }

    private void ServerProgress(NetcodePlayer[] netcodePlayers, float dt)
    {
        // TODO: Find a smarter way to progress the server
        if (netcodePlayers.Length > 0)
        {
            desiredServerTickNumber++;
            uint lowest_player_server_tick_number = netcodePlayers.Min(player => player.server_tick_number);

            uint missingInput = desiredServerTickNumber - lowest_player_server_tick_number;
            if (missingInput <= serverInputBuffer && missingInput > 0)
            {
                return;
            }
        }

        // All clients are ready to progress ->  lowest_player_server_tick_number > server_tick_number
        // Not all clients are ready to progress -> lowest_player_server_tick_number <= server_tick_number
        // 
        // WRONG: I don't care, skip the input -> lowest_player_server_tick_number + serverInputBuffer <= server_tick_number

        // Need a DESIRED server_tick_number based on the number of physics frames that have passed
        // If any client is behind the desired by less than serverInputBuffer, just skip the update


        // TODO: Switch to a fixed progression rate
        // TODO: If we missed an input, wait a frame to see if it comes in

        for (int i = (int)server_tick_number; i < desiredServerTickNumber; ++i)
        {

            foreach (NetcodePlayer netcodePlayer in netcodePlayers)
            {
                InputAck inputAck = netcodePlayer.server_input_buffer[i % serverInputBuffer];
                //if(inputAck.received)
                //{
                    NetcodeManager.PrePhysicsStep(netcodePlayer, inputAck.inputs);
                //}
                netcodePlayer.server_tick_number = (uint)i;
            }

            //Debug.Log("Running physics");
            Physics.Simulate(dt);
            ++server_tick_accumulator;
            server_tick_number++;
        }

        // TODO: Seperate server display
        //this.server_display_player.transform.position = server_rigidbody.position;
        //this.server_display_player.transform.rotation = server_rigidbody.rotation;
        
    }

    public void ServerSendState()
    {
        server_tick_accumulator = 0;

        StateMessage state_msg;
        state_msg.tick_number = server_tick_number;
        state_msg.time = Time.time;
        NetcodeObject[] netcodeObjects = FindObjectsOfType<NetcodeObject>();
        state_msg.states = new State[netcodeObjects.Length];

        for (int i = 0; i < netcodeObjects.Length; i++)
        {
            Rigidbody2D rigidbody = netcodeObjects[i].GetComponent<Rigidbody2D>();
            state_msg.states[i].netId = netcodeObjects[i].netId;
            state_msg.states[i].state.position = rigidbody.position;
            state_msg.states[i].state.rotation = rigidbody.rotation;
            state_msg.states[i].state.velocity = rigidbody.velocity;
            state_msg.states[i].state.angularVelocity = rigidbody.angularVelocity;
        }

        this.RpcQueueClientState(state_msg);
    }

    [ClientRpc]
    void RpcQueueClientState(StateMessage state_msg)
    {
        //Debug.Log($"Received state {state_msg.tick_number}");
        client_state_msgs.Enqueue(state_msg);
    }

    #endregion

    public static void PrePhysicsStep(NetcodePlayer player, Inputs inputs)
    {
        player.GetComponent<PlayerPlanetController>().Move(inputs.movement);
    }
}
