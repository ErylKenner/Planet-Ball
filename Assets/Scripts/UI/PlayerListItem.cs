using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerListItem : MonoBehaviour
{
    public GameObject NameGui;
    public GameObject ReadyGui;
    public GameObject RemoveGui;

    private CustomRoomPlayer player;

    public void SetPlayer(CustomRoomPlayer player)
    {
        this.player = player;
    }

    // Start is called before the first frame update
    void Start()
    {

    }


    // Update is called once per frame
    void Update()
    {
        if (player.Name == null)
        {
            return;
        }
        NameGui.GetComponent<TMPro.TextMeshProUGUI>().text = player.Name;
        ReadyGui.gameObject.SetActive(player.readyToBegin);
        if ((player.isServer && player.index > 0) || player.isServerOnly){
            RemoveGui.SetActive(true);
            RemoveGui.GetComponent<Button>().onClick.AddListener(disconnectPlayer);
        } else
        {
            RemoveGui.SetActive(false);
        }
    }

    private void disconnectPlayer()
    {
        player.GetComponent<NetworkIdentity>().connectionToClient.Disconnect();
    }
}
