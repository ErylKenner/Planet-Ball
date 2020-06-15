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

            mockObjectEndScreen = new GameObject();
            mockObjectEndScreen.AddComponent<EndScreen>();
            endScreen = mockObjectEndScreen.GetComponent<EndScreen>();

            GameObject temp1 = new GameObject();
            GameObject temp2 = new GameObject();
            GameObject temp3 = new GameObject();
            temp1.AddComponent<Text>();
            temp2.AddComponent<Image>();
            temp3.AddComponent<Text>();
            endScreen.EndScreenImage = temp2.GetComponent<Image>();
            endScreen.EndScreenText = temp1.GetComponent<Text>();
            endScreen.FinalScoreText = temp3.GetComponent<Text>();
        }

        [Test]
        public void GetScore_Invalid()
        {
            Assert.Catch<System.NullReferenceException>(() => Score.GetScore(3));
        }

    }
}
