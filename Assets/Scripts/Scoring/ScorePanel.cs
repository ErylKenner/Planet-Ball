using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class ScorePanel : MonoBehaviour
{

    public int teamNumber;

    private Text text;

    private void Start()
    {
        text = GetComponent<Text>();
    }

    void Update()
    {
        int currentScore = Score.GetScore(teamNumber);

        if (text.text != currentScore.ToString())
        {
            GetComponent<Text>().text = currentScore.ToString();
        }
    }
}
