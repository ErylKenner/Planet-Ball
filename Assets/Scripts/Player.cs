using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(LineRenderer))]
public class Player : MonoBehaviour
{
    //------PUBLIC------
    public Rigidbody2D Body {
        get {
            return body;
        }
    }
    public PlayerInput ControllerInput = null;
    public float speed;
    public float maxSpeed;
    public float reelSpeed = 0.3f;
    public int PlayerNumber;



    //------PRIVATE-----
    Rigidbody2D body;
    LineRenderer lineRenderer;
    Planet planet;
    Planet[] planets;
    float radius;
    float minSpeed;
    bool tetherDisabled;


    void Start()
    {
        planets = FindObjectsOfType<Planet>();
        body = GetComponent<Rigidbody2D>();

        minSpeed = 50;
        maxSpeed = 250;
        tetherDisabled = false;

        body.velocity = new Vector2(0, 120);
        radius = 0;
        speed = Mathf.Clamp(body.velocity.magnitude, minSpeed, maxSpeed);

        lineRenderer = gameObject.GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = lineRenderer.endColor = new Color(200, 200, 200);
        lineRenderer.widthMultiplier = 1f;
        lineRenderer.positionCount = 2;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKey(KeyCode.Space) || (ControllerInput != null && Input.GetButtonUp(ControllerInput.Button("R"))))
        {
            DetatchTether();
        }
        else if ((Input.GetKeyUp(KeyCode.Space) || (ControllerInput != null && Input.GetButtonDown(ControllerInput.Button("R")))) && !tetherDisabled)
        {
            AttatchTether();
        }
        if (Input.GetKey(KeyCode.RightArrow) || (ControllerInput != null && Input.GetButton(ControllerInput.Button("A"))))
        {
            speed += 50 * Time.deltaTime;
            speed = Mathf.Clamp(speed, minSpeed, maxSpeed);
        }

        float reelPercent = 0;

        if (ControllerInput != null)
        {
            reelPercent = -Input.GetAxis(ControllerInput.Axis("Vertical"));
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            reelPercent = 1;
        }
        if (reelPercent > 0)
        {
            if (planet != null)
            {
                radius -= reelPercent * reelSpeed * speed * Time.deltaTime;
                radius = Mathf.Clamp(radius, planet.minDistance, Mathf.Infinity);
            }
        }
    }

    void FixedUpdate()
    {
        if (planet != null && !tetherDisabled)
        {
            lineRenderer.enabled = true;
            RotationalPhysics.RotateAroundPoint(body, planet.transform.position, radius, speed, planet.minDistance);

            Vector2 endPos = body.position + body.velocity * Time.fixedDeltaTime;
            //float distance = Vector2.Distance(endPos, planet.transform.position);
            //endPos = (Vector2)planet.transform.position + (endPos - (Vector2)planet.transform.position) * (distance - 0.08f * body.transform.localScale.x) / distance;

            lineRenderer.SetPosition(0, planet.transform.position);
            lineRenderer.SetPosition(1, endPos);
        }
        else
        {
            lineRenderer.enabled = false;
            body.velocity = body.velocity.normalized * speed;
        }
        if (body.velocity.magnitude < minSpeed)
        {
            body.velocity = body.velocity.normalized * minSpeed;
            if (Mathf.Approximately(body.velocity.magnitude, 0))
            {
                body.velocity = new Vector2(1, 1).normalized * minSpeed;
            }
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

    IEnumerator DisableTether(float time)
    {
        tetherDisabled = true;
        planet = null;
        radius = 0;

        yield return new WaitForSeconds(time);

        tetherDisabled = false;
        AttatchTether();
    }

    void AttatchTether()
    {
        planet = getClosestPlanet();
        radius = RotationalPhysics.GetRadius(body, planet.transform.position);
        body.velocity = RotationalPhysics.ConvertToTangentialVelocity(body, planet.transform.position);
        speed = Mathf.Clamp(body.velocity.magnitude, minSpeed, maxSpeed);
    }

    void DetatchTether()
    {
        planet = null;
        radius = 0;
    }
}
