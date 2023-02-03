using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIControls : MonoBehaviour
{
    private CustomRoomManager customRoomManager;

    private void Start()
    {
        customRoomManager = FindObjectOfType<CustomRoomManager>();

        if(customRoomManager == null)
        {
            Debug.LogWarning("UIControls cannot find CustomRoomManager object");
            return;
        }
    }

    public void OnCancel(InputValue input)
    {
        customRoomManager.Disconnect();
    }
}
