using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerPlanetController : NetworkBehaviour
{
    public GameObject Model;
    public float MaxSpeed = 10f;
    private Rigidbody2D rigidbody2d;


    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
    }

    private void Awake()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
    }

    // Start is called before the first frame update
    public void Start()
    {
        //base.Start();
        if (isLocalPlayer)
        {
            //controller = gameObject.GetComponent<CharacterController>();
        }
        else
        {
            // Disable any components that we don't want to compute since they belong to other players
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (isLocalPlayer)
        {
            float xDir = Input.GetAxis("Horizontal");
            float zDir = Input.GetAxis("Vertical");
            Vector2 dir = new Vector2(xDir, zDir);
            Vector2 diff = 5 * Time.deltaTime * dir.normalized;
            //rigidbody2d.MovePosition(rigidbody2d.position + diff);
            rigidbody2d.AddForce(40 * dir);
            rigidbody2d.velocity = Mathf.Clamp(rigidbody2d.velocity.magnitude, 0f, MaxSpeed) * rigidbody2d.velocity.normalized;
        }
    }
}
