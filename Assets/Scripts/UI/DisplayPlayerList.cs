using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayPlayerList : MonoBehaviour
{
    public List<CustomRoomPlayer> ConnectedPlayers;
    //Name, Ready State, Remove button (will be X button)
    // Start is called before the first frame update
    void Start()
    {
        if (ConnectedPlayers == null)
        {
            ConnectedPlayers = new List<CustomRoomPlayer>();
        }
    }

    private void OnDisable()
    {
        ConnectedPlayers.Clear();
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < transform.childCount; ++i)
        {
            GameObject cur = transform.GetChild(i).gameObject;
            if (i >= ConnectedPlayers.Count)
            {
                cur.SetActive(false);
                continue;
            }
            cur.SetActive(true);
            cur.GetComponent<PlayerListItem>().SetPlayer(ConnectedPlayers[i]);
        }
    }
}
