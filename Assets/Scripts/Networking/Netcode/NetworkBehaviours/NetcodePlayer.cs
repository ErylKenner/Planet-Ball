using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClientServerPrediction;
using UnityEngine.InputSystem;


[RequireComponent(typeof(PlayerPlanetController))]
public class NetcodePlayer : NetcodeObject, IInputful
{
    private PlayerPlanetController playerController;


    public override State GetState() {
        State ret = base.GetState();
        ret.playerState = new PlayerState(playerController.playerState);
        return ret;
    }

    public override void SetState(State state)
    {
        base.SetState(state);
        playerController.playerState = new PlayerState(state.playerState);
    }

    public override void PredictState(State state)
    {
        base.PredictState(state);
        Inputs previousInputs = Inputs.FromState(state);
        if(previousInputs != null)
        {
            // TODO: Pass RunContext to PredictState
            playerController.ApplyInput(previousInputs, Time.fixedDeltaTime);
        }
    }

    public void ApplyInput(Inputs input)
    {
        // TODO: Pass RunContext to ApplyInput
        playerController.ApplyInput(input, Time.fixedDeltaTime);
    }

    public Inputs GetInput()
    {
        return playerController.GetInputs();
    }

    protected override void Start()
    {
        playerController = GetComponent<PlayerPlanetController>();
        base.Start();
        NetcodeManager.instance?.client.AddInputful(this, netId, isLocalPlayer);
        NetcodeManager.instance?.server.AddInputful(this, netId);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        NetcodeManager.instance?.client.DeleteInputful(netId);
        NetcodeManager.instance?.server.DeleteInputful(netId);
    }
}
