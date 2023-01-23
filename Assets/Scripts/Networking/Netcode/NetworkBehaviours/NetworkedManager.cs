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
    public TMPro.TextMeshProUGUI text;

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

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
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

                //GameObject serverTrail = Instantiate(playerServerTrail, client.lastServerMessage, Quaternion.identity);
                //serverTrail.name = $"Server {client.lastReceivedTick}";
                GameObject clientTrail =  Instantiate(playerTrail, ((NetworkedPlayer)client.stateMap[(uint)client.localNetId]).transform.position, Quaternion.identity);
                clientTrail.name = $"Client {client.tick}";

                text.text = $"{client.tick - client.lastReceivedTick}";

                if (inputMessage != null && NetworkClient.ready)
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

    public void ResetState(float freezeTime)
    {
        // TODO: Pass in a Runcontext to get the dt
        float dt = Time.fixedDeltaTime;

        var customRoomManager = FindObjectOfType<CustomRoomManager>();
        customRoomManager.ResetTakenStartPositions();
        var playerTeams = FindObjectsOfType<PlayerTeam>();

        Dictionary<uint, Vector2> startPositions = new Dictionary<uint, Vector2>();
        foreach(var playerTeam in playerTeams)
        {
            Transform startPosition = customRoomManager.GetStartPositionForPlayer(playerTeam.TeamNumber);
            startPositions.Add(playerTeam.netId, startPosition.position);
        }
        
        server.ResetState(runner, new RunContext { dt = dt }, in startPositions);
        server.Freeze(true);
        frozenTimer = freezeTime;
    }
}
