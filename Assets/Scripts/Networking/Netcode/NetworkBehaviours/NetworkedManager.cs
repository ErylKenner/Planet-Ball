using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using ClientServerPrediction;

public class NetworkedManager : NetworkBehaviour
{
    public ClientState client = new ClientState();
    public ServerState server = new ServerState();
    private NetworkedPhysics runner = new NetworkedPhysics();

    public GameObject playerTrail;
    public GameObject playerServerTrail;

    public static NetworkedManager instance = null;

    public Ball ball;

    float timer = 0;
    float frozenTimer = 0;


    // Start is called before the first frame update
    void Start()
    {
        if(instance == null)
        {
            instance = this;
        } else
        {
            Debug.LogWarning("There can only be one NetworkedManager");
            Destroy(this.gameObject);
            return;
        }

        Instantiate(ball);
    }

    // Update is called once per frame
    void Update()
    {
        float dt = Time.fixedDeltaTime;
        timer += Time.deltaTime;
        
        if(frozenTimer > 0)
        {
            frozenTimer = Mathf.Clamp(frozenTimer - Time.deltaTime, 0, frozenTimer);
            if(frozenTimer <= 0)
            {
                server.Freeze(false);
            }
        }

        while (timer >= Time.fixedDeltaTime)
        {
            timer -= Time.fixedDeltaTime;
            if (isClient)
            {
                InputMessage inputMessage = client.Tick(runner, new RunContext { dt = dt }, isClientOnly);

                GameObject serverTrail = Instantiate(playerServerTrail, client.lastServerMessage, Quaternion.identity);
                serverTrail.name = $"Server {client.lastReceivedTick}";
                GameObject clientTrail =  Instantiate(playerTrail, ((NetworkedPlayer)client.stateMap[(uint)client.localNetId]).transform.position, Quaternion.identity);
                clientTrail.name = $"Client {client.tick}";

                if (inputMessage != null)
                {
                    CmdSendInputMessage(inputMessage);
                }
            }

            if (isServer)
            {
                StateMessage stateMessage = server.Tick(runner, new RunContext { dt = dt });
                if (stateMessage != null)
                {
                    RpcSendStateMessage(stateMessage);
                }
            }
        }
    }

    [ClientRpc]
    void RpcSendStateMessage(StateMessage stateMessage)
    {
        client.stateMessageQueue.Enqueue(stateMessage);
    }

    [Command(requiresAuthority = false)]
    void CmdSendInputMessage(InputMessage inputMessage)
    {
        server.inputMessageQueue.Enqueue(inputMessage);
    }

    public void ResetState()
    {
        float dt = Time.fixedDeltaTime;
        // Reset all state objects to default -> sent to the clients as StateMessages (freeze)
        server.ResetState(runner, new RunContext { dt = dt });
        // Flush my input queue, input buffer
        // Stop accepting new inputs
        server.Freeze(true);
        // Wait a second
        frozenTimer = 3;
        // Send an unfreeze RPC
    }

}
