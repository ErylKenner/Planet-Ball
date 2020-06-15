using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Collider2D))]
public class Goal : MonoBehaviour
{
    public delegate void BallScored(int team);
    public static event BallScored OnBallScored;

    public ParticleSystem particle;
    public int TeamNumber;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Ball ball = other.GetComponent<Ball>();
        if (ball != null && !ball.Scored)
        {
            particle.Play();
            OnBallScored?.Invoke(TeamNumber);
        }
    }
}
