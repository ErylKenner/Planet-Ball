using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public class Test_RotationalPhysics
    {
        float accumulator;
        //private GameObject player;
        //private Player pl;
        //private GameObject planet;
        private GameObject mockObject;
        private Rigidbody2D mockBody;
        private float startingRadius;
        private float desiredRadius;
        private float defaultSpeed;

        [SetUp]
        public void Setup()
        {
            mockObject = new GameObject();
            mockObject.AddComponent<Rigidbody2D>();
            mockBody = mockObject.GetComponent<Rigidbody2D>();
            startingRadius = 100;
            desiredRadius = 50;
            defaultSpeed = 200;
            accumulator = 0.0f;
        }

        [TearDown]
        public void Teardown()
        {

        }

        [Test]
        public void RotateAroundPoint_ReelMedium()
        {
            Vector2 centerpoint = new Vector2(0, 0);
            mockBody.position = new Vector2(startingRadius, 0);
            mockBody.velocity = new Vector2(0, -defaultSpeed);
            while (Vector2.Distance(mockBody.position, centerpoint) > desiredRadius && accumulator < 0.35f)
            {
                accumulator += Time.fixedDeltaTime;
                RotationalPhysics.RotateAroundPoint(mockBody, centerpoint, desiredRadius, defaultSpeed, Time.fixedDeltaTime);
                mockBody.position += mockBody.velocity * Time.fixedDeltaTime;
            }
            Assert.AreEqual(50, Vector2.Distance(mockBody.position, centerpoint), 5);
        }


        [Test]
        public void RotateAroundPoint_NormalUseShort()
        {
            Vector2 centerpoint = new Vector2(0, 0);
            mockBody.position = new Vector2(startingRadius, 0);
            mockBody.velocity = new Vector2(0, -defaultSpeed);
            while (accumulator < 0.785398f)
            {
                accumulator += Time.fixedDeltaTime;
                RotationalPhysics.RotateAroundPoint(mockBody, centerpoint, startingRadius, defaultSpeed, Time.fixedDeltaTime);
                mockBody.position += mockBody.velocity * Time.fixedDeltaTime;
            }
            Assert.AreEqual(0, mockBody.position.x, 5);
            Assert.AreEqual(-100, mockBody.position.y, 5);
        }

        [Test]
        public void RotateAroundPoint_NormalUseMedium()
        {
            Vector2 centerpoint = new Vector2(0, 0);
            mockBody.position = new Vector2(startingRadius, 0);
            mockBody.velocity = new Vector2(0, -defaultSpeed);
            while (accumulator < 1.5708f)
            {
                accumulator += Time.fixedDeltaTime;
                RotationalPhysics.RotateAroundPoint(mockBody, centerpoint, startingRadius, defaultSpeed, Time.fixedDeltaTime);
                mockBody.position += mockBody.velocity * Time.fixedDeltaTime;
            }
            Assert.AreEqual(-100, mockBody.position.x, 5);
            Assert.AreEqual(0, mockBody.position.y, 5);
        }

        [Test]
        public void RotateAroundPoint_NormalUseLong()
        {
            Vector2 centerpoint = new Vector2(0, 0);
            mockBody.position = new Vector2(startingRadius, 0);
            mockBody.velocity = new Vector2(0, -defaultSpeed);
            while (accumulator < 15.708f)
            {
                accumulator += Time.fixedDeltaTime;
                RotationalPhysics.RotateAroundPoint(mockBody, centerpoint, startingRadius, defaultSpeed, Time.fixedDeltaTime);
                mockBody.position += mockBody.velocity * Time.fixedDeltaTime;
            }
            Assert.AreEqual(100, mockBody.position.x, 5);
            Assert.AreEqual(0, mockBody.position.y, 5);
        }


        /*[Test]
        public void PlayerSanity()
        {
            Assert.IsNotNull(player.GetComponent<Player>().Body);
            Assert.AreEqual(100, player.GetComponent<Player>().AttachedPlanetRadius);
        }*/


        [Test]
        public void ConvertToUnitTangentialVelocity_NormalUse()
        {
            Vector2 position = new Vector2(9, 13);
            Vector2 velocity = new Vector2(-2, -9);
            Vector2 center = new Vector2(0, 1);
            Vector2 output = RotationalPhysics.ConvertToUnitTangentialVelocity(position, velocity, center);
            Assert.AreEqual(new Vector2(0.8f, -0.6f), output);
        }

        [Test]
        public void ConvertToUnitTangentialVelocity_Zeros()
        {
            Vector2 position = new Vector2(0, 0);
            Vector2 velocity = new Vector2(0, 0);
            Vector2 center = new Vector2(0, 0);
            Vector2 output = RotationalPhysics.ConvertToUnitTangentialVelocity(position, velocity, center);
            Assert.AreEqual(new Vector2(0.0f, -0.0f), output);
        }

        [Test]
        public void ConvertToUnitTangentialVelocity_Horizontal()
        {
            Vector2 position = new Vector2(9, 0);
            Vector2 velocity = new Vector2(0, 1);
            Vector2 center = new Vector2(0, 0);
            Vector2 output = RotationalPhysics.ConvertToUnitTangentialVelocity(position, velocity, center);
            Assert.AreEqual(new Vector2(0.0f, 1.0f), output);
        }

        [Test]
        public void ConvertToUnitTangentialVelocity_VelocityAlongRadius()
        {
            Vector2 position = new Vector2(9, 0);
            Vector2 velocity = new Vector2(-3, 0);
            Vector2 center = new Vector2(0, 0);
            Vector2 output = RotationalPhysics.ConvertToUnitTangentialVelocity(position, velocity, center);
            Assert.AreEqual(new Vector2(0.0f, -1.0f), output);
        }

    }
}