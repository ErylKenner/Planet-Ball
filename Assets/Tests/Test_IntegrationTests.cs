using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Tests
{
    public class Test_IntegrationTests
    {
        private GameObject player;
        private GameObject planet = null;
        private Player pl;
        private GameObject spawnpoint;
        private GameObject mockBall;
        private Ball ball;
        private float accumulator;

        [SetUp]
        public void Setup()
        {
            float originalTimeScale = Time.timeScale;
            Time.timeScale = 0;
            accumulator = 0.0f;

            if (planet == null)
            {
                planet = new GameObject();
                planet.AddComponent<SpriteRenderer>();
                planet.AddComponent<Shadow>();
                planet.AddComponent<Planet>();
                planet.transform.position = new Vector2(0, 0);
            }

            if (player != null)
            {
                GameObject.DestroyImmediate(player);
            }
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

            pl.AttachTether();

            Time.timeScale = originalTimeScale;
        }

        [UnityTest]
        public IEnumerator Player_RotateAroundPoint_ReelMedium()
        {
            pl.Body.position = new Vector2(100, 0);
            pl.Body.velocity = new Vector2(0, -pl.DefaultSpeed);
            pl.Speed = pl.Body.velocity.magnitude;
            accumulator = 0;
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
            pl.Body.position = new Vector2(100, 0);
            pl.Body.velocity = new Vector2(0, -pl.DefaultSpeed);
            pl.Speed = pl.Body.velocity.magnitude;
            accumulator = 0;

            while (accumulator < 0.785398f)
            {
                accumulator += Time.fixedDeltaTime;
                //Debug.Log(pl.Body.position);
                yield return new WaitForFixedUpdate();
            }
            float angleDeg = Mathf.Rad2Deg * Mathf.Atan2(pl.Body.position.y, pl.Body.position.x);
            Assert.AreEqual(-90, angleDeg, 15);
            Assert.AreEqual(100, Vector2.Distance(pl.Body.position, planet.transform.position), 5);
        }

        [UnityTest]
        public IEnumerator Player_RotateAroundPoint_Medium()
        {
            pl.Body.position = new Vector2(100, 0);
            pl.Body.velocity = new Vector2(0, -pl.DefaultSpeed);
            pl.Speed = pl.Body.velocity.magnitude;
            accumulator = 0;

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
            pl.Body.position = new Vector2(100, 0);
            pl.Body.velocity = new Vector2(0, -pl.DefaultSpeed);
            pl.Speed = pl.Body.velocity.magnitude;
            accumulator = 0;

            while (accumulator < 12.56637061f)
            {
                accumulator += Time.fixedDeltaTime;
                Debug.Log(pl.Body.position);
                Debug.Log(Mathf.Rad2Deg * Mathf.Atan2(pl.Body.position.y, pl.Body.position.x));
                yield return new WaitForFixedUpdate();
            }
            float angleDeg = Mathf.Rad2Deg * Mathf.Atan2(pl.Body.position.y, pl.Body.position.x);
            Assert.AreEqual(0, angleDeg, 15);
            Assert.AreEqual(100, Vector2.Distance(pl.Body.position, planet.transform.position), 5);
        }

        [Test]
        public void Ball_CountDownUI()
        {
            GameObject mockObjectScore = new GameObject();
            mockObjectScore.AddComponent<Score>();
            Score score = mockObjectScore.GetComponent<Score>();
            Score.ScoreTeam1 = Score.ScoreTeam2 = 0;

            GameObject mockObjectEndScreen = new GameObject();
            mockObjectEndScreen.AddComponent<EndScreen>();
            EndScreen endScreen = mockObjectEndScreen.GetComponent<EndScreen>();

            GameObject temp1 = new GameObject();
            GameObject temp2 = new GameObject();
            GameObject temp3 = new GameObject();
            temp1.AddComponent<Text>();
            temp2.AddComponent<Image>();
            temp3.AddComponent<Text>();
            endScreen.screen = temp2.GetComponent<Image>();
            endScreen.text = temp1.GetComponent<Text>();
            endScreen.scoreText = temp3.GetComponent<Text>();

            Score.Instance.EndScreen = endScreen;

            GameObject text = new GameObject();
            text.AddComponent<Text>();

            GameObject countdown = new GameObject();
            countdown.AddComponent<CountDownUI>();
            countdown.GetComponent<CountDownUI>().countDownText = text.GetComponent<Text>();
            CountDownUI.StartCountdown(ball, 1);

            Assert.IsTrue(CountDownUI.Instance.countDownText.gameObject.activeSelf);
            Assert.AreEqual(Score.Instance.team1Color, CountDownUI.Instance.countDownText.color);
        }
    }
}
