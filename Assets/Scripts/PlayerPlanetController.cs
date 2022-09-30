using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;

public class PlayerPlanetController : NetworkBehaviour
{
    public GameObject Model;
    private CharacterController controller;

    private Vector2 movement;

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
    }

    // Start is called before the first frame update
    public void Start()
    {
        //base.Start();
        if (isLocalPlayer)
        {
            controller = gameObject.GetComponent<CharacterController>();
        }
        else
        {
            // Disable any components that we don't want to compute since they belong to other players
        }
    }
    
    public void OnMove(InputValue axis)
    {
        if (axis.Get() == null)
        {
            movement = Vector2.zero;
        }
        else
        {
            movement = (Vector2)axis.Get();
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isLocalPlayer)
        {
            Vector3 dir = new Vector3(movement.x, 0.0f, movement.y);
            controller.Move(dir.normalized * 5 * Time.deltaTime);
        }
    }
}
