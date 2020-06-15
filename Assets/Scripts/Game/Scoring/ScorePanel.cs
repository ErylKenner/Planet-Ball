using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class ScorePanel : MonoBehaviour
{
    public int TeamNumber;
    private Text text;

    private void Awake()
    {
        text = GetComponent<Text>();
        Goal.OnBallScored += UpdateText;
    }

    private void UpdateText(int teamNumber)
    {
        if (teamNumber == TeamNumber)
        {
            text.text = Score.GetScore(teamNumber).ToString();
        }
    }
}
