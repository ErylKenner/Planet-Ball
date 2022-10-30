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

    float timer;


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

        if (timer >= Time.fixedDeltaTime)
        {
            timer = 0;
            if (isClient)
            {
                InputMessage inputMessage = client.Tick(runner, new RunContext { dt = dt });

                Instantiate(playerServerTrail, client.lastServerMessage, Quaternion.identity);
                Instantiate(playerTrail, ((NetworkedPlayer)client.stateMap[(uint)client.localNetId]).transform.position, Quaternion.identity);

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
}
