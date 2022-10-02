using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
using static NetcodePlayer;
using static NetcodeObject;
using System.Linq;

public class NetcodeManager : NetworkBehaviour
{

    public struct State
    {
        public uint netId;
        public ClientState state;
    }
    public struct StateMessage
    {
        public uint tick_number;
        public State[] states;
    }

    private Queue<StateMessage> client_state_msgs;

    // common stuff
    //public Transform local_player_camera_transform;
    //public float player_movement_impulse;
    //public float player_jump_y_threshold;
    //public GameObject client_player;
    //public GameObject client_ball;
    //public GameObject smoothed_client_player;
    //public GameObject smoothed_client_ball;
    //public GameObject server_player;
    //public GameObject server_ball;
    //public GameObject server_display_player;
    //public GameObject server_display_ball;
    //public GameObject proxy_player;
    //public float latency = 0.1f;
    //public float packet_loss_chance = 0.05f;

    // client specific
    //public bool client_enable_corrections = true;
    //public bool client_correction_smoothing = true;
    //public bool client_send_redundant_inputs = true;

    public static float client_timer;
    public static uint client_tick_number;
    public static uint client_last_received_state_tick;
    public static readonly uint c_client_buffer_size = 1024;



    // server specific
    public uint server_snapshot_rate;
    private uint server_tick_number;
    private uint server_tick_accumulator;
    
    //private Scene server_scene, client_scene;
    //private PhysicsScene server_physics_scene, client_physics_scene;

    private void Start()
    {
        this.client_state_msgs = new Queue<StateMessage>();

        this.server_tick_number = 0;
        this.server_tick_accumulator = 0;
        

        //server_scene = SceneManager.LoadScene("physics_scene", new LoadSceneParameters() { loadSceneMode = LoadSceneMode.Additive, localPhysicsMode = LocalPhysicsMode.Physics3D });
        //client_scene = SceneManager.LoadScene("physics_scene", new LoadSceneParameters() { loadSceneMode = LoadSceneMode.Additive, localPhysicsMode = LocalPhysicsMode.Physics3D });

        //server_physics_scene = server_scene.GetPhysicsScene();
        //client_physics_scene = client_scene.GetPhysicsScene();

        //SceneManager.MoveGameObjectToScene(client_player, client_scene);
        //SceneManager.MoveGameObjectToScene(client_ball, client_scene);
        //SceneManager.MoveGameObjectToScene(server_player, server_scene);
        //SceneManager.MoveGameObjectToScene(server_ball, server_scene);
    }

    private void Update()
    {
        float dt = Time.fixedDeltaTime;
        if (isClientOnly)
        {
            UpdateClient(dt);
        }

        if(isServer)
        {
            UpdateServer(dt);
        }
        
    }

    private void UpdateServer(float dt)
    {
        //uint server_tick_number = this.server_tick_number;
        //uint server_tick_accumulator = this.server_tick_accumulator;

        NetcodePlayer[] netcodePlayers = FindObjectsOfType<NetcodePlayer>();

        foreach (NetcodePlayer netcodePlayer in netcodePlayers)
        {

            while (netcodePlayer?.server_input_msgs.Count > 0)
            {
                InputMessage input_msg = netcodePlayer.server_input_msgs.Dequeue();

                // message contains an array of inputs, calculate what tick the final one is
                uint max_tick = input_msg.start_tick_number + (uint)input_msg.inputs.Count - 1;

                // if that tick is greater than or equal to the current tick we're on, then it
                // has inputs which are new
                if (max_tick >= netcodePlayer.server_tick_number)
                {
                    // there may be some inputs in the array that we've already had,
                    // so figure out where to start
                    uint start_i = netcodePlayer.server_tick_number > input_msg.start_tick_number ? (netcodePlayer.server_tick_number - input_msg.start_tick_number) : 0;

                    // run through all relevant inputs, and step player forward
                    for (int i = (int)start_i; i < input_msg.inputs.Count; ++i)
                    {

                        netcodePlayer.server_input_buffer[netcodePlayer.server_tick_number % c_client_buffer_size] = input_msg.inputs[i];
                        ++netcodePlayer.server_tick_number;
                    }
                }
            }

        }


        // TODO: Find a smarter way to progress the server
        if (netcodePlayers.Length > 0)
        {
            uint new_server_tick_number = netcodePlayers.Min(player => player.server_tick_number);
            for (int i = (int)server_tick_number; i < new_server_tick_number; ++i)
            {

                foreach (NetcodePlayer netcodePlayer in netcodePlayers)
                {
                    NetcodeManager.PrePhysicsStep(netcodePlayer, netcodePlayer.server_input_buffer[i % NetcodeManager.c_client_buffer_size]);
                }

                //Debug.Log("Running physics");
                Physics.Simulate(dt);
                ++server_tick_accumulator;
            }

            // TODO: Seperate server display
            //this.server_display_player.transform.position = server_rigidbody.position;
            //this.server_display_player.transform.rotation = server_rigidbody.rotation;

            server_tick_number = new_server_tick_number;
        }

        if (server_tick_accumulator >= this.server_snapshot_rate)
        {
            server_tick_accumulator = 0;

            StateMessage state_msg;
            state_msg.tick_number = server_tick_number;
            NetcodeObject[] netcodeObjects = FindObjectsOfType<NetcodeObject>();
            state_msg.states = new State[netcodeObjects.Length];

            for (int i = 0; i < netcodeObjects.Length; i++)
            {
                Rigidbody2D rigidbody = netcodeObjects[i].GetComponent<Rigidbody2D>();
                state_msg.states[i].netId = netcodeObjects[i].netId;
                state_msg.states[i].state.position = rigidbody.position;
                state_msg.states[i].state.rotation = rigidbody.rotation;
                state_msg.states[i].state.velocity = rigidbody.velocity;
                state_msg.states[i].state.angularVelocity = rigidbody.angularVelocity;
            }

            this.RpcQueueClientState(state_msg);
        }
    }

    [ClientRpc]
    void RpcQueueClientState(StateMessage state_msg)
    {
        client_state_msgs.Enqueue(state_msg);
    }

    private void UpdateClient(float dt)
    {
        NetcodeObject[] netcodeObjects = FindObjectsOfType<NetcodeObject>();
        Dictionary<uint, NetcodeObject> netIdToNetcodeObject = new Dictionary<uint, NetcodeObject>();

        foreach (NetcodeObject netcodeObject in netcodeObjects)
        {
            netIdToNetcodeObject.Add(netcodeObject.netId, netcodeObject);
        }


        if (client_state_msgs.Count > 0)
        {
            StateMessage state_msg = this.client_state_msgs.Dequeue();
            while (client_state_msgs.Count > 0) // make sure if there are any newer state messages available, we use those instead
            {
                state_msg = this.client_state_msgs.Dequeue();
            }

            Dictionary<uint, State> netIdToState = new Dictionary<uint, State>();

            client_last_received_state_tick = state_msg.tick_number;
            uint buffer_slot = state_msg.tick_number % c_client_buffer_size;

            // TODO: Smartly correct instead of correction on any
            List<Vector2> position_errors = new List<Vector2>();
            List<float> rotation_errors = new List<float>();
            foreach (State state in state_msg.states)
            {
                netIdToState.Add(state.netId, state);
                NetcodeObject currentObject = netIdToNetcodeObject[state.netId];
                Debug.Log(currentObject.gameObject.name);

                Vector2 position_error = state.state.position - currentObject.client_state_buffer[buffer_slot].position;
                Debug.Log(position_error);
                position_errors.Add(position_error);
                float rotation_error = state.state.rotation - currentObject.client_state_buffer[buffer_slot].rotation;
                rotation_errors.Add(rotation_error);
            }

            bool doCorrection = (position_errors.Any(error => error.sqrMagnitude > 0.0000001f)); //||
                //rotation_errors.Any(error => Mathf.Abs(error) > 0.00001f));



            if (doCorrection)
            {
                Debug.Log("Correcting for error at tick " + state_msg.tick_number + " (rewinding " + (client_tick_number - state_msg.tick_number) + " ticks)");
                Dictionary<NetcodeObject, Vector2> previousPositions = new Dictionary<NetcodeObject, Vector2>();
                Dictionary<NetcodeObject, float> previousRotations = new Dictionary<NetcodeObject, float>();

                foreach (NetcodeObject netcodeObject in netcodeObjects)
                {
                    Rigidbody2D netcodeRigidbody2D = netcodeObject.GetComponent<Rigidbody2D>();
                    // capture the current predicted pos for smoothing
                    Vector2 prev_pos = netcodeRigidbody2D.position + netcodeObject.client_error.position;
                    previousPositions.Add(netcodeObject, prev_pos);
                    float prev_rot = netcodeRigidbody2D.rotation * netcodeObject.client_error.rotation;
                    previousRotations.Add(netcodeObject, prev_rot);
                    State currentState = netIdToState[netcodeObject.netId];

                    // rewind & replay
                    netcodeRigidbody2D.position = currentState.state.position;
                    netcodeRigidbody2D.rotation = currentState.state.rotation;
                    netcodeRigidbody2D.velocity = currentState.state.velocity;
                    netcodeRigidbody2D.angularVelocity = currentState.state.angularVelocity;

                }

                uint rewind_tick_number = state_msg.tick_number;
                while (rewind_tick_number < client_tick_number)
                {
                    buffer_slot = rewind_tick_number % c_client_buffer_size;

                    foreach (NetcodeObject netcodeObject in netcodeObjects)
                    {
                        netcodeObject.StoreClientState(buffer_slot);
                        if (netcodeObject is NetcodePlayer)
                        {
                            NetcodePlayer netcodePlayer = (NetcodePlayer)netcodeObject;
                            PrePhysicsStep(netcodePlayer, netcodePlayer.client_input_buffer[buffer_slot]);
                        }
                    }
                    Physics.Simulate(dt);

                    ++rewind_tick_number;
                }

                foreach (NetcodeObject netcodeObject in netcodeObjects)
                {
                    Rigidbody2D netcodeRigidbody2D = netcodeObject.GetComponent<Rigidbody2D>();
                    Vector2 prev_pos = previousPositions[netcodeObject];
                    float prev_rot = previousRotations[netcodeObject];
                    // if more than 2ms apart, just snap
                    if ((prev_pos - netcodeRigidbody2D.position).sqrMagnitude >= 4.0f)
                    {
                        netcodeObject.client_error.position = Vector2.zero;
                        netcodeObject.client_error.rotation = 0f;
                    }
                    else
                    {
                        netcodeObject.client_error.position = prev_pos - netcodeRigidbody2D.position;
                        netcodeObject.client_error.rotation = prev_rot - netcodeRigidbody2D.rotation;
                    }
                }
            }

            // Smoothing
            foreach (NetcodeObject netcodeObject in netcodeObjects)
            {
                // TODO: Turn on smoothing
                //netcodeObject.client_error.position *= 0.9f;
                //netcodeObject.client_error.rotation *= 0.9f;

                // TODO: Use seperate object for the smoothed position
                //this.smoothed_client_player.transform.position = client_rigidbody.position + this.client_pos_error;
                //this.smoothed_client_player.transform.rotation = client_rigidbody.rotation * this.client_rot_error;

                //Rigidbody2D netcodeRigidbody2D = netcodeObject.GetComponent<Rigidbody2D>();

                //netcodeRigidbody2D.position = netcodeRigidbody2D.position + netcodeObject.client_error.position;
                //netcodeRigidbody2D.rotation = netcodeRigidbody2D.rotation + netcodeObject.client_error.rotation;
            }



        }
    }

    public static void PrePhysicsStep(NetcodePlayer player, Inputs inputs)
    {
        player.GetComponent<PlayerPlanetController>().Move(inputs.movement);
    }
}
