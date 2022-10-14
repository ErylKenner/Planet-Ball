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
}
