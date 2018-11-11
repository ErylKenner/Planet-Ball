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
    Color disabledTetherColor;
    Color enabledTetherColor;


    void Start()
    {
        planets = FindObjectsOfType<Planet>();
        body = GetComponent<Rigidbody2D>();

        minSpeed = 50;
        maxSpeed = 250;
        tetherDisabled = false;

        body.velocity = new Vector2(0, 160);
        radius = 0;
        speed = Mathf.Clamp(body.velocity.magnitude, minSpeed, maxSpeed);

        disabledTetherColor = Color.gray;
        disabledTetherColor.a = 0.1f;
        enabledTetherColor = Color.black;

        lineRenderer = gameObject.GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = lineRenderer.endColor = disabledTetherColor;
        lineRenderer.widthMultiplier = 1f;
        lineRenderer.positionCount = 2;


    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space) || (ControllerInput != null && Input.GetButtonUp(ControllerInput.Button("R"))))
        {
            DetatchTether();
        }
        else if ((Input.GetKeyDown(KeyCode.Space) || (ControllerInput != null && Input.GetButtonDown(ControllerInput.Button("R")))) && !tetherDisabled)
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
            RotationalPhysics.RotateAroundPoint(body, planet.transform.position, radius, speed, planet.minDistance);

            lineRenderer.startColor = lineRenderer.endColor = enabledTetherColor;
            lineRenderer.SetPosition(0, planet.transform.position);
            lineRenderer.SetPosition(1, body.position + body.velocity * Time.fixedDeltaTime);
        }
        else
        {
            body.velocity = body.velocity.normalized * speed;

            lineRenderer.startColor = lineRenderer.endColor = disabledTetherColor;
            lineRenderer.SetPosition(0, getClosestPlanet().transform.position);
            lineRenderer.SetPosition(1, body.position + body.velocity * Time.fixedDeltaTime);
        }

        //Fix small or zero velocity
        if (body.velocity.magnitude < minSpeed)
        {
            body.velocity = body.velocity.normalized * minSpeed;
            if (Mathf.Approximately(body.velocity.magnitude, 0))
            {
                body.velocity = new Vector2(1, 1).normalized * minSpeed;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.GetComponent<Player>() != null)
        {
            StartCoroutine(DisableTether(0.75f));
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
        lineRenderer.enabled = false;
        planet = null;
        radius = 0;

        yield return new WaitForSeconds(time);

        tetherDisabled = false;
        lineRenderer.enabled = true;
        //AttatchTether();
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
