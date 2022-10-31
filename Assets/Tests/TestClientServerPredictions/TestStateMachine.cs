using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using ClientServerPrediction;
using MockModel;

public class TestStateMachine
{
    #region Run

    /// <summary>
    /// GIVEN: Valid player, input, Runner, RunContext
    /// WHEN: Run() is called
    /// THEN: Input is applied to the MockPlayer
    /// </summary>
    [Test]
    public void TestRun()
    {
        uint mockNetId = 10;

        MockPlayer mockPlayer = new MockPlayer();
        Dictionary<uint, Inputs> inputActionMap = new Dictionary<uint, Inputs>();
        Dictionary<uint, IInputful> inputMap = new Dictionary<uint, IInputful>();

        Inputs inputAction = mockPlayer.GetInput();
        inputActionMap.Add(mockNetId, inputAction);
        inputMap.Add(mockNetId, mockPlayer);
        State originalState = mockPlayer.GetState();

        MockRunner mockRunner = new MockRunner();
        RunContext runContext = new RunContext();

        StateMachine.Run(in inputActionMap, ref inputMap, mockRunner, runContext);

        Assert.AreEqual(mockPlayer.GetState().position, originalState.position + inputAction.movement);
    }

    /// <summary>
    /// GIVEN: Valid player, no input, Runner, RunContext
    /// WHEN: Run() is called
    /// THEN: MockPlayer remains
    /// </summary>
    [Test]
    public void TestRunNoInput ()
    {
        uint mockNetId = 10;

        MockPlayer mockPlayer = new MockPlayer();
        Dictionary<uint, Inputs> inputActionMap = new Dictionary<uint, Inputs>();
        Dictionary<uint, IInputful> inputMap = new Dictionary<uint, IInputful>();

        inputMap.Add(mockNetId, mockPlayer);
        State originalState = mockPlayer.GetState();

        MockRunner mockRunner = new MockRunner();
        RunContext runContext = new RunContext();

        StateMachine.Run(in inputActionMap, ref inputMap, mockRunner, runContext);

        Assert.AreEqual(mockPlayer.GetState().position, originalState.position);
    }

    #endregion
}
