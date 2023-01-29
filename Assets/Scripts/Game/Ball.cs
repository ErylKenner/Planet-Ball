using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Ball : NetworkBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        GameObject[] goals = GameObject.FindGameObjectsWithTag("Goal");
        for(int i = 0; i < goals.Length; i++)
        {
            Physics2D.IgnoreCollision(goals[i].GetComponent<Collider2D>(), GetComponent<Collider2D>());
        }
    }

    // Update is called once per frame
    void Update()
    {
        bool autoWin = ContextManager.instance.AdminManager.AutoWin;
        bool serverFrozen = NetcodeManager.instance.server.frozen;
        if(isServer && autoWin && !serverFrozen)
        {
            GetComponent<Rigidbody2D>().velocity = new Vector2(ContextManager.instance.AdminManager.AutoWinSpeed, 0);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.tag == "Player")
        {
            
        }
    }

}
