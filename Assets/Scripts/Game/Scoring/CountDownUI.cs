using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CountDownUI : MonoBehaviour
{

    public static CountDownUI Instance = null;

    public int countDownTime = 5;
    public Text countDownText;

    private float currentCountDown = 0;
    private bool counting = false;
    private Ball currentBall;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (counting)
        {
            int num = Mathf.CeilToInt(currentCountDown);

            if (num <= 3)
            {
                countDownText.text = num.ToString();
            }
            else
            {
                int p1score = Score.GetScore(1);
                int p2score = Score.GetScore(2);
                int max = Mathf.Max(p1score, p2score);
                int min = Mathf.Min(p1score, p2score);
                countDownText.text = max + " - " + min;
            }

            currentCountDown -= Time.deltaTime;

            if (currentCountDown < 0)
            {
                currentBall.ResetBall();
                counting = false;
                countDownText.gameObject.SetActive(false);
            }
        }
    }

    public static void StartCountdown(Ball ball, int playerNumber)
    {


        ball.Score();
        Instance.currentBall = ball;
        Instance.currentCountDown = Instance.countDownTime;
        Instance.counting = true;
        Instance.countDownText.color = Score.GetColor(playerNumber);
        Instance.countDownText.gameObject.SetActive(true);
    }


}
