using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class FindUIInputModule : NetworkBehaviour
{
    private void Start()
    {
        Debug.Log("Hello");
        if(isLocalPlayer)
        {
            Debug.Log("There");
            GrabUIInput(GetComponent<PlayerInput>());
        }
    }

    public static void GrabUIInput(PlayerInput playerInput)
    {
        if(playerInput == null)
        {
            Debug.LogWarning("PlayerInput was null, cannot set Input Module");
            return;
        }


        playerInput.uiInputModule = FindObjectOfType<InputSystemUIInputModule>();
    }
}
