using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using ClientServerPrediction;

[RequireComponent(typeof(Rigidbody2D))]

public class NetworkedObject : NetworkBehaviour, IStateful
{
    protected Rigidbody2D body;

    public State GetState()
    {
        return new State
        {
            position = body.position,
            velocity = body.velocity,
            rotation = body.rotation,
            angularVelocity = body.angularVelocity
        };
    }

    public void SetState(State state)
    {
        body.position = state.position;
        body.velocity = state.velocity;
        body.rotation = state.rotation;
        body.angularVelocity = state.angularVelocity;
    }


    protected void Start()
    {
        body = GetComponent<Rigidbody2D>();
        NetworkedManager.instance.client.AddStateful(this, netId);
        NetworkedManager.instance.server.AddStateful(this, netId);
    }


    protected void OnDestroy()
    {
        NetworkedManager.instance.client.DeleteStateful(netId);
        NetworkedManager.instance.server.DeleteStateful(netId);
    }

}
