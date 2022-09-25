using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerPlanetController : NetworkBehaviour
{
    public GameObject Model;

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
    }

    // Start is called before the first frame update
    public void Start()
    {
        //base.Start();
        if (!isLocalPlayer)
        {
            // Disable any components that we don't want to compute since they belong to other players
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isLocalPlayer)
        {
            float xDir = Input.GetAxis("Horizontal");
            float zDir = Input.GetAxis("Vertical");
            Vector3 dir = new Vector3(xDir, 0.0f, zDir);
            gameObject.transform.position += dir * 3 * Time.deltaTime;
        }
    }
}
