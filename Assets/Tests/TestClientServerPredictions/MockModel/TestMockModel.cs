using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using MockModel;
using ClientServerPrediction;

public class TestMockModel
{
    /// <summary>
    /// GIVEN: Default MockPlayer
    /// WHEN: GetPosition() is called
    /// THEN: Returns default position (Vector2.zero)
    /// </summary>
    [Test]
    public void TestGetPosition()
    {
        MockPlayer player = new MockPlayer();
        Assert.AreEqual(player.GetPosition(), Vector2.zero);
    }

    /// <summary>
    /// GIVEN: Default MockPlayer, Input with Vector2.Up
    /// WHEN: ApplyInput is called
    /// THEN: GetPosition returns that player has moved up one
    /// </summary>
    [Test]
    public void TestApplyInput()
    {
        MockPlayer player = new MockPlayer();
        Inputs input = new Inputs { movement = Vector2.up };

        player.ApplyInput(input);

        Assert.AreEqual(player.GetPosition(), Vector2.up);
    }

    /// <summary>
    /// GIVEN: Default MockPlayer
    /// WHEN: GetInput() is called
    /// THEN: Returns default input (Vector2.up)
    /// </summary>
    [Test]
    public void TestGetInput()
    {
        MockPlayer player = new MockPlayer();
        Assert.AreEqual(player.GetInput().movement, Vector2.up);
    }

    /// <summary>
    /// GIVEN: Default MockPlayer
    /// WHEN: GetState() is called
    /// THEN: Return state with position = default position (Vector2.zero)
    /// </summary>
    [Test]
    public void TestGetState()
    {
        MockPlayer player = new MockPlayer();
        Assert.AreEqual(player.GetState().position, Vector2.zero);
    }

    /// <summary>
    /// GIVEN: Default MockPlayer, State with position = Vector2.up
    /// WHEN: SetState() is called
    /// THEN: Position is set to State position
    /// </summary>
    [Test]
    public void TestSetState()
    {
        MockPlayer player = new MockPlayer();
        State state = new State { position = Vector2.up };
        player.SetState(state);
        Assert.AreEqual(player.GetPosition(), Vector2.up);
    }
}
