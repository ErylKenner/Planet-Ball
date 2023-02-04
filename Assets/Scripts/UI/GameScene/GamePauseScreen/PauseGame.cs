using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseGame : MonoBehaviour
{
    private UIAccessor accessor;
    private void Start()
    {
        accessor = FindObjectOfType<UIAccessor>();
        if(accessor == null)
        {
            Debug.LogWarning("PauseGame cannot find UIAccessor");
            return;
        }
    }

    public void FlipFlopPause()
    {
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
            accessor.DefaultPauseSelection.Select();
            playerInput.currentActionMap = playerInput.actions.FindActionMap("UI");
        }
    }

    public void OnPause(InputValue input)
    {
        FlipFlopPause();
    }

    public void OnCancel(InputValue input)
    {
        if(accessor.Options.activeSelf)
        {
            accessor.Options.SetActive(false);
            accessor.Pause.SetActive(true);
        } else
        {
            FlipFlopPause();
        }
    }
}
