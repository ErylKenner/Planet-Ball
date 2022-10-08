using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;
using System.Linq;

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

    static public NetcodePlayer LocalPlayer { get
        {
            return localPlayer;
        }
    }

    private static NetcodePlayer localPlayer = null;

    // Client specific
    public Inputs[] client_input_buffer; // client stores predicted inputs here
    public uint client_tick_number;
    public uint ClientLastRecievedTick
    {
        get
        {
            return (uint)clientLastRecievedTick;
        }
    }
    private int clientLastRecievedTick;
    private float client_timer;

    // Server specific
    public InputBuffer<Inputs> server_input_buffer;
    //public uint server_tick_number;
    public Queue<InputMessage> server_input_msgs;
    private Vector2 movement;


    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        client_timer = 0;

        this.server_input_buffer = new InputBuffer<Inputs>(NetcodeManager.c_client_buffer_size);
        this.client_input_buffer = new Inputs[NetcodeManager.c_client_buffer_size];
        this.server_input_msgs = new Queue<InputMessage>();

        clientLastRecievedTick = -1;
        client_timer = 0;

        if (isLocalPlayer)
        {
            if(localPlayer != null)
            {
                Debug.LogWarning("LocalPlayer was already set");
            }

            localPlayer = this;
        }
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
    }

    [Command]
    protected void CmdQueueInputMessages(InputMessage input_msg)
    {
        server_input_msgs.Enqueue(input_msg);
    }

    public void UpdateClientLastReceivedTick(NetcodeManager.GlobalStateMessage globalState)
    {
        StateMessage playerMessage = globalState.states.ToList().Find(state => state.state.netId == netId);
        clientLastRecievedTick = (int)playerMessage.clientTick;
    }

    public void UpdateClient(float dt)
    {

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
            NetcodeObject[] netcodeObjects = FindObjectsOfType<NetcodeObject>();
            // Store all state
            foreach (NetcodeObject netcodeObject in netcodeObjects)
            {
                netcodeObject.StoreClientState(buffer_slot);
            }
            StoreClientState(buffer_slot);
            NetcodeManager.PrePhysicsStep(this, client_input_buffer[buffer_slot]);
            Physics.Simulate(dt);


            // send input packet to server

            InputMessage input_msg;
            input_msg.start_tick_number = clientLastRecievedTick == -1 ? 0 : ClientLastRecievedTick;
            input_msg.inputs = new List<Inputs>();

            for (uint tick = input_msg.start_tick_number; tick <= client_tick_number; ++tick)
            {
                input_msg.inputs.Add(this.client_input_buffer[tick % NetcodeManager.c_client_buffer_size]);
            }

            CmdQueueInputMessages(input_msg);


            ++client_tick_number;
        }
    }


}
