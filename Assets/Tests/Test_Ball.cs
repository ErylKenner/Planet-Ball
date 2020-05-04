using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class Test_Ball
    {
        private GameObject spawnpoint;
        private GameObject mockBall;
        private Ball ball;


        [SetUp]
        public void Setup()
        {
            spawnpoint = new GameObject();
            spawnpoint.transform.position = new Vector2(25, 30);

            GameObject mockBarrier = new GameObject();
            mockBarrier.AddComponent<BoxCollider2D>();
            mockBarrier.AddComponent<GoalBarrier>();

            mockBall = new GameObject();
            mockBall.AddComponent<CircleCollider2D>();
            mockBall.AddComponent<Rigidbody2D>();
            mockBall.AddComponent<Ball>();
            ball = mockBall.GetComponent<Ball>();
            ball.spawnPoint = spawnpoint.transform;
            ball.barrier = mockBarrier.GetComponent<GoalBarrier>();
        }

        [Test]
        public void Ball_ScoredFalse()
        {
            Assert.IsFalse(ball.Scored);
        }

        [Test]
        public void Ball_Reset()
        {
            ball.Score();
            ball.ResetBall();
            Assert.IsFalse(ball.Scored);
            Assert.AreEqual(spawnpoint.transform.position.x, mockBall.transform.position.x, 0);
            Assert.AreEqual(spawnpoint.transform.position.y, mockBall.transform.position.y, 0);
        }

    }
}
