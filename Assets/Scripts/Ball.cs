using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Ball : NetworkBehaviour
{
    public GameObject Model;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        Debug.Log("In ball start");
        CmdSpawnBall();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [Command]
    void CmdSpawnBall()
    {
        RpcSpawnBall();
    }

    [ClientRpc]
    void RpcSpawnBall()
    {
        GameObject obj = Instantiate(Model, transform);
        NetworkServer.Spawn(obj);
    }
}
