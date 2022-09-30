using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TestingInputSystem : MonoBehaviour
{
    private Rigidbody rigidbody;
    private PlayerInputActions playerInputActions;
    private CustomRoomPlayer player;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();

        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
        playerInputActions.Player.Jump.performed += OnJump;
        List<CustomRoomPlayer> players = new List < CustomRoomPlayer >(FindObjectsOfType<CustomRoomPlayer>());
        player = players.Find(player => player.netId == NetworkClient.localPlayer.netId);
        player.input = this;
    }

    private void FixedUpdate()
    {
        Vector2 inputVector = playerInputActions.Player.Movement.ReadValue<Vector2>();
        float speed = 5f;
        rigidbody.AddForce(new Vector3(inputVector.x, 0, inputVector.y) * speed, ForceMode.Force);
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        Debug.Log("Jumping");
        rigidbody.AddForce(Vector3.up * 5f, ForceMode.Impulse);
    }
}
