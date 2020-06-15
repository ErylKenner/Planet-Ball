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
            ball.SpawnPoint = spawnpoint.transform;
            ball.Barrier = mockBarrier.GetComponent<GoalBarrier>();
        }

        [Test]
        public void Ball_ScoredFalse()
        {
            Assert.IsFalse(ball.Scored);
        }



    }
}
