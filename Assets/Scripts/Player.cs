using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    Planet[] planets;

    public Rigidbody2D Body {
        get {
            return body;
        }
    }

    public float speed;
    public float reelSpeed = 100;

    private Rigidbody2D body;
    Planet planet;
    float radius;
    float minSpeed;
    public float maxSpeed;

    public int PlayerNumber;
    public PlayerInput ControllerInput = null;

    void Start()
    {
        planets = FindObjectsOfType<Planet>();

        body = GetComponent<Rigidbody2D>();

        planet = getClosestPlanet();
        minSpeed = 0;
        maxSpeed = 350;

        body.velocity = new Vector2(0, 300);
        if (planet != null)
        {
            radius = RotationalPhysics.GetRadius(body, planet.transform.position);
            body.velocity = RotationalPhysics.OnlyTangentialVelocity(body, planet.transform.position);
        }
        else
        {
            radius = 0;
        }
        speed = Mathf.Clamp(body.velocity.magnitude, minSpeed, maxSpeed);
    }

    void Update()
    {
        /*
        if(ControllerInput != null && Input.GetButtonDown(ControllerInput.Button("R"))) {
            Debug.Log("!!!");
        } */

        if (Input.GetKeyDown(KeyCode.Space) || (ControllerInput != null && Input.GetButtonDown(ControllerInput.Button("R"))))
        {
            planet = null;
            radius = 0;
        }
        else if (Input.GetKeyUp(KeyCode.Space) || (ControllerInput != null && Input.GetButtonUp(ControllerInput.Button("R"))))
        {
            planet = getClosestPlanet();
            radius = RotationalPhysics.GetRadius(body, planet.transform.position);
            body.velocity = RotationalPhysics.OnlyTangentialVelocity(body, planet.transform.position);
            speed = Mathf.Clamp(body.velocity.magnitude, minSpeed, maxSpeed);
        }
        if (Input.GetKey(KeyCode.RightArrow) || (ControllerInput != null && Input.GetButton(ControllerInput.Button("A"))))
        {
            speed += 50 * Time.deltaTime;
            speed = Mathf.Clamp(speed, minSpeed, maxSpeed);
        }

        float reelAmount = 0; ;

        if(ControllerInput != null)
        {
            reelAmount = Input.GetAxis(ControllerInput.Axis("Vertical"));
        }

        if (Input.GetKey(KeyCode.DownArrow) || reelAmount > 0)
        {
            if (planet != null)
            {
                radius -= reelSpeed * reelAmount * Time.deltaTime;
            }
        }
    }

    void FixedUpdate()
    {
        if (planet != null)
        {
            RotationalPhysics.RotateAroundPoint(body, planet.transform.position, radius, speed);
        }
        else
        {
            body.velocity = body.velocity.normalized * speed;
        }
    }


    //----------------------------------------------------------------------------------------------

    Planet getClosestPlanet()
    {
        float shortestDistance = Mathf.Infinity;
        Planet closest = null;

        foreach (Planet cur in planets)
        {
            float dist = Vector2.Distance(cur.transform.position, body.position);
            if (dist < shortestDistance)
            {
                shortestDistance = dist;
                closest = cur;
            }
        }
        return closest;
    }
}
