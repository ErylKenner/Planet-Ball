using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ContinueGame : MonoBehaviour
{
    public void Continue()
    {
        NetworkClient.localPlayer.GetComponent<PauseGame>().FlipFlopPause();
    }
}
