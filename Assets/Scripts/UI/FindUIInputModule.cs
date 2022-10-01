using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

[RequireComponent(typeof(PlayerInput))]
public class FindUIInputModule : MonoBehaviour
{
    private void Awake()
    {
        PlayerInput playerInput = GetComponent<PlayerInput>();
        playerInput.uiInputModule = FindObjectOfType<InputSystemUIInputModule>();
    }
}
