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


    public override State GetState() {
        State ret = base.GetState();
        ret.playerState = playerController.playerState;
        return ret;
    }

    public override void SetState(State state)
    {
        base.SetState(state);
        playerController.playerState = state.playerState;
        // IF THIS BREAKS, WE MAY NEED TO DO A DEEPCOPY
    }




    public void ApplyInput(Inputs input)
    {
        playerController.ApplyInput(input, Time.fixedDeltaTime);
        //TODO: Fix this
        //playerController.Move(input.movement);
    }

    public Inputs GetInput()
    {
        return playerController.GetInputs();
    }

    protected override void Start()
    {
        playerController = GetComponent<PlayerPlanetController>();
        base.Start();
        NetworkedManager.instance.client.AddInputful(this, netId, isLocalPlayer);
        NetworkedManager.instance.server.AddInputful(this, netId);
    }

    protected override void OnDestroy()
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
