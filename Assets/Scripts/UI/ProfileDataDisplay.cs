using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Profiling;
using System.Text;

public class ProfileDataDisplay : MonoBehaviour
{
    public TextMeshProUGUI statsText;

    ProfilerRecorder serverTickRecorder;
    ProfilerRecorder clientTickRecorder;
    ProfilerRecorder playerRecorder1;
    ProfilerRecorder playerRecorder2;
    ProfilerRecorder[] playerRecorders;

    void OnEnable()
    {
        serverTickRecorder = ProfilerRecorder.StartNew(NetcodeManager.NetcodeCategory, "Server Tick");
        clientTickRecorder = ProfilerRecorder.StartNew(NetcodeManager.NetcodeCategory, "Client Tick");
        playerRecorder1 = ProfilerRecorder.StartNew(NetcodeManager.NetcodeCategory, "Player 1 Tick");
        playerRecorder2 = ProfilerRecorder.StartNew(NetcodeManager.NetcodeCategory, "Player 2 Tick");
        playerRecorders = new ProfilerRecorder[] { playerRecorder1, playerRecorder2 };
    }

    void OnDisable()
    {
        serverTickRecorder.Dispose();
        clientTickRecorder.Dispose();
        playerRecorder1.Dispose();
        playerRecorder2.Dispose();
    }

    // Update is called once per frame
    void Update()
    {
        var stringBuilder = new StringBuilder(500);
        stringBuilder.AppendLine($"Server Tick: {serverTickRecorder.LastValue}");
        stringBuilder.AppendLine($"Client Tick: {clientTickRecorder.LastValue}");

        for(int index = 0; index < NetcodeManager.NetIdToPlayerTickCounter.Count; ++index)
        {
            var playerCounter = NetcodeManager.NetIdToPlayerTickCounter[index];
            stringBuilder.AppendLine($"Player {playerCounter.Item1} Tick: {playerRecorders[index].LastValue}");
        }

        statsText.text = stringBuilder.ToString();
    }
}
