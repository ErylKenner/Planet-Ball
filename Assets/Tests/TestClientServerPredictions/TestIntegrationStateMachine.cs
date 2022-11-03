using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using ClientServerPrediction;
using MockModel;


public class TestIntegrationStateMachine
{

    /// <summary>
    /// GIVEN: Valid objects
    /// WHEN: The entire state machine is called
    /// THEN: The client correction behaves as expected
    /// </summary>
    [Test]
    public void TestStateMachine()
    {
        // Consts
        uint mockNetId = 10;
        uint mockBufferSize = 64;
        uint mockClientTick = 20;
        uint mockServerTick = 10;

        MockPlayer mockPlayer = new MockPlayer();
        MockPlayer mockServerPlayer = new MockPlayer();
        MockRunner mockRunner = new MockRunner();


        // Client Objects
        Queue<StateMessage> stateMessageQueue = new Queue<StateMessage>();
        Dictionary<uint, IInputful> inputMap = new Dictionary<uint, IInputful>();
        Dictionary<uint, IStateful> stateMap = new Dictionary<uint, IStateful>();

        inputMap.Add(mockNetId, mockPlayer);
        stateMap.Add(mockNetId, mockPlayer);

        Dictionary<uint, Inputs[]> inputBufferMap = new Dictionary<uint, Inputs[]>();
        Dictionary<uint, State[]> stateBufferMap = new Dictionary<uint, State[]>();

        Inputs[] inputBuffer = new Inputs[mockBufferSize];
        State[] stateBuffer = new State[mockBufferSize];

        stateBufferMap.Add(mockNetId, stateBuffer);
        inputBufferMap.Add(mockNetId, inputBuffer);


        //Server Object
        Queue<InputMessage> inputMessageQueue = new Queue<InputMessage>();
        Dictionary<uint, IInputful> serverInputMap = new Dictionary<uint, IInputful>();
        Dictionary<uint, IStateful> serverStateMap = new Dictionary<uint, IStateful>();

        serverInputMap.Add(mockNetId, mockServerPlayer);
        serverStateMap.Add(mockNetId, mockServerPlayer);

        Dictionary<uint, InputBuffer<Inputs>> serverInputBufferMap = new Dictionary<uint, InputBuffer<Inputs>>();

        // Client
        Dictionary<uint, Inputs> currentInputMap = ClientStateMachine.StoreInput(ref inputBufferMap, in inputMap, mockClientTick);
        StateMachine.Run(currentInputMap, ref inputMap, ref stateMap, mockRunner, new RunContext());
        InputMessage inputMessage = ClientStateMachine.CreateInputMessage(inputBufferMap, mockClientTick - 1, mockClientTick);
        mockClientTick++;
        ClientStateMachine.StoreState(ref stateBufferMap, in stateMap, mockClientTick);
        ClientStateMachine.SendInputMessage(inputMessage, ref inputMessageQueue);

        // Server
        ServerStateMachine.ProcessInputMessages(ref inputMessageQueue, ref serverInputBufferMap, mockBufferSize);
        ServerStateMachine.ApplyInput(ref serverInputBufferMap, ref serverInputMap, mockServerTick);
        // TODO: Run the runner
        mockServerTick++;
        StateMessage stateMessage = ServerStateMachine.CreateStateMessage(ref serverInputBufferMap, serverStateMap, mockServerTick);
        ServerStateMachine.SendStateMessage(in stateMessage, ref stateMessageQueue);

        //Back to the Client
        StateMessage lastestStateMessage = ClientStateMachine.GetLatestStateMessage(ref stateMessageQueue, mockNetId);
        StateError stateError = new StateError { positionDiff = 0.1f };

        State originalState = mockPlayer.GetState();

        uint lastReceivedTick = ClientStateMachine.CorrectClient(
            ref inputBufferMap,
            ref stateBufferMap,
            ref inputMap,
            ref stateMap,
            in lastestStateMessage,
            in stateError,
            mockRunner,
            new RunContext(),
            mockNetId,
            mockClientTick,
            0
         );

        Assert.AreEqual(lastReceivedTick, mockClientTick - 1);
        Assert.AreEqual(originalState.position, mockPlayer.GetState().position);




    }

}