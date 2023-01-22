using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayScore : MonoBehaviour
{
    public int TeamNumber;

    private TMPro.TextMeshProUGUI text;

    private void Start()
    {
        text = GetComponent<TMPro.TextMeshProUGUI>();
    }


    void Update()
    {
        ScoreManager scoreManager = NetworkedManager.instance?.GetComponent<ScoreManager>();
        text.text = scoreManager?.GetTeamScore(TeamNumber).ToString();
    }
}
