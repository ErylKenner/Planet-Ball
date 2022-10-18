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
    /// GIVEN: Valid State buffer map, valid Stateful map, buffer slot larger than the buffer
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

    #region StoreInput
    /// <summary>
    /// GIVEN: Valid Input buffer map, valid Inputful map, valid buffer slot
    /// WHEN: StoreInput() is called
    /// THEN: Input is stored at the buffer slot
    /// </summary>
    [Test]
    public void TestStoreInput()
    {
        uint mockBufferSize = 64;
        uint mockNetId = 10;
        uint mockBufferSlot = 15;

        Inputs[] inputBuffer = new Inputs[mockBufferSize];
        Dictionary<uint, Inputs[]> inputBufferMap = new Dictionary<uint, Inputs[]>();
        inputBufferMap.Add(mockNetId, inputBuffer);

        MockPlayer mockPlayer = new MockPlayer();
        Vector2 mockInput = mockPlayer.GetInput().movement;

        Dictionary<uint, IInputful> inputMap = new Dictionary<uint, IInputful>();
        inputMap.Add(mockNetId, mockPlayer);
        ClientStateMachine.StoreInput(ref inputBufferMap, in inputMap, mockBufferSlot);

        Assert.AreEqual(inputBufferMap[mockNetId][mockBufferSlot].movement, mockInput);

    }

    /// <summary>
    /// GIVEN: Valid Input buffer map, empty Inputful map, valid buffer slot
    /// WHEN: StoreInput() is called
    /// THEN: No input stored at the buffer slot
    /// </summary>
    [Test]
    public void TestStoreInputNoPlayer()
    {
        uint mockBufferSize = 64;
        uint mockNetId = 10;
        uint mockBufferSlot = 15;

        Inputs[] inputBuffer = new Inputs[mockBufferSize];
        Dictionary<uint, Inputs[]> inputBufferMap = new Dictionary<uint, Inputs[]>();
        inputBufferMap.Add(mockNetId, inputBuffer);

        Dictionary<uint, IInputful> inputMap = new Dictionary<uint, IInputful>();
        ClientStateMachine.StoreInput(ref inputBufferMap, in inputMap, mockBufferSlot);

        Assert.AreEqual(inputBufferMap[mockNetId][mockBufferSlot], null);

    }

    /// <summary>
    /// GIVEN: Empty Input buffer map, valid Inputful map, valid buffer slot
    /// WHEN: StoreInput() is called
    /// THEN: Nothing happens
    /// </summary>
    [Test]
    public void TestStoreInputNoBuffer()
    {
        uint mockNetId = 10;
        uint mockBufferSlot = 15;

        Dictionary<uint, Inputs[]> inputBufferMap = new Dictionary<uint, Inputs[]>();

        MockPlayer mockPlayer = new MockPlayer();
        Vector2 mockInput = mockPlayer.GetInput().movement;

        Dictionary<uint, IInputful> inputMap = new Dictionary<uint, IInputful>();
        inputMap.Add(mockNetId, mockPlayer);
        ClientStateMachine.StoreInput(ref inputBufferMap, in inputMap, mockBufferSlot);

        Assert.False(inputBufferMap.ContainsKey(mockNetId));

    }

    /// <summary>
    /// GIVEN: Valid Input buffer map, valid Inputful map, buffer slot larger than the buffer
    /// WHEN: StoreInput() is called
    /// THEN: Throws IndexOutOfRangeException
    /// </summary>
    [Test]
    public void TestStoreInputSlotOutOfRange()
    {
        uint mockBufferSize = 64;
        uint mockNetId = 10;
        uint mockBufferSlot = 70;

        Inputs[] inputBuffer = new Inputs[mockBufferSize];
        Dictionary<uint, Inputs[]> inputBufferMap = new Dictionary<uint, Inputs[]>();
        inputBufferMap.Add(mockNetId, inputBuffer);

        MockPlayer mockPlayer = new MockPlayer();

        Dictionary<uint, IInputful> inputMap = new Dictionary<uint, IInputful>();
        inputMap.Add(mockNetId, mockPlayer);

        Assert.Throws<System.IndexOutOfRangeException>(() =>
            ClientStateMachine.StoreInput(ref inputBufferMap, in inputMap, mockBufferSlot)
        );

    }
    #endregion

    #region CreateInputMessage

    /// <summary>
    /// GIVEN: Valid Input buffer map, valid last recieved tick, valid client tick (last recieved + 1)
    /// WHEN: CreateInputMessage() is called
    /// THEN: Returns input message with one player and one input, start tick is one after last recieved
    /// </summary>
    [Test]
    public void TestCreateInputMessage()
    {
        uint mockNetId = 10;
        uint mockBufferSize = 64;
        uint mockLastRecievedTick = 15;
        uint mockClientTick = 16;
        Vector2 mockMovement = Vector2.up;
        Dictionary<uint, Inputs[]> inputBufferMap = new Dictionary<uint, Inputs[]>();

        Inputs[] inputBuffer = new Inputs[mockBufferSize];
        inputBuffer[mockClientTick] = new Inputs { movement = mockMovement };
        inputBufferMap.Add(mockNetId, inputBuffer);

        InputMessage inputMessage = ClientStateMachine.CreateInputMessage(in inputBufferMap, mockLastRecievedTick, mockClientTick);
        Dictionary<uint, InputContext> inputMessageMap = inputMessage.GetMap();

        Assert.AreEqual(inputMessage.startTick, mockLastRecievedTick + 1);
        Assert.AreEqual(inputMessage.inputContexts.Count, 1);
        Assert.AreEqual(inputMessage.inputContexts[0].inputs.Count, 1);
        Assert.AreEqual(inputMessageMap[mockNetId].inputs[0].movement, mockMovement);
    }

    /// <summary>
    /// GIVEN: Valid Input buffer map, valid last recieved tick, valid client tick - goes around buffer
    /// WHEN: CreateInputMessage() is called
    /// THEN: Returns input message with one player and two inputs, handled fine
    /// </summary>
    [Test]
    public void TestCreateInputMessageBufferWrap()
    {
        uint mockNetId = 10;
        uint mockBufferSize = 16;
        uint mockLastRecievedTick = 15;
        uint mockClientTick = 17;
        Vector2 mockMovement = Vector2.up;
        Dictionary<uint, Inputs[]> inputBufferMap = new Dictionary<uint, Inputs[]>();

        Inputs[] inputBuffer = new Inputs[mockBufferSize];
        inputBuffer[(mockClientTick - 1) % inputBuffer.Length] = new Inputs { movement = mockMovement };
        inputBuffer[mockClientTick % inputBuffer.Length] = new Inputs { movement = mockMovement };
        inputBufferMap.Add(mockNetId, inputBuffer);

        InputMessage inputMessage = ClientStateMachine.CreateInputMessage(in inputBufferMap, mockLastRecievedTick, mockClientTick);
        Dictionary<uint, InputContext> inputMessageMap = inputMessage.GetMap();

        Assert.AreEqual(inputMessage.startTick, mockLastRecievedTick + 1);
        Assert.AreEqual(inputMessage.inputContexts.Count, 1);
        Assert.AreEqual(inputMessage.inputContexts[0].inputs.Count, 2);
        Assert.AreEqual(inputMessageMap[mockNetId].inputs[0].movement, mockMovement);
        Assert.AreEqual(inputMessageMap[mockNetId].inputs[1].movement, mockMovement);
    }

    #endregion
}
