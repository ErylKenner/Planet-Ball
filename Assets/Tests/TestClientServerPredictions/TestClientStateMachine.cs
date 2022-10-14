using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using ClientServerPrediction;
using MockModel;

public class TestClientStateMachine
{
    #region GetLatestStateMessage

    /// <summary>
    /// GIVEN: Empty StateMessage queue
    /// WHEN: GetLatestStateMessage() is called
    /// THEN: Returns null
    /// </summary>
    [Test]
    public void TestGetLatestStateMessageEmptyQueue()
    {
        Queue<StateMessage> stateMessages = new Queue<StateMessage>();

        StateMessage stateMessage = ClientStateMachine.GetLatestStateMessage(ref stateMessages);

        Assert.AreEqual(stateMessage, null);
    }

    /// <summary>
    /// GIVEN: StateMessage queue with mutliple messages
    /// WHEN: GetLatestStateMessage() is called
    /// THEN: Returns message with the greatest server tick, queue is empty
    /// </summary>
    [Test]
    public void TestGetLatestStateMessage()
    {
        uint firstMockServerTick = 30;
        uint secondMockServerTick = 50;
        Queue<StateMessage> stateMessages = new Queue<StateMessage>();
        StateMessage firstStateMessage = new StateMessage { serverTick = firstMockServerTick };
        StateMessage secondStateMessage = new StateMessage { serverTick = secondMockServerTick };

        stateMessages.Enqueue(secondStateMessage);
        stateMessages.Enqueue(firstStateMessage);


        StateMessage stateMessage = ClientStateMachine.GetLatestStateMessage(ref stateMessages);

        Assert.AreEqual(stateMessage, secondStateMessage);
        Assert.AreEqual(stateMessages.Count, 0);
    }

    #endregion

    #region CorrectClient

    #endregion


    #region StoreState
    /// <summary>
    /// GIVEN: Valid State buffer map, valid Stateful map, valid buffer slot
    /// WHEN: StoreState() is called
    /// THEN: State is stored at the buffer slot
    /// </summary>
    [Test]
    public void TestStoreState()
    {
        uint mockBufferSize = 64;
        uint mockNetId = 10;
        Vector2 mockPosition = Vector2.up;
        uint mockBufferSlot = 15;

        State[] stateBuffer = new State[mockBufferSize];
        Dictionary<uint, State[]> stateBufferMap = new Dictionary<uint, State[]>();
        stateBufferMap.Add(mockNetId, stateBuffer);

        MockPlayer mockPlayer = new MockPlayer();
        mockPlayer.SetState(new State { position = mockPosition });

        Dictionary<uint, IStateful> stateMap = new Dictionary<uint, IStateful>();
        stateMap.Add(mockNetId, mockPlayer);
        ClientStateMachine.StoreState(ref stateBufferMap, in stateMap, mockBufferSlot);

        Assert.AreEqual(stateBufferMap[mockNetId][mockBufferSlot].position, mockPosition);

    }

    /// <summary>
    /// GIVEN: Valid State buffer map, empty Stateful map, valid buffer slot
    /// WHEN: StoreState() is called
    /// THEN: No state stored at the buffer slot
    /// </summary>
    [Test]
    public void TestStoreStateNoPlayer()
    {
        uint mockBufferSize = 64;
        uint mockNetId = 10;
        uint mockBufferSlot = 15;

        State[] stateBuffer = new State[mockBufferSize];
        Dictionary<uint, State[]> stateBufferMap = new Dictionary<uint, State[]>();
        stateBufferMap.Add(mockNetId, stateBuffer);

        Dictionary<uint, IStateful> stateMap = new Dictionary<uint, IStateful>();
        ClientStateMachine.StoreState(ref stateBufferMap, in stateMap, mockBufferSlot);

        Assert.AreEqual(stateBufferMap[mockNetId][mockBufferSlot], null);

    }

    /// <summary>
    /// GIVEN: Empty State buffer map, valid Stateful map, valid buffer slot
    /// WHEN: StoreState() is called
    /// THEN: Nothing happens
    /// </summary>
    [Test]
    public void TestStoreStateNoBuffer()
    {
        uint mockNetId = 10;
        Vector2 mockPosition = Vector2.up;
        uint mockBufferSlot = 15;

        Dictionary<uint, State[]> stateBufferMap = new Dictionary<uint, State[]>();

        MockPlayer mockPlayer = new MockPlayer();
        mockPlayer.SetState(new State { position = mockPosition });

        Dictionary<uint, IStateful> stateMap = new Dictionary<uint, IStateful>();
        stateMap.Add(mockNetId, mockPlayer);
        ClientStateMachine.StoreState(ref stateBufferMap, in stateMap, mockBufferSlot);

        Assert.False(stateBufferMap.ContainsKey(mockNetId));

    }

    /// <summary>
    /// GIVEN: Valid State buffer map, valid Stateful map, buffer slot larger than the bugger
    /// WHEN: StoreState() is called
    /// THEN: Throws IndexOutOfRangeException
    /// </summary>
    [Test]
    public void TestStoreStateSlotOutOfRange()
    {
        uint mockBufferSize = 64;
        uint mockNetId = 10;
        Vector2 mockPosition = Vector2.up;
        uint mockBufferSlot = 70;

        State[] stateBuffer = new State[mockBufferSize];
        Dictionary<uint, State[]> stateBufferMap = new Dictionary<uint, State[]>();
        stateBufferMap.Add(mockNetId, stateBuffer);

        MockPlayer mockPlayer = new MockPlayer();
        mockPlayer.SetState(new State { position = mockPosition });

        Dictionary<uint, IStateful> stateMap = new Dictionary<uint, IStateful>();
        stateMap.Add(mockNetId, mockPlayer);

        Assert.Throws<System.IndexOutOfRangeException>(() => 
            ClientStateMachine.StoreState(ref stateBufferMap, in stateMap, mockBufferSlot)
        );

    }
    #endregion
}
