using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Tests
{
    public class Test_Score
    {
        private GameObject mockObjectScore;
        private Score score;
        private GameObject mockObjectEndScreen;
        private EndScreen endScreen;

        [SetUp]
        public void Setup()
        {
            mockObjectScore = new GameObject();
            mockObjectScore.AddComponent<Score>();
            score = mockObjectScore.GetComponent<Score>();
            Score.ScoreTeam1 = Score.ScoreTeam2 = 0;

            mockObjectEndScreen = new GameObject();
            mockObjectEndScreen.AddComponent<EndScreen>();
            endScreen = mockObjectEndScreen.GetComponent<EndScreen>();

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
        }



        [Test]
        public void AddToScore_Team1()
        {
            Score.AddToScore(1, 1);
            Assert.AreEqual(1, Score.ScoreTeam1, 0);
        }

        [Test]
        public void AddToScore_Team2()
        {
            Score.AddToScore(2, 1);
            Assert.AreEqual(1, Score.ScoreTeam2, 0);
        }

        [Test]
        public void AddToScore_InvalidTeam()
        {
            Score.AddToScore(3, 1);
            LogAssert.Expect(LogType.Error, "Invalid team number!");
            Assert.AreEqual(0, Score.ScoreTeam1, 0);
            Assert.AreEqual(0, Score.ScoreTeam2, 0);
        }


        [Test]
        public void AddToScore_Team1Win()
        {
            Score.ScoreTeam2 = Score.ScoreToWin - 1;
            Score.AddToScore(2, 1);
            Assert.AreEqual(Score.ScoreToWin, Score.ScoreTeam2, 0);
            Assert.IsTrue(endScreen.Ended);
        }

        [Test]
        public void GetScore_Invalid()
        {
            Assert.AreEqual(-1, Score.GetScore(3));
            LogAssert.Expect(LogType.Error, "Invalid team number!");
        }

    }
}
