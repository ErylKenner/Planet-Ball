using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class Test_IntegrationTests
    {
        private GameObject player;
        private GameObject planet;
        private Player pl;
        private float accumulator;

        [SetUp]
        public void Setup()
        {
            accumulator = 0.0f;

            planet = new GameObject();
            planet.AddComponent<SpriteRenderer>();
            planet.AddComponent<Shadow>();
            planet.AddComponent<Planet>();
            planet.transform.position = new Vector2(0, 0);

            player = new GameObject();
            player.AddComponent<SpriteRenderer>();
            player.AddComponent<Rigidbody2D>();
            player.AddComponent<CircleCollider2D>();
            player.AddComponent<Shadow>();
            player.AddComponent<LineRenderer>();
            player.AddComponent<Player>();
            pl = player.GetComponent<Player>();
            pl.Body.position = new Vector2(100, 0);
            pl.Body.velocity = new Vector2(0, -pl.DefaultSpeed);
            pl.Speed = pl.Body.velocity.magnitude;

            pl.AttachTether();
        }

        [UnityTest]
        public IEnumerator Player_RotateAroundPoint_ReelMedium()
        {
            float desiredRadius = 50;
            pl.ReelTether = true;
            while (pl.AttachedPlanetRadius > desiredRadius && accumulator < 0.5f)
            {
                accumulator += Time.fixedDeltaTime;
                //Debug.Log(pl.Body.position);
                yield return new WaitForFixedUpdate();
            }
            Assert.AreEqual(50, Vector2.Distance(pl.Body.position, planet.transform.position), 5);
        }


        [UnityTest]
        public IEnumerator Player_RotateAroundPoint_Short()
        {
            while (accumulator < 0.785398f)
            {
                accumulator += Time.fixedDeltaTime;
                //Debug.Log(pl.Body.position);
                yield return new WaitForFixedUpdate();
            }
            float angleDeg = Mathf.Rad2Deg * Mathf.Atan2(pl.Body.position.y, pl.Body.position.x);
            Assert.AreEqual(90, angleDeg, 15);
            Assert.AreEqual(100, Vector2.Distance(pl.Body.position, planet.transform.position), 5);
        }

        [UnityTest]
        public IEnumerator Player_RotateAroundPoint_Medium()
        {
            while (accumulator < 1.5708f)
            {
                accumulator += Time.fixedDeltaTime;
                //Debug.Log(pl.Body.position);
                yield return new WaitForFixedUpdate();
            }
            float angleDeg = Mathf.Rad2Deg * Mathf.Atan2(pl.Body.position.y, pl.Body.position.x);
            if (angleDeg < 0)
            {
                angleDeg += 360.0f;
            }
            Assert.AreEqual(180, angleDeg, 15);
            Assert.AreEqual(100, Vector2.Distance(pl.Body.position, planet.transform.position), 5);
        }

        [UnityTest]
        public IEnumerator Player_RotateAroundPoint_Long()
        {
            while (accumulator < 15.708f)
            {
                accumulator += Time.fixedDeltaTime;
                //Debug.Log(pl.Body.position);
                yield return new WaitForFixedUpdate();
            }
            float angleDeg = Mathf.Rad2Deg * Mathf.Atan2(pl.Body.position.y, pl.Body.position.x);
            Assert.AreEqual(0, angleDeg, 15);
            Assert.AreEqual(100, Vector2.Distance(pl.Body.position, planet.transform.position), 5);
        }


    }
}
