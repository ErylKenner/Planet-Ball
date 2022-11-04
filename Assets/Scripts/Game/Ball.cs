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
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.tag == "Player")
        {
            
        }
    }

}
