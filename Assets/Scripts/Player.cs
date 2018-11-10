using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    Planet[] planets;

    Rigidbody2D body;
    Planet planet;
    float radius;
    float speed;
    float minSpeed;
    float maxSpeed;

    public int PlayerNumber;
    public PlayerInput ControllerInput = null;

    void Start()
    {
        planets = FindObjectsOfType<Planet>();

        body = GetComponent<Rigidbody2D>();
        planet = getClosestPlanet();
        radius = RotationalPhysics.GetRadius(body, planet.transform.position);
        minSpeed = 20;
        maxSpeed = 500;

        body.velocity = new Vector2(0, 300);
        body.velocity = RotationalPhysics.OnlyTangentialVelocity(body, planet.transform.position);
        speed = Mathf.Clamp(body.velocity.magnitude, minSpeed, maxSpeed);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            planet = null;
            radius = 0;
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            planet = getClosestPlanet();
            radius = RotationalPhysics.GetRadius(body, planet.transform.position);
            body.velocity = RotationalPhysics.OnlyTangentialVelocity(body, planet.transform.position);
            speed = Mathf.Clamp(body.velocity.magnitude, minSpeed, maxSpeed);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            speed += 50 * Time.deltaTime;
            speed = Mathf.Clamp(speed, minSpeed, maxSpeed);
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            if (planet != null)
            {
                radius -= 25 * Time.deltaTime;
            }
        }
        if (ControllerInput != null)
        {
            //add controls here
            if (Input.GetButtonDown(ControllerInput.Button("A")))
            {
                Debug.Log(name);
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
