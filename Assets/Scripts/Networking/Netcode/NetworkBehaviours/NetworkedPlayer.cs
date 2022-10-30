using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClientServerPrediction;
using UnityEngine.InputSystem;


[RequireComponent(typeof(PlayerPlanetController))]
public class NetworkedPlayer : NetworkedObject, IInputful
{
    private PlayerPlanetController playerController;
    private Vector2 movement;
    public void ApplyInput(Inputs input)
    {
        playerController.Move(input.movement);
    }

    public Inputs GetInput()
    {
        return new Inputs
        {
            movement = movement
        };
    }

    protected void Start()
    {
        playerController = GetComponent<PlayerPlanetController>();
        base.Start();
        NetworkedManager.instance.client.AddInputful(this, netId, isLocalPlayer);
        NetworkedManager.instance.server.AddInputful(this, netId);
    }

    protected void OnDestroy()
    {
        base.OnDestroy();
        NetworkedManager.instance.client.DeleteInputful(netId);
        NetworkedManager.instance.server.DeleteInputful(netId);
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
}
