using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalScored : NetworkBehaviour
{
    public string Name;
    public Color teamColor;
    public TMPro.TextMeshProUGUI text;
    public float FreezeTime = 3;

    private float timer = 0;
    void Start()
    {
        text.gameObject.SetActive(false);
    }

    void Update()
    {
        if(timer > 0)
        {
            timer -= Time.deltaTime;
            if(timer <= 0)
            {
                timer = 0;
                text.gameObject.SetActive(false);
            }
        }
    }

    [ClientRpc]
    public void RpcPlayerScored(string name)
    {
        Debug.Log(name);
        text.color = teamColor;
        text.gameObject.SetActive(true);
        timer = FreezeTime;

    }
    

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isServer)
        {
            return;
        }
        Ball ball = other.GetComponent<Ball>();
        if (ball != null)
        {
            NetworkedManager.instance.ResetState(FreezeTime);
            RpcPlayerScored(Name);
        }
    }
}
