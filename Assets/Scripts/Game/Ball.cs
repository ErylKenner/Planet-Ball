using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Ball : NetworkBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        GameObject[] goals = GameObject.FindGameObjectsWithTag("Goal");
        for(int i = 0; i < goals.Length; i++)
        {
            Physics2D.IgnoreCollision(goals[i].GetComponent<Collider2D>(), GetComponent<Collider2D>());
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.tag == "Player")
        {
            //CmdAssignNetworkAuthority(GetComponent<NetworkIdentity>(), collision.transform.GetComponent<NetworkIdentity>());
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdAssignNetworkAuthority(NetworkIdentity ballId, NetworkIdentity clientId)
    {
        Debug.Log("Assigning ball authority to " + clientId.netId);
        ballId.RemoveClientAuthority();
        ballId.AssignClientAuthority(clientId.connectionToClient);
        return;
        //If -> ball has an owner && owner isn't the actual owner
        if (ballId.connectionToClient != null && ballId.connectionToClient != clientId.connectionToClient)
        {
            // Remove authority
            ballId.RemoveClientAuthority();
        }

        //If -> ball has no owner
        if (ballId.connectionToClient == null)
        {
            // Add client as owner
            ballId.AssignClientAuthority(clientId.connectionToClient);
        }
    }

    [ClientRpc]
    public void RpcReset()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.MovePosition(Vector2.zero);
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

}
