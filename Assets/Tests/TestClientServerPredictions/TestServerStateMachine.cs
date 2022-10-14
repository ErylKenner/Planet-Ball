using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using ClientServerPrediction;
using MockModel;

public class TestServerStateMachine
{

    #region ProcessInputMessages
    /// <summary>
    /// GIVEN: InputMessage Queue with a message, valid InputBuffer Map
    /// WHEN: ProcessInputMessages() is called
    /// THEN: Input from Queue is the LastRecieved() of the InputBuffer
    /// </summary>
    [Test]
    public void TestProcessInputMessagesAddsToInputBuffer()
    {
        uint mockNetId = 10;
        uint bufferSize = 64;
        uint mockStartTick = 15;
        Vector2 mockMovement = Vector2.up;
        Inputs mockInputs = new Inputs { movement = mockMovement };
        List<Inputs> mockInputList = new List<Inputs> { mockInputs };

        InputMessage inputMessage = new InputMessage { netId = mockNetId, startTick = mockStartTick, inputs = mockInputList };
        Queue<InputMessage> inputQueue = new Queue<InputMessage>();
        inputQueue.Enqueue(inputMessage);

        Dictionary<uint, InputBuffer<Inputs>> inputBufferMap = new Dictionary<uint, InputBuffer<Inputs>>();
        InputBuffer<Inputs> inputBuffer = new InputBuffer<Inputs>(bufferSize);
        inputBufferMap.Add(mockNetId, inputBuffer);

        ServerStateMachine.ProcessInputMessages(ref inputQueue, ref inputBufferMap, bufferSize);

        Assert.AreEqual(inputBuffer.LastRecieved().input.movement, mockMovement);
        Assert.AreEqual(inputBuffer.LastRecieved().clientTick, mockStartTick);
    }

    /// <summary>
    /// GIVEN: InputMessage Queue with a message with two inputs, one already received, valid InputBuffer Map with entry already received
    /// WHEN: ProcessInputMessages() is called
    /// THEN: Input from Queue is the LastRecieved() of the InputBuffer
    /// </summary>
    [Test]
    public void TestProcessInputMessagesExistingInputReceived()
    {
        uint mockNetId = 10;
        uint bufferSize = 64;
        uint mockStartTick = 15;
        Vector2 mockMovementFirst = Vector2.up;
        Vector2 mockMovementSecond = Vector2.down;

        Inputs mockInputsFirst = new Inputs { movement = mockMovementFirst };
        Inputs mockInputsSecond = new Inputs { movement = mockMovementSecond };
        List<Inputs> mockInputList = new List<Inputs> { mockInputsFirst, mockInputsSecond };

        InputMessage inputMessage = new InputMessage { netId = mockNetId, startTick = mockStartTick, inputs = mockInputList };
        Queue<InputMessage> inputQueue = new Queue<InputMessage>();
        inputQueue.Enqueue(inputMessage);

        Dictionary<uint, InputBuffer<Inputs>> inputBufferMap = new Dictionary<uint, InputBuffer<Inputs>>();
        InputBuffer<Inputs> inputBuffer = new InputBuffer<Inputs>(bufferSize);
        inputBuffer.Enqueue(mockInputsFirst, mockStartTick);
        inputBufferMap.Add(mockNetId, inputBuffer);

        ServerStateMachine.ProcessInputMessages(ref inputQueue, ref inputBufferMap, bufferSize);

        Assert.AreEqual(inputBuffer.LastRecieved().input.movement, mockMovementSecond);
        Assert.AreEqual(inputBuffer.LastRecieved().clientTick, mockStartTick + 1);
    }

    /// <summary>
    /// GIVEN: InputMessage Queue with a message with one input, valid InputBuffer Map with entry already received
    /// WHEN: ProcessInputMessages() is called
    /// THEN: InputBuffer has not changed
    /// </summary>
    [Test]
    public void TestProcessInputMessagesExistingNothingNew()
    {
        uint mockNetId = 10;
        uint bufferSize = 64;
        uint mockStartTick = 15;
        Vector2 mockMovement = Vector2.up;

        Inputs mockInputs = new Inputs { movement = mockMovement };
        List<Inputs> mockInputList = new List<Inputs> { mockInputs };

        InputMessage inputMessage = new InputMessage { netId = mockNetId, startTick = mockStartTick, inputs = mockInputList };
        Queue<InputMessage> inputQueue = new Queue<InputMessage>();
        inputQueue.Enqueue(inputMessage);

        Dictionary<uint, InputBuffer<Inputs>> inputBufferMap = new Dictionary<uint, InputBuffer<Inputs>>();
        InputBuffer<Inputs> inputBuffer = new InputBuffer<Inputs>(bufferSize);
        inputBuffer.Enqueue(mockInputs, mockStartTick);
        inputBufferMap.Add(mockNetId, inputBuffer);

        Assert.AreEqual(inputBuffer.LastRecieved().input.movement, mockMovement);
        Assert.AreEqual(inputBuffer.LastRecieved().clientTick, mockStartTick);

        ServerStateMachine.ProcessInputMessages(ref inputQueue, ref inputBufferMap, bufferSize);

        Assert.AreEqual(inputBuffer.LastRecieved().input.movement, mockMovement);
        Assert.AreEqual(inputBuffer.LastRecieved().clientTick, mockStartTick);
    }

    /// <summary>
    /// GIVEN: InputMessage Queue with a message with one input, empty InputBuffer map
    /// WHEN: ProcessInputMessages() is called
    /// THEN: NetId of the message has been been added as a key in the InputBuffer map
    /// </summary>
    [Test]
    public void TestProcessInputMessagesNewNetId()
    {
        uint mockNetId = 10;
        uint bufferSize = 64;
        uint mockStartTick = 15;
        Vector2 mockMovement = Vector2.up;

        Inputs mockInputs = new Inputs { movement = mockMovement };
        List<Inputs> mockInputList = new List<Inputs> { mockInputs };

        InputMessage inputMessage = new InputMessage { netId = mockNetId, startTick = mockStartTick, inputs = mockInputList };
        Queue<InputMessage> inputQueue = new Queue<InputMessage>();
        inputQueue.Enqueue(inputMessage);

        Dictionary<uint, InputBuffer<Inputs>> inputBufferMap = new Dictionary<uint, InputBuffer<Inputs>>();

        ServerStateMachine.ProcessInputMessages(ref inputQueue, ref inputBufferMap, bufferSize);

        Assert.True(inputBufferMap.ContainsKey(mockNetId));
    }

    #endregion

    #region ApplyInput
    /// <summary>
    /// GIVEN: InputBuffer map with an unprocessed input, Input map with a mock player
    /// WHEN: ApplyInput() is called
    /// THEN: LastProcessed equal to the mock movement, clientTick, and serverTick
    ///       Input was applied to the MockPlayer
    /// </summary>
    [Test]
    public void TestApplyInput()
    {
        uint mockNetId = 10;
        uint bufferSize = 64;
        uint mockClientTick = 15;
        uint mockServerTick = 30;
        Vector2 mockMovement = Vector2.up;

        Inputs mockInputs = new Inputs { movement = mockMovement };

        Dictionary<uint, InputBuffer<Inputs>> inputBufferMap = new Dictionary<uint, InputBuffer<Inputs>>();
        InputBuffer<Inputs> inputBuffer = new InputBuffer<Inputs>(bufferSize);
        inputBuffer.Enqueue(mockInputs, mockClientTick);
        inputBufferMap.Add(mockNetId, inputBuffer);

        Dictionary<uint, IInputful> inputMap = new Dictionary<uint, IInputful>();
        MockPlayer mockPlayer = new MockPlayer();
        Vector2 initialPosition = mockPlayer.GetPosition();
        inputMap.Add(mockNetId, mockPlayer);

        ServerStateMachine.ApplyInput(ref inputBufferMap, ref inputMap, mockServerTick);

        Assert.AreEqual(inputBuffer.LastProcessed().input.movement, mockMovement);
        Assert.AreEqual(inputBuffer.LastProcessed().clientTick, mockClientTick);
        Assert.AreEqual(inputBuffer.LastProcessed().serverTick, mockServerTick);
        Assert.AreEqual(mockPlayer.GetPosition(), initialPosition + mockMovement);
    }

    /// <summary>
    /// GIVEN: InputBuffer map with an unprocessed input, Input map missing that player
    /// WHEN: ApplyInput() is called
    /// THEN: Input is not Dequeued
    /// </summary>
    [Test]
    public void TestApplyInputNoPlayer()
    {
        uint mockNetId = 10;
        uint bufferSize = 64;
        uint mockClientTick = 15;
        uint mockServerTick = 30;
        Vector2 mockMovement = Vector2.up;

        Inputs mockInputs = new Inputs { movement = mockMovement };

        Dictionary<uint, InputBuffer<Inputs>> inputBufferMap = new Dictionary<uint, InputBuffer<Inputs>>();
        InputBuffer<Inputs> inputBuffer = new InputBuffer<Inputs>(bufferSize);
        inputBuffer.Enqueue(mockInputs, mockClientTick);
        inputBufferMap.Add(mockNetId, inputBuffer);

        Dictionary<uint, IInputful> inputMap = new Dictionary<uint, IInputful>();

        ServerStateMachine.ApplyInput(ref inputBufferMap, ref inputMap, mockServerTick);

        Assert.True(!inputBuffer.BeenProcessed);
    }

    /// <summary>
    /// GIVEN: InputBuffer map with an no unprocessed input, Input map with a mock player
    /// WHEN: ApplyInput() is called
    /// THEN: Input is not applied
    /// </summary>
    [Test]
    public void TestApplyInputNoUnproccessedInput()
    {
        uint mockNetId = 10;
        uint bufferSize = 64;
        uint mockServerTick = 30;
        Vector2 mockMovement = Vector2.up;

        Inputs mockInputs = new Inputs { movement = mockMovement };

        Dictionary<uint, InputBuffer<Inputs>> inputBufferMap = new Dictionary<uint, InputBuffer<Inputs>>();
        InputBuffer<Inputs> inputBuffer = new InputBuffer<Inputs>(bufferSize);
        inputBufferMap.Add(mockNetId, inputBuffer);

        Dictionary<uint, IInputful> inputMap = new Dictionary<uint, IInputful>();
        MockPlayer mockPlayer = new MockPlayer();
        Vector2 initialPosition = mockPlayer.GetPosition();
        inputMap.Add(mockNetId, mockPlayer);

        ServerStateMachine.ApplyInput(ref inputBufferMap, ref inputMap, mockServerTick);

        Assert.AreEqual(mockPlayer.GetPosition(), initialPosition);
    }

    #endregion

    #region CreateStateMessage

    /// <summary>
    /// GIVEN: InputBuffer map with a processed Input, valid State map
    /// WHEN: Server Tick is incrementeted and CreateStateMessage() is called
    /// THEN: Returns default state of player
    ///       Server tick references passed Server tick
    ///       Last processed client tick reference input buffer
    ///       Last processed server tick reference input buffer
    /// </summary>
    [Test]
    public void TestCreateStateMessage()
    {
        uint mockNetId = 10;
        uint bufferSize = 64;
        uint mockServerTick = 30;
        uint mockClientTick = 15;

        Vector2 mockMovement = Vector2.up;

        Inputs mockInputs = new Inputs { movement = mockMovement };

        Dictionary<uint, InputBuffer<Inputs>> inputBufferMap = new Dictionary<uint, InputBuffer<Inputs>>();
        InputBuffer<Inputs> inputBuffer = new InputBuffer<Inputs>(bufferSize);
        inputBuffer.Enqueue(mockInputs, mockClientTick);
        inputBuffer.Dequeue(mockServerTick);
        inputBufferMap.Add(mockNetId, inputBuffer);
        

        Dictionary<uint, IStateful> stateMap = new Dictionary<uint, IStateful>();
        MockPlayer mockPlayer = new MockPlayer();

        // Make sure we're capturing non-default state
        mockPlayer.SetState(new State { position = Vector2.up });
        stateMap.Add(mockNetId, mockPlayer);

        uint newMockServerTick = mockServerTick + 1;

        StateMessage stateMessage = ServerStateMachine.CreateStateMessage(ref inputBufferMap, in stateMap, newMockServerTick);
        Dictionary<uint, StateContext> stateMessageMap = stateMessage.GetMap();


        Assert.AreEqual(stateMessageMap[mockNetId].state.position, mockPlayer.GetPosition());
        Assert.AreEqual(stateMessageMap[mockNetId].tickSync.lastProcessedClientTick, mockClientTick);
        Assert.AreEqual(stateMessageMap[mockNetId].tickSync.lastProcessedServerTick, mockServerTick);
        Assert.AreEqual(stateMessage.serverTick, newMockServerTick);
    }

    /// <summary>
    /// GIVEN: Empty Input buffer map, valid State map
    /// WHEN: Server Tick is incrementeted and CreateStateMessage() is called
    /// THEN: Returns default state of player
    ///       Server tick references passed Server tick
    ///       TickSync is null
    /// </summary>
    [Test]
    public void TestCreateStateMessageNoPlayer()
    {
        uint mockNetId = 10;
        uint bufferSize = 64;
        uint mockServerTick = 30;

        Dictionary<uint, InputBuffer<Inputs>> inputBufferMap = new Dictionary<uint, InputBuffer<Inputs>>();
        InputBuffer<Inputs> inputBuffer = new InputBuffer<Inputs>(bufferSize);
        inputBufferMap.Add(mockNetId, inputBuffer);


        Dictionary<uint, IStateful> stateMap = new Dictionary<uint, IStateful>();
        MockPlayer mockPlayer = new MockPlayer();

        // Make sure we're capturing non-default state
        mockPlayer.SetState(new State { position = Vector2.up });
        stateMap.Add(mockNetId, mockPlayer);

        uint newMockServerTick = mockServerTick + 1;

        StateMessage stateMessage = ServerStateMachine.CreateStateMessage(ref inputBufferMap, in stateMap, newMockServerTick);
        Dictionary<uint, StateContext> stateMessageMap = stateMessage.GetMap();


        Assert.AreEqual(stateMessageMap[mockNetId].state.position, mockPlayer.GetPosition());
        Assert.AreEqual(stateMessageMap[mockNetId].tickSync, null);
        Assert.AreEqual(stateMessage.serverTick, newMockServerTick);
    }

    /// <summary>
    /// GIVEN: Input buffer map with no processed input, valid State map
    /// WHEN: Server Tick is incrementeted and CreateStateMessage() is called
    /// THEN: Returns default state of player
    ///       Server tick references passed Server tick
    ///       TickSync is null
    /// </summary>
    [Test]
    public void TestCreateStateMessageNotProcessed()
    {
        uint mockNetId = 10;
        uint bufferSize = 64;
        uint mockServerTick = 30;
        uint mockClientTick = 15;

        Vector2 mockMovement = Vector2.up;

        Inputs mockInputs = new Inputs { movement = mockMovement };

        Dictionary<uint, InputBuffer<Inputs>> inputBufferMap = new Dictionary<uint, InputBuffer<Inputs>>();
        InputBuffer<Inputs> inputBuffer = new InputBuffer<Inputs>(bufferSize);
        inputBuffer.Enqueue(mockInputs, mockClientTick);
        inputBufferMap.Add(mockNetId, inputBuffer);


        Dictionary<uint, IStateful> stateMap = new Dictionary<uint, IStateful>();
        MockPlayer mockPlayer = new MockPlayer();

        // Make sure we're capturing non-default state
        mockPlayer.SetState(new State { position = Vector2.up });
        stateMap.Add(mockNetId, mockPlayer);

        uint newMockServerTick = mockServerTick + 1;

        StateMessage stateMessage = ServerStateMachine.CreateStateMessage(ref inputBufferMap, in stateMap, newMockServerTick);
        Dictionary<uint, StateContext> stateMessageMap = stateMessage.GetMap();


        Assert.AreEqual(stateMessageMap[mockNetId].state.position, mockPlayer.GetPosition());
        Assert.AreEqual(stateMessageMap[mockNetId].tickSync, null);
        Assert.AreEqual(stateMessage.serverTick, newMockServerTick);
    }

    #endregion

    #region SendStateMessage

    /// <summary>
    /// GIVEN: Empty StateMessage Queue, default StateMessage
    /// WHEN: SendStateMessage() is called
    /// THEN: Message is queued
    /// </summary>
    [Test]
    public void TestSendStateMessage()
    {
        uint mockServerTick = 30;
        Queue<StateMessage> stateMessages = new Queue<StateMessage>();
        StateMessage stateMessage = new StateMessage { serverTick = mockServerTick };

        ServerStateMachine.SendStateMessage(in stateMessage, ref stateMessages);

        Assert.AreEqual(stateMessages.Peek(), stateMessage);
    }

    #endregion
}
