using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class GoalBarrier : MonoBehaviour
{

    public void IgnoreCollision(Collider2D collider)
    {
        Physics2D.IgnoreCollision(GetComponent<Collider2D>(), collider);
    }
}
