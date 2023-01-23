using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeTeam : MonoBehaviour
{
    public void ChangeMyTeam()
    {
        Debug.Log("HELLO");
        var localPlayer = NetworkClient.localPlayer;
        var roomPlayer = localPlayer.GetComponent<CustomRoomPlayer>();
        if(roomPlayer)
        {
            roomPlayer.ChangeTeam();
        } else
        {
            Debug.LogError($"${localPlayer} has no CustomRoomPlayer");
        }
        
    }
}
