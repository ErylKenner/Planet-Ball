using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInputManager))]
public class PlayerJoin : MonoBehaviour
{
    int playerCount = 0;
    public Color[] PlayerColors = new Color[4];
    public Ability[] Abilities = new Ability[4];

    public void OnPlayerJoined(PlayerInput playerInput)
    {
        Player player = playerInput.GetComponent<Player>();
        player.PlayerNumber = playerCount + 1;
        player.ironAbility = (Iron)Abilities[(2 * playerCount) % Abilities.Length];
        player.boostAbility = (Boost)Abilities[(2 * playerCount + 1) % Abilities.Length];

        playerInput.gameObject.name = "Player " + player.PlayerNumber.ToString();
        playerInput.GetComponent<SpriteRenderer>().color = PlayerColors[playerCount % PlayerColors.Length];
        playerCount++;
    }
}
