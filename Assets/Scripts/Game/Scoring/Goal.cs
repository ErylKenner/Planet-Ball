using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Collider2D))]
public class Goal : MonoBehaviour
{
    public ParticleSystem particle;
    public int TeamNumber;

    private void OnTriggerEnter2D(Collider2D other)
    {
        OnScore(other);
    }

    public void OnScore(Collider2D other)
    {
        Ball ball = other.GetComponent<Ball>();
        if (ball != null && !ball.Scored)
        {
            int scoringTeam = TeamNumber == 1 ? 2 : 1;
            Score.AddToScore(scoringTeam);

            CountDownUI.StartCountdown(ball, scoringTeam);
            particle.Play();
        }
    }
}
