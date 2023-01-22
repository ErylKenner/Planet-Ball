using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using ClientServerPrediction;

[RequireComponent(typeof(Rigidbody2D))]

public class NetworkedObject : NetworkBehaviour, IStateful
{
    protected Rigidbody2D body;
    public GameObject model;

    public virtual State GetState()
    {
        return new State
        {
            position = body.position,
            velocity = body.velocity,
            rotation = body.rotation,
            angularVelocity = body.angularVelocity
        };
    }

    public virtual void SetState(State state)
    {
        body.position = state.position;
        body.velocity = state.velocity;
        body.rotation = state.rotation;
        body.angularVelocity = state.angularVelocity;
    }

    public virtual void PredictState(State state)
    {

    }

    public virtual void SmoothState(State oldState, State newState, RunContext runContext, StateError stateError)
    {
        Vector2 newModelPosition = new Vector2(model.transform.position.x, model.transform.position.y);
        Vector2 oldModelPosition = oldState.position + (newModelPosition - newState.position);

        float distance = Vector2.Distance(newState.position, oldModelPosition);

        if (distance < stateError.snapDistance && distance > 0)
        {
            // Gets t% of the way in one second
            float t = 0.99999f;
            model.transform.position = Vector2.Lerp(oldModelPosition, newState.position, 1 - Mathf.Pow(1 - t, runContext.dt));
        }
        else
        {
            model.transform.position = newState.position;
        }
    }


    protected virtual void Start()
    {
        body = GetComponent<Rigidbody2D>();
        NetworkedManager.instance?.client.AddStateful(this, netId);
        NetworkedManager.instance?.server.AddStateful(this, netId);
    }


    protected virtual void OnDestroy()
    {
        NetworkedManager.instance?.client.DeleteStateful(netId);
        NetworkedManager.instance?.server.DeleteStateful(netId);
    }

}
