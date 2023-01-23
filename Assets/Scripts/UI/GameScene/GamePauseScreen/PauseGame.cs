using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseGame : MonoBehaviour
{
    public void FlipFlopPause()
    {
        UIAccessor accessor = FindObjectOfType<UIAccessor>();
        var playerInput = GetComponent<PlayerInput>();
        if (accessor.PauseScreen.activeSelf)
        {
            accessor.PauseScreen.SetActive(false);
            accessor.Pause.SetActive(true);
            accessor.Options.SetActive(false);
            playerInput.currentActionMap = playerInput.actions.FindActionMap("Player");
        } else
        {
            accessor.PauseScreen.SetActive(true);
            accessor.Pause.SetActive(true);
            accessor.Options.SetActive(false);
            playerInput.currentActionMap = playerInput.actions.FindActionMap("UI");
        }
    }

    public void OnPause(InputValue input)
    {
        FlipFlopPause();
    }
}
