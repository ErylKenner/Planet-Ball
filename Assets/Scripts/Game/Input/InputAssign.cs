using System.Collections.Generic;
using UnityEngine;

public class InputAssign : MonoBehaviour
{

    public static int playerCount;
    public static int currentPlayer = 1;

    public static bool AreRemainingPlayers {
        get {
            return currentPlayer <= playerCount;
        }
    }

    public static Player[] players;

    private List<int> connectedControllerNumbers = new List<int>();

    public static Player GetPlayer(int playerNumber)
    {
        return System.Array.Find(players, x => x.PlayerNumber == playerNumber);
    }

    private void Awake()
    {
        players = FindObjectsOfType<Player>();
        playerCount = players.Length;
    }

    private void Update()
    {
        if (AreRemainingPlayers)
        {
            //increase this value as we add more controllers to the InputManager
            const int currentControllerMax = 2;
            int controllerCount = Mathf.Min(currentControllerMax, Input.GetJoystickNames().Length);
            for (int currentController = 1; currentController <= controllerCount; currentController++)
            {
                if (connectedControllerNumbers.Contains(currentController))
                {
                    continue;
                }

                if (Input.GetButtonDown(PlayerInput.Button("Start", currentController)))
                {
                    Player player = GetPlayer(currentPlayer);

                    if (player != null)
                    {
                        player.ControllerInput = new PlayerInput(currentController);
                        connectedControllerNumbers.Add(currentController);
                        currentPlayer++;
                        Debug.Log("Assigned " + player.name + " to controller " + currentController);
                    }
                    else
                    {
                        Debug.LogError("Controllers are not set up correctly!");
                        return;
                    }
                }
            }
        }

    }
}
