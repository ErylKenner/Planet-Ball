using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class Ball : MonoBehaviour
{
    public Transform SpawnPoint;
    public GoalBarrier Barrier;

    public bool Scored { get; private set; } = false;

    private void Awake()
    {
        Goal.OnBallScored += Score;
        CountDownUI.OnRestartScene += ResetBall;
    }

    private void Start()
    {
        Barrier.IgnoreCollision(GetComponent<Collider2D>());
        ResetBall();
    }

    private void Score(int team)
    {
        Scored = true;
    }

    private void ResetBall()
    {
        Scored = false;
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        transform.position = SpawnPoint.position;
    }
}
