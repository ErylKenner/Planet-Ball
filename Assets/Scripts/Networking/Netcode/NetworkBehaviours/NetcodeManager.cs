using ClientServerPrediction;
using Mirror;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NetcodeManager : NetworkBehaviour
{
    public ClientState client = new ClientState();
    public ServerState server = new ServerState();
    private NetcodePhysics runner = new NetcodePhysics();

    public GameObject playerTrail;
    public GameObject playerServerTrail;
    public TMPro.TextMeshProUGUI text;

    public static NetcodeManager instance = null;

    public Ball ball;

    public float TrailExpirationLength = 0.2f;

    public bool ServerDebug = false;
    public Transform TrailParent;

    float timer = 0;
    float frozenTimer = 0;


    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogWarning("There can only be one NetcodeManager");
            Destroy(this.gameObject);
            return;
        }

        if(isServer)
        {
            Ball ballObject = Instantiate(ball);
            NetworkServer.Spawn(ballObject.gameObject);
            FreezeServer(1f);
        }   
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

        if (frozenTimer > 0)
        {
            frozenTimer = Mathf.Clamp(frozenTimer - Time.deltaTime, 0, frozenTimer);
            if (frozenTimer <= 0)
            {
                Debug.Log("Server resuming");
                server.Freeze(false);
            }
        }
        int tickCount = 0;
        while (timer >= Time.fixedDeltaTime)
        {
            tickCount++;
            timer -= Time.fixedDeltaTime;
            if (isClient)
            {
                InputMessage inputMessage = client.Tick(runner, new RunContext { dt = dt }, isClientOnly);

                if (ServerDebug)
                {
                    GameObject serverTrail = Instantiate(playerServerTrail, client.lastServerMessage, Quaternion.identity);
                    serverTrail.name = $"Server {client.lastReceivedTick}";
                    serverTrail.transform.parent = TrailParent ? TrailParent : gameObject.transform;
                    serverTrail.AddComponent<Die>().ExpirationDate = TrailExpirationLength;
                }

                GameObject clientTrail = Instantiate(playerTrail, ((NetcodePlayer)client.stateMap[(uint)client.localNetId]).transform.position, Quaternion.identity);
                clientTrail.name = $"Client {client.tick}";
                clientTrail.transform.parent = TrailParent ? TrailParent : gameObject.transform;
                clientTrail.AddComponent<Die>().ExpirationDate = TrailExpirationLength;

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

        SetDebugText(text);
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

    public void SetDebugText(TMPro.TextMeshProUGUI debugText)
    {
        if(!debugText.gameObject.activeInHierarchy)
        {
            return;
        }

        List<string> debugStrings = new List<string>();

        if(isServer)
        {
            List<uint> serverNetIds = new List<uint>(server.serverStateMap.Keys);
            string serverNetIdsString = string.Join(' ', serverNetIds.Select(netId => netId.ToString()));
            debugStrings.Add($"Server NIDs: {serverNetIdsString}");
        }

        if(isClient)
        {
            List<uint> clientNetIds = new List<uint>(server.serverStateMap.Keys);
            string clientNetIdsString = string.Join(' ', clientNetIds.Select(netId => netId.ToString()));
            debugStrings.Add($"Client NIDs: {clientNetIdsString}");
        }



        debugText.text = string.Join('\n', debugStrings);

    }

    public void ResetState(float freezeTime)
    {
        // TODO: Pass in a Runcontext to get the dt
        float dt = Time.fixedDeltaTime;

        var customRoomManager = FindObjectOfType<CustomRoomManager>();
        customRoomManager.ResetTakenStartPositions();
        var playerTeams = FindObjectsOfType<PlayerTeam>();

        Dictionary<uint, Vector2> startPositions = new Dictionary<uint, Vector2>();
        foreach (var playerTeam in playerTeams)
        {
            Transform startPosition = customRoomManager.GetStartPositionForPlayer(playerTeam.TeamNumber);
            startPositions.Add(playerTeam.netId, startPosition.position);
        }

        server.ResetState(runner, new RunContext { dt = dt }, in startPositions);
        FreezeServer(freezeTime);
    }

    public void FreezeServer(float freezeTime)
    {
        server.Freeze(true);
        frozenTimer = freezeTime;
        Debug.Log($"Server freezing for {freezeTime}s");
    }
}
