using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;

public class PlayerPlanetController : NetworkBehaviour
{
    public GameObject Model;
    public float MaxSpeed = 10f;
    private Rigidbody2D rigidbody2d;

    //public Vector2 movement;

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
    }

    private void Awake()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
    }

    // Start is called before the first frame update
    public void Start()
    {
        //base.Start();
        if (isLocalPlayer)
        {
            //controller = gameObject.GetComponent<CharacterController>();
        }
        else
        {
            // Disable any components that we don't want to compute since they belong to other players
            GetComponent<PlayerInput>().enabled = false;
        }
    }

    //public void OnMove(InputValue axis)
    //{
    //    if (axis.Get() == null)
    //    {
    //        movement = Vector2.zero;
    //    }
    //    else
    //    {
    //        movement = (Vector2)axis.Get();
    //    }

    //}

    // Update is called once per frame
    void FixedUpdate()
    {
        //if (isLocalPlayer)
        //{
        //    rigidbody2d.AddForce(100 * movement);
        //    rigidbody2d.velocity = Mathf.Clamp(rigidbody2d.velocity.magnitude, 0f, MaxSpeed) * rigidbody2d.velocity.normalized;
        //}
    }

    public void Move(Vector2 movement)
    {
        rigidbody2d.AddForce(100 * movement);
        rigidbody2d.velocity = Mathf.Clamp(rigidbody2d.velocity.magnitude, 0f, MaxSpeed) * rigidbody2d.velocity.normalized;
    }
}
