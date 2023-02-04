using UnityEngine;
using Mirror;
using kcp2k;
using Mirror.FizzySteam;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/components/network-room-manager
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkRoomManager.html

	See Also: NetworkManager
	Documentation: https://mirror-networking.gitbook.io/docs/components/network-manager
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkManager.html
*/

/// <summary>
/// This is a specialized NetworkManager that includes a networked room.
/// The room has slots that track the joined players, and a maximum player count that is enforced.
/// It requires that the NetworkRoomPlayer component be on the room player objects.
/// NetworkRoomManager is derived from NetworkManager, and so it implements many of the virtual functions provided by the NetworkManager class.
/// </summary>
public class CustomRoomManager : NetworkRoomManager
{
    public GameObject MainMenuGui;
    public GameObject RoomSceneGui;
    public Selectable MainMenuDefaultSelection;
    public Selectable RoomSceneDefaultSelection;

    [Scene]
    public string MainMenuScene;

    public PlayerInput MainMenuInput;

    private HashSet<Transform> takenStartPositions = new HashSet<Transform>();

    public override void Awake()
    {
        FizzySteamworks fizzySteamWorks = GetComponent<FizzySteamworks>();
        KcpTransport kcpTransport = GetComponent<KcpTransport>();

        if (!ConfigManager.UseSteamworks)
        {
            
            if(fizzySteamWorks != null)
            {
                Destroy(fizzySteamWorks);
                transport = kcpTransport;
                GetComponent<NetworkManagerHUD>().enabled = true;
            }
        } else
        {
            
            if (kcpTransport != null)
            {
                Destroy(kcpTransport);
                transport = fizzySteamWorks;
            }
        }

        base.Awake();

    }

    #region Server Callbacks

    /// <summary>
    /// This is called on the server when the server is started - including when a host is started.
    /// </summary>
    public override void OnRoomStartServer()
    {
        ServerChangeScene(RoomScene);
    }

    /// <summary>
    /// This is called on the server when the server is stopped - including when a host is stopped.
    /// </summary>
    public override void OnRoomStopServer() { }

    /// <summary>
    /// This is called on the host when a host is started.
    /// </summary>
    public override void OnRoomStartHost() { }

    /// <summary>
    /// This is called on the host when the host is stopped.
    /// </summary>
    public override void OnRoomStopHost() { }

    /// <summary>
    /// This is called on the server when a new client connects to the server.
    /// </summary>
    /// <param name="conn">The new connection.</param>
    public override void OnRoomServerConnect(NetworkConnectionToClient conn) { }

    /// <summary>
    /// This is called on the server when a client disconnects.
    /// </summary>
    /// <param name="conn">The connection that disconnected.</param>
    public override void OnRoomServerDisconnect(NetworkConnectionToClient conn) { }

    /// <summary>
    /// This is called on the server when a networked scene finishes loading.
    /// </summary>
    /// <param name="sceneName">Name of the new scene.</param>
    public override void OnRoomServerSceneChanged(string sceneName)
    {
        if(sceneName == GameplayScene)
        {
            ResetTakenStartPositions();
        }
    }

    /// <summary>
    /// This allows customization of the creation of the room-player object on the server.
    /// <para>By default the roomPlayerPrefab is used to create the room-player, but this function allows that behaviour to be customized.</para>
    /// </summary>
    /// <param name="conn">The connection the player object is for.</param>
    /// <returns>The new room-player object.</returns>
    public override GameObject OnRoomServerCreateRoomPlayer(NetworkConnectionToClient conn)
    {
        int teamNumber = numPlayers % 2;
        GameObject roomPlayerGameObject = Instantiate(roomPlayerPrefab).gameObject;
        var teamRoomPlayer = roomPlayerGameObject.GetComponent<CustomRoomPlayer>();
        if(teamRoomPlayer)
        {
            teamRoomPlayer.TeamNumber = teamNumber;
        }

        return roomPlayerGameObject;
        
    }


    /// <summary>
    /// This allows customization of the creation of the GamePlayer object on the server.
    /// <para>By default the gamePlayerPrefab is used to create the game-player, but this function allows that behaviour to be customized. The object returned from the function will be used to replace the room-player on the connection.</para>
    /// </summary>
    /// <param name="conn">The connection the player object is for.</param>
    /// <param name="roomPlayer">The room player object for this connection.</param>
    /// <returns>A new GamePlayer object.</returns>
    public override GameObject OnRoomServerCreateGamePlayer(NetworkConnectionToClient conn, GameObject roomPlayer)
    {
        CustomRoomPlayer customRoomPlayer = roomPlayer.GetComponent<CustomRoomPlayer>();
        Transform startPosition;
        if (!customRoomPlayer)
        {
            startPosition = GetStartPosition();
        }
        else
        {
            startPosition = GetStartPositionForPlayer(customRoomPlayer.TeamNumber);
        }

        GameObject playerObject = Instantiate(playerPrefab, startPosition.position, Quaternion.identity);
        var playerTeam = playerObject.GetComponent<PlayerTeam>();
        if (playerTeam)
        {
            playerTeam.TeamNumber = customRoomPlayer.TeamNumber;
        }

        return playerObject;
    }

    public void ResetTakenStartPositions()
    {
        takenStartPositions.Clear();
    }

    public Transform GetStartPositionForPlayer(int teamNumber)
    {
        Transform startPosition = null;
        
        List<Transform> teamSpawnPoints = startPositions.FindAll(transform =>
        {
            var teamNetworkStartPosition = transform.GetComponent<TeamNetworkStartPosition>();
            return teamNetworkStartPosition != null && teamNetworkStartPosition.TeamNumber == teamNumber && !takenStartPositions.Contains(transform);
        });

        if (teamSpawnPoints.Count > 0)
        {
            startPosition = teamSpawnPoints[Random.Range(0, teamSpawnPoints.Count)];
            takenStartPositions.Add(teamSpawnPoints[0]);
        }
        else
        {
            startPosition = GetStartPosition();
        }

        return startPosition;
    }

    /// <summary>
    /// This allows customization of the creation of the GamePlayer object on the server.
    /// <para>This is only called for subsequent GamePlay scenes after the first one.</para>
    /// <para>See OnRoomServerCreateGamePlayer to customize the player object for the initial GamePlay scene.</para>
    /// </summary>
    /// <param name="conn">The connection the player object is for.</param>
    public override void OnRoomServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnRoomServerAddPlayer(conn);
    }

    /// <summary>
    /// This is called on the server when it is told that a client has finished switching from the room scene to a game player scene.
    /// <para>When switching from the room, the room-player is replaced with a game-player object. This callback function gives an opportunity to apply state from the room-player to the game-player object.</para>
    /// </summary>
    /// <param name="conn">The connection of the player</param>
    /// <param name="roomPlayer">The room player object.</param>
    /// <param name="gamePlayer">The game player object.</param>
    /// <returns>False to not allow this player to replace the room player.</returns>
    public override bool OnRoomServerSceneLoadedForPlayer(NetworkConnectionToClient conn, GameObject roomPlayer, GameObject gamePlayer)
    {
        return base.OnRoomServerSceneLoadedForPlayer(conn, roomPlayer, gamePlayer);
    }

    /// <summary>
    /// This is called on the server when all the players in the room are ready.
    /// <para>The default implementation of this function uses ServerChangeScene() to switch to the game player scene. By implementing this callback you can customize what happens when all the players in the room are ready, such as adding a countdown or a confirmation for a group leader.</para>
    /// </summary>
    public override void OnRoomServerPlayersReady()
    {
        base.OnRoomServerPlayersReady();
    }

    /// <summary>
    /// This is called on the server when CheckReadyToBegin finds that players are not ready
    /// <para>May be called multiple times while not ready players are joining</para>
    /// </summary>
    public override void OnRoomServerPlayersNotReady() { }

    #endregion

    #region Client Callbacks

    /// <summary>
    /// This is a hook to allow custom behaviour when the game client enters the room.
    /// </summary>
    public override void OnRoomClientEnter()
    {
        if (NetworkClient.localPlayer == null)
        {
            NetworkClient.AddPlayer();
        }
    }

    /// <summary>
    /// This is a hook to allow custom behaviour when the game client exits the room.
    /// </summary>
    public override void OnRoomClientExit() { }

    /// <summary>
    /// This is called on the client when it connects to server.
    /// </summary>
    public override void OnRoomClientConnect()
    {

    }

    /// <summary>
    /// This is called on the client when disconnected from a server.
    /// </summary>
    public override void OnRoomClientDisconnect() {
        SceneManager.LoadScene(MainMenuScene);
        SceneManager.sceneLoaded += SetMainMenuScene;

    }

    private void SetMainMenuScene(Scene scene, LoadSceneMode mode)
    {
        SetSceneGui(MainMenuScene);
        SceneManager.sceneLoaded -= SetMainMenuScene;
    }

    /// <summary>
    /// This is called on the client when a client is started.
    /// </summary>
    public override void OnRoomStartClient() { }

    /// <summary>
    /// This is called on the client when the client stops.
    /// </summary>
    public override void OnRoomStopClient() { }

    /// <summary>
    /// This is called on the client when the client is finished loading a new networked scene.
    /// </summary>
    public override void OnRoomClientSceneChanged()
    {
        SetSceneGui(networkSceneName);
    }

    /// <summary>
    /// Called on the client when adding a player to the room fails.
    /// <para>This could be because the room is full, or the connection is not allowed to have more players.</para>
    /// </summary>
    public override void OnRoomClientAddPlayerFailed() { }

    #endregion

    #region Optional UI

    public void SetSceneGui(string scene)
    {
        if (scene.Contains(MainMenuScene))
        {
            MainMenuGui.SetActive(true);
            RoomSceneGui.SetActive(false);

            MainMenuDefaultSelection.Select();

            MainMenuInput.enabled = true;
            FindUIInputModule.GrabUIInput(MainMenuInput);
        }
        else if (scene.Contains(RoomScene))
        {
            MainMenuGui.SetActive(false);
            RoomSceneGui.SetActive(true);

            RoomSceneDefaultSelection.Select();

            MainMenuInput.enabled = false;

            // For room re-entry
            PlayerInput playerInput = NetworkClient.localPlayer?.GetComponent<PlayerInput>();
            if(playerInput != null)
            {
                FindUIInputModule.GrabUIInput(playerInput);
            }
        }
        else if (scene.Contains(GameplayScene))
        {
            MainMenuGui.SetActive(false);
            RoomSceneGui.SetActive(false);

            MainMenuInput.enabled = false;        }
    }

    public void ExitGame(bool gameEnd=false)
    {
        // Ignore gameEnd case because server has already changed the scene.
        // We still want to destroy the PlayerInput
        if(networkSceneName != GameplayScene && !gameEnd)
        {
            Debug.LogWarning($"{networkSceneName} is not the GameplayScene {GameplayScene}");
            return;
        }

        Destroy(NetworkClient.localPlayer.GetComponent<PlayerInput>());

        if(!NetworkClient.isHostClient)
        {
            if(gameEnd)
            {
                return;
            }
            Disconnect();
        } else
        {
            ServerChangeScene(RoomScene);
        }
    }

    public void Disconnect()
    {
        StopClient();
        StopHost();
        SetSceneGui(MainMenuScene);
    }


    public override void OnGUI()
    {
        if (!showRoomGUI)
            return;

        if (NetworkServer.active && IsSceneActive(GameplayScene))
        {
            GUILayout.BeginArea(new Rect(Screen.width - 150f, 10f, 140f, 30f));
            if (GUILayout.Button("Return to Room"))
            {
                ServerChangeScene(RoomScene);
            }
            GUILayout.EndArea();
        }

        //if (IsSceneActive(RoomScene))
        //{
        //    GUI.Box(new Rect(200f, 180f, 520f, 150f), "PLAYERS");
        //}
    }

    #endregion
}
