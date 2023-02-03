using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitRoom : MonoBehaviour
{
    public CustomRoomManager CustomRoomManager;

    private void Start()
    {
        if(CustomRoomManager == null)
        {
            Debug.LogWarning("No CustomRoomManager attached to ExitRoom");
            return;
        }
    }

    public void LeaveRoom()
    {
        CustomRoomManager.Disconnect();
    }
}
