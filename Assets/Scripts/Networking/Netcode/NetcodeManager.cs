using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
using System.Linq;
using TMPro;
using Unity.Profiling;
using NetcodeData;

// TODO: Convert sever logic to non-mono class

public class NetcodeManager : NetworkBehaviour
{
    #region Structs

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
        if (isClientOnly)
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
        // Generate a mapping netId -> netcodeObject
        Dictionary<uint, NetcodeObject> netIdToNetcodeObject = new Dictionary<uint, NetcodeObject>();
        foreach (NetcodeObject netcodeObject in netcodeObjects)
        {
            netIdToNetcodeObject.Add(netcodeObject.netId, netcodeObject);
        }

        Dictionary<uint, State> netIdToState;

        bool doCorrection = NetcodeClientSystem.ClientCalculateCorrection(netIdToNetcodeObject,
                                  client_state_msgs,
                                  c_client_buffer_size,
                                  dt,
                                  out netIdToState);

        if(doCorrection)
        {
            NetcodeClientSystem.ClientPerformCorrection(netcodeObjects,
                                    NetcodePlayer.LocalPlayer.ClientLastRecievedTick % c_client_buffer_size,
                                    netIdToState,
                                    c_client_buffer_size,
                                    dt);
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
        NetcodeServerSystem.ServerUpdateInputBuffer(netcodePlayers);

        server_timer += Time.deltaTime;
        while (server_timer >= dt)
        {
            server_timer -= dt;
            if (NetcodeServerSystem.ServerDesireProgression(server_tick_number, serverInputBuffer, ref desiredServerTickNumber))
            {
                server_tick_number += NetcodeServerSystem.ServerProgress(netcodePlayers, server_tick_number, desiredServerTickNumber, dt);
            }
            
        }

        GlobalStateMessage? stateMessage = NetcodeServerSystem.ServerSendState(netcodeObjects, server_tick_number);
        if(stateMessage != null)
        {
            RpcQueueClientState((GlobalStateMessage)stateMessage);
        }
    }

    // TODO: Do this in the NetcodePlayer script
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

        if(netcodePlayer.server_input_buffer != null && netcodePlayer.server_input_buffer.BeenProcessed)
        {
            playerTickMonitor.Sample(netcodePlayer.server_input_buffer.LastRecieved().clientTick % NetcodeManager.c_client_buffer_size);
        }
    }


    [ClientRpc]
    void RpcQueueClientState(GlobalStateMessage state_msg)
    {
        //Debug.Log($"Received state {state_msg.tick_number}");
        client_state_msgs?.Enqueue(state_msg);
    }

    #endregion


}
