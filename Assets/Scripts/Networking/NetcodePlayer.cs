using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;

public class NetcodePlayer : NetcodeObject
{



    public struct InputMessage
    {
        public uint start_tick_number;
        public List<Inputs> inputs;
    }

    public struct Inputs
    {
        public Vector2 movement;
    }

    public Inputs[] client_input_buffer; // client stores predicted inputs here
    public Inputs[] server_input_buffer;
    public uint server_tick_number;
    public Queue<InputMessage> server_input_msgs;
    private Vector2 movement;


    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        this.server_input_buffer = new Inputs[NetcodeManager.c_client_buffer_size];
        this.client_input_buffer = new Inputs[NetcodeManager.c_client_buffer_size];
        this.server_input_msgs = new Queue<InputMessage>();
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

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (isLocalPlayer && isClient)
        {
            float dt = Time.fixedDeltaTime;
            UpdateClient(dt);
        }
        
    }

    [Command]
    protected void CmdQueueInputMessages(InputMessage input_msg)
    {
        server_input_msgs.Enqueue(input_msg);
    }

    private void UpdateClient(float dt)
    {
        float client_timer = NetcodeManager.client_timer;
        uint client_tick_number = NetcodeManager.client_tick_number;

        client_timer += Time.deltaTime;
        while (client_timer >= dt)
        {
            client_timer -= dt;

            uint buffer_slot = client_tick_number % NetcodeManager.c_client_buffer_size;

            // sample and store inputs for this tick
            Inputs inputs;
            inputs.movement = movement;
            this.client_input_buffer[buffer_slot] = inputs;

            // store state for this tick, then use current state + input to step simulation
            StoreClientState(buffer_slot);
            NetcodeManager.PrePhysicsStep(this, client_input_buffer[buffer_slot]);
            Physics.Simulate(dt);


            // send input packet to server

            InputMessage input_msg;
            input_msg.start_tick_number = NetcodeManager.client_last_received_state_tick;
            input_msg.inputs = new List<Inputs>();

            for (uint tick = input_msg.start_tick_number; tick <= client_tick_number; ++tick)
            {
                input_msg.inputs.Add(this.client_input_buffer[tick % NetcodeManager.c_client_buffer_size]);
            }

            CmdQueueInputMessages(input_msg);


            ++client_tick_number;
        }

        NetcodeManager.client_timer = client_timer;
        NetcodeManager.client_tick_number = client_tick_number;
    }


}
