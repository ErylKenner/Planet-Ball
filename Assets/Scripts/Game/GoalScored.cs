using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalScored : NetworkBehaviour
{
    public string Name;
    public Color teamColor;
    public TMPro.TextMeshProUGUI text;

    private float timer = 0;
    // Start is called before the first frame update
    void Start()
    {
        text.gameObject.SetActive(false);
    }

    // Update is called once per frame
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
    public void RpcPrintName(string name)
    {
        Debug.Log(Name);
        text.color = teamColor;
        text.gameObject.SetActive(true);
        timer = 3;

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

            NetworkedManager.instance.ResetState();
            // Disable 


            RpcPrintName(Name);
            //ball.RpcReset();
            
            //Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            //rb.MovePosition(Vector2.zero);
            //rb.velocity = Vector2.zero;
            //rb.angularVelocity = 0f;
        }
    }
}
