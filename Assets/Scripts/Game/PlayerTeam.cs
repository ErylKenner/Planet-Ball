using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTeam : NetworkBehaviour
{
    public MeshRenderer playerMesh;

    [SyncVar(hook = nameof(TeamNumberHook))]
    public int TeamNumber = -1;

    private void Start()
    {
        if(ContextManager.instance.TeamManager.ValidTeam(TeamNumber))
        {
            SetPlayerColor(TeamNumber);
        }
    }

    public void SetPlayerColor(int teamNumber)
    {
        playerMesh.material.color = ContextManager.instance.TeamManager.GetTeam(teamNumber).TeamColor;
    }

    public void TeamNumberHook(int oldTeam, int newTeam)
    {
        SetPlayerColor(newTeam);
    }

}
