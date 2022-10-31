//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using TMPro;
//using Unity.Profiling;
//using System.Text;
//using System.Linq;

//public class ProfileDataDisplay : MonoBehaviour
//{
//    public TextMeshProUGUI statsText;

//    ProfilerRecorder serverTickRecorder;
//    ProfilerRecorder serverDesiredTickRecorder;
//    ProfilerRecorder clientTickRecorder;
//    ProfilerRecorder clientTickLastRecRecorder;
//    ProfilerRecorder playerRecorder1;
//    ProfilerRecorder playerRecorder2;
//    ProfilerRecorder[] playerRecorders;
//    ProfilerRecorder positionErrorRecorder;
//    ProfilerRecorder positionCorrectionRecorder;
//    ProfilerRecorder positionCorrectionTickRecorder;


//    double previousPositionError = 0;
//    double previousPositionCorrection = 0;

//    const int frameBufferSize = 256;
//    int previousCorrectionTick = 0;
//    bool[] correctionBuffer;


//    void OnEnable()
//    {
//        correctionBuffer = new bool[frameBufferSize];

//        serverTickRecorder = ProfilerRecorder.StartNew(NetcodeManager.NetcodeCategory, "Server Tick");
//        serverDesiredTickRecorder = ProfilerRecorder.StartNew(NetcodeManager.NetcodeCategory, "Server Desired Tick");
//        clientTickRecorder = ProfilerRecorder.StartNew(NetcodeManager.NetcodeCategory, "Client Tick");
//        clientTickLastRecRecorder = ProfilerRecorder.StartNew(NetcodeManager.NetcodeCategory, "Client Last Received Tick");
//        playerRecorder1 = ProfilerRecorder.StartNew(NetcodeManager.NetcodeCategory, "Player 1 Tick");
//        playerRecorder2 = ProfilerRecorder.StartNew(NetcodeManager.NetcodeCategory, "Player 2 Tick");
//        playerRecorders = new ProfilerRecorder[] { playerRecorder1, playerRecorder2 };

//        positionErrorRecorder = ProfilerRecorder.StartNew(NetcodeManager.NetcodeCategory, "Position Error");
//        positionCorrectionRecorder = ProfilerRecorder.StartNew(NetcodeManager.NetcodeCategory, "Position Correction");
//        positionCorrectionTickRecorder = ProfilerRecorder.StartNew(NetcodeManager.NetcodeCategory, "Position Correction Tick");
//    }

//    void OnDisable()
//    {
//        serverTickRecorder.Dispose();
//        serverDesiredTickRecorder.Dispose();
//        clientTickRecorder.Dispose();
//        clientTickLastRecRecorder.Dispose();
//        playerRecorder1.Dispose();
//        playerRecorder2.Dispose();
//        positionErrorRecorder.Dispose();
//        positionCorrectionRecorder.Dispose();
//        positionCorrectionTickRecorder.Dispose();
//    }

//    //double GetRecorderAverage(ProfilerRecorder recorder, int frames=60)
//    //{
//    //    List<ProfilerRecorderSample> samples = new List<ProfilerRecorderSample>(frames);
//    //    recorder.CopyTo(samples);

//    //    Debug.Log(string.Join(' ', samples));

//    //    List<long> nonZeroSamples = samples.Select(sample => sample.Value).Where(sample => sample != 0).ToList();
//    //    Debug.Log(string.Join(' ', nonZeroSamples));

//    //    if (nonZeroSamples.Count == 0)
//    //    {
//    //        return 0;
//    //    }

//    //    return nonZeroSamples.Average();
//    //}

//    float GetCorrectionPercentage()
//    {
//        if(!NetcodePlayer.LocalPlayer)
//        {
//            return 0f;
//        }

//        if (positionCorrectionTickRecorder.LastValue != 0)
//        {
//            for (; previousCorrectionTick < positionCorrectionTickRecorder.LastValue;)
//            {
//                previousCorrectionTick++;
//                int bufferSlot = previousCorrectionTick % frameBufferSize;
//                correctionBuffer[bufferSlot] = false;
//            }

//            correctionBuffer[previousCorrectionTick % frameBufferSize] = true;
//        }
        
//        // Mutliple frames per tick - always set to true if true comes in
//        // Multiple cycles per tick - reset the tick to false


//        int corrections = 0;
//        int total = 0;
//        for(; total < frameBufferSize && total < (int)NetcodePlayer.LocalPlayer.client_tick_number; total++)
//        {
//            if(correctionBuffer[total])
//            {
//                corrections++;
//            }
//        }

//        if(total == 0)
//        {
//            return 0;
//        }

//        return (float)corrections / total;


//    }

//    void SetPositionMetrics()
//    {
//        double positionError = positionErrorRecorder.LastValueAsDouble;
//        if(positionError != 0)
//        {
//            previousPositionError = positionError;
//        }

//        double positionCorrection = positionCorrectionRecorder.LastValueAsDouble;
//        if (positionCorrection != 0)
//        {
//            previousPositionCorrection = positionCorrection;
//        }
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        var stringBuilder = new StringBuilder(500);
//        stringBuilder.AppendLine($"Server Tick: {serverTickRecorder.LastValue}");
//        stringBuilder.AppendLine($"Server Desired Tick: {serverDesiredTickRecorder.LastValue}");
//        stringBuilder.AppendLine($"Client Tick: {clientTickRecorder.LastValue}");
//        stringBuilder.AppendLine($"Client Last Rec Tick: {clientTickLastRecRecorder.LastValue}");


//        for(int index = 0; index < NetcodeManager.NetIdToPlayerTickCounter.Count; ++index)
//        {
//            var playerCounter = NetcodeManager.NetIdToPlayerTickCounter[index];
//            stringBuilder.AppendLine($"Player {playerCounter.Item1} Tick: {playerRecorders[index].LastValue}");
//        }

//        stringBuilder.AppendLine($"Last Position Correction Tick: {positionCorrectionTickRecorder.LastValue % NetcodeManager.c_client_buffer_size}");

//        float correctionPercentage = GetCorrectionPercentage();
//        float ticksPerCorrection = correctionPercentage != 0 ? 1 / correctionPercentage : 0;
//        stringBuilder.AppendLine($"Correction Percentage: {correctionPercentage * 100:F1}%");
//        stringBuilder.AppendLine($"Average Ticks Per Correction: {ticksPerCorrection:F1}");

//        SetPositionMetrics();
//        stringBuilder.AppendLine($"Last Position Error: {previousPositionError:F1}");
//        stringBuilder.AppendLine($"Last Position Correction: {previousPositionCorrection:F1}");
        

//        statsText.text = stringBuilder.ToString();
//    }
//}
