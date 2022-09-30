using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayPlayerList : MonoBehaviour
{
    public List<CustomRoomPlayer> ConnectedPlayers;

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // TODO: Use event based handling instead of on Update
        ConnectedPlayers = new List<CustomRoomPlayer>(FindObjectsOfType<CustomRoomPlayer>());
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
