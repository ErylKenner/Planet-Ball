using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetcodeObject : NetworkBehaviour
{

    public ClientState[] client_state_buffer; // client stores predicted moves here
    public ClientState client_error;

    public struct ClientState
    {
        public Vector2 position;
        public Vector2 velocity;
        public float rotation;
        public float angularVelocity;
    }
    // Start is called before the first frame update
    protected virtual void Start()
    {
        this.client_state_buffer = new ClientState[NetcodeManager.c_client_buffer_size];
        this.client_error.position = Vector2.zero;
        this.client_error.velocity = Vector2.zero;
        this.client_error.rotation = 0;
        this.client_error.angularVelocity = 0;
    }
    protected virtual void Update()
    {
    }

    public void StoreClientState(uint buffer_slot)
    {
        Rigidbody2D netcodeRigidbody = GetComponent<Rigidbody2D>();
        client_state_buffer[buffer_slot].position = netcodeRigidbody.position;
        client_state_buffer[buffer_slot].velocity = netcodeRigidbody.velocity;
        client_state_buffer[buffer_slot].rotation = netcodeRigidbody.rotation;
        client_state_buffer[buffer_slot].angularVelocity = netcodeRigidbody.angularVelocity;
    }

}
