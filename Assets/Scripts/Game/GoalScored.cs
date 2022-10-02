using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalScored : NetworkBehaviour
{
    public string Name;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [ClientRpc]
    public void RpcPrintName(string name)
    {
        Debug.Log(Name);
    }
    

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isServer)
        {
            return;
        }
        Ball ball = other.GetComponent<Ball>();
        if (ball != null)
        {
            RpcPrintName(Name);
            //ball.RpcReset();
            
            //Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            //rb.MovePosition(Vector2.zero);
            //rb.velocity = Vector2.zero;
            //rb.angularVelocity = 0f;
        }
    }
}
