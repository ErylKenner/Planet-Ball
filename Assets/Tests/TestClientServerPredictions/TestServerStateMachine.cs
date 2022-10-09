using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using ClientServerPrediction;

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
    public void TestProcessInputMessagesExistingNewNetId()
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
}
