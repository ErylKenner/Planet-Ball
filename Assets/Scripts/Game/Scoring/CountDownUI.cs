using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CountDownUI : MonoBehaviour
{
    public int CountDownTime = 3;
    public int DisplayScoreTime = 2;

    private Text text;
    private float remainingTime = 0;

    public delegate void RestartScene();
    public static event RestartScene OnRestartScene;

    private void Awake()
    {
        Goal.OnBallScored += StartCountdown;
        text = GetComponent<Text>();
    }

    private void Update()
    {
        if (remainingTime > 0)
        {
            if (remainingTime > CountDownTime)
            {
                text.text = Score.GetScoresTextShort();
            }
            else
            {
                text.text = Mathf.CeilToInt(remainingTime).ToString();
            }
            remainingTime -= Time.deltaTime;
            if (remainingTime <= 0.0f)
            {
                text.enabled = false;
                OnRestartScene?.Invoke();
            }
        }
    }

    private void StartCountdown(int team)
    {
        remainingTime = CountDownTime + DisplayScoreTime;
        text.color = Score.GetColor(team);
        text.enabled = true;
    }
}
