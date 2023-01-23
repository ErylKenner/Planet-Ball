using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaveGame : MonoBehaviour
{
    public void Leave()
    {
        CustomRoomManager customRoomManager = FindObjectOfType<CustomRoomManager>();
        customRoomManager.ServerChangeScene(customRoomManager.RoomScene);
    }
}
