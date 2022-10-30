using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using ClientServerPrediction;
using MockModel;


public class TestIntegrationClientState
{
    /// <summary>
    /// GIVEN: Valid Player, ClientState
    /// WHEN: Tick() is called
    /// THEN: Input message only contains 1 input, next tick contains 2
    /// </summary>
    [Test]
    public void TestTick()
    {
        // Consts
        uint mockNetId = 10;

        MockPlayer mockPlayer = new MockPlayer();
        MockRunner mockRunner = new MockRunner();
        RunContext mockRunContext = new RunContext();

        ClientState client = new ClientState();
        client.AddStateful(mockPlayer, mockNetId);
        client.AddInputful(mockPlayer, mockNetId, true);

        InputMessage inputMessage = client.Tick(mockRunner, mockRunContext);

        Assert.AreEqual(inputMessage.GetMap()[mockNetId].inputs.Count, 1);

        inputMessage = client.Tick(mockRunner, mockRunContext);

        Assert.AreEqual(inputMessage.GetMap()[mockNetId].inputs.Count, 2);
    }

    /// <summary>
    /// GIVEN: Valid Player, ClientState
    /// WHEN: Tick() is called, Message is added to stateMessageQueue
    /// THEN: Input message only contains 1 input, next tick contains 1
    /// </summary>
    [Test]
    public void TestTickReceived()
    {
        // Consts
        uint mockNetId = 10;
        uint mockServerTick = 10;

        MockPlayer mockPlayer = new MockPlayer();
        MockRunner mockRunner = new MockRunner();
        RunContext mockRunContext = new RunContext();

        ClientState client = new ClientState();
        client.AddStateful(mockPlayer, mockNetId);
        client.AddInputful(mockPlayer, mockNetId, true);

        // Inputs for tick 1
        // State for tick 2
        InputMessage inputMessage = client.Tick(mockRunner, mockRunContext);

        Assert.AreEqual(inputMessage.GetMap()[mockNetId].inputs.Count, 1);

        StateMessage stateMessage = new StateMessage();

        // State for tick 2
        stateMessage.serverTick = mockServerTick + 1;
        stateMessage.stateContexts = new List<StateContext>();

        stateMessage.stateContexts.Add(new StateContext { netId = mockNetId,
                                                          state = mockPlayer.GetState(),
                                                          // Inputs for tick 1
                                                          tickSync = new TickSync { lastProcessedClientTick = client.tick - 1, lastProcessedServerTick = mockServerTick } });
        client.stateMessageQueue.Enqueue(stateMessage);

        inputMessage = client.Tick(mockRunner, mockRunContext);

        Assert.AreEqual(inputMessage.GetMap()[mockNetId].inputs.Count, 1);
    }
}
