using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class Ball : MonoBehaviour
{

    public Transform spawnPoint;
    public GoalBarrier barrier;

    public bool Scored { get; private set; } = false;

    private void Start()
    {
        ResetBall();
    }

    public void Score()
    {
        Scored = true;
        //GetComponent<Collider2D>().enabled = false;
    }

    public void ResetBall()
    {
        Scored = false;
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        transform.position = spawnPoint.position;
        //GetComponent<Collider2D>().enabled = true;
        barrier.IgnoreCollision(GetComponent<Collider2D>());
    }
}
