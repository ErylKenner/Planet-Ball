using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

public class Test_RotationalPhysics
{
    [SetUp]
    public void Setup()
    {

    }

    [TearDown]
    public void Teardown()
    {

    }

    [UnityTest]
    public IEnumerator ConvertToUnitTangentialVelocity_NormalUse()
    {
        Vector2 position = new Vector2(9, 13);
        Vector2 velocity = new Vector2(-2, -9);
        Vector2 center = new Vector2(0, 1);
        Vector2 output = RotationalPhysics.ConvertToUnitTangentialVelocity(position, velocity, center);
        Assert.AreEqual(new Vector2(0.8f, -0.6f), output);
        yield return null;
    }

    [UnityTest]
    public IEnumerator ConvertToUnitTangentialVelocity_Zero()
    {
        Vector2 position = new Vector2(0, 0);
        Vector2 velocity = new Vector2(0, 0);
        Vector2 center = new Vector2(0, 0);
        Vector2 output = RotationalPhysics.ConvertToUnitTangentialVelocity(position, velocity, center);
        Assert.AreEqual(new Vector2(0.0f, -0.0f), output);
        yield return null;
    }

    [UnityTest]
    public IEnumerator ConvertToUnitTangentialVelocity_Horizontal()
    {
        Vector2 position = new Vector2(9, 0);
        Vector2 velocity = new Vector2(0, 1);
        Vector2 center = new Vector2(0, 0);
        Vector2 output = RotationalPhysics.ConvertToUnitTangentialVelocity(position, velocity, center);
        Assert.AreEqual(new Vector2(0.0f, 1.0f), output);
        yield return null;
    }

    [UnityTest]
    public IEnumerator ConvertToUnitTangentialVelocity_VelocityAlongRadius()
    {
        Vector2 position = new Vector2(9, 0);
        Vector2 velocity = new Vector2(-3, 0);
        Vector2 center = new Vector2(1, 0);
        Vector2 output = RotationalPhysics.ConvertToUnitTangentialVelocity(position, velocity, center);
        Assert.AreEqual(new Vector2(0.0f, -1.0f), output);
        yield return null;
    }

}