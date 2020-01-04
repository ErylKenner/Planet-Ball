using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(LineRenderer))]
public class Player : MonoBehaviour
{
    Rigidbody2D rb;
    public Rigidbody2D Body {
        get {
            return rb;
        }
    }

    float speed;
    public float Speed {
        get {
            return speed;
        }
        set {
            speed = Mathf.Clamp(value, MinSpeed, MaxSpeed);
        }
    }

    float radius;
    public float Radius {
        get {
            return radius;
        }
        set {
            if(attachedPlanet != null)
            {
                radius = Mathf.Clamp(value, attachedPlanet.minDistance, Mathf.Infinity);
            }
            else
            {
                radius = 0.0f;
            }
        }
    }

    public PlayerInput ControllerInput = null;
    public int PlayerNumber;
    public float ReelSpeed = 0.75f;
    public float MinSpeed = 50.0f;
    public float MaxSpeed = 250.0f;
    public float SpeedChangeRate = 50.0f;
    public Color DisabledTetherColor = Color.grey;
    public Color EnabledTetherColor = Color.black;
    public float CollisionDisableTetherTime = 0.75f;

    LineRenderer lineRenderer;
    Planet attachedPlanet;
    Planet[] planets;
    float initialSpeed = 160.0f;
    bool tetherDisabled = false;

    float reelRate = 0;
    int speedChangeDir = 0;
    bool detachTether = false;
    bool attachTether = false;

    void Start()
    {
        planets = FindObjectsOfType<Planet>();
        rb = GetComponent<Rigidbody2D>();
        lineRenderer = gameObject.GetComponent<LineRenderer>();

        rb.velocity = initialSpeed * Vector2.up;
        Speed = initialSpeed;

        DisabledTetherColor.a = 0.1f;

        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = lineRenderer.endColor = DisabledTetherColor;
        lineRenderer.widthMultiplier = 1f;
        lineRenderer.positionCount = 2;
    }

    void Update()
    {
        if (ControllerInput != null)
        {
            reelRate = ReelSpeed * -Input.GetAxis(ControllerInput.Axis("Vertical"));
            reelRate = Mathf.Clamp(reelRate, 0.0f, Mathf.Infinity);
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            reelRate = ReelSpeed;
        }
        else
        {
            reelRate = 0.0f;
        }

        if (Input.GetKeyUp(KeyCode.Space) || (ControllerInput != null && Input.GetButtonUp(ControllerInput.Button("R"))))
        {
            detachTether = true;
            attachTether = false;
        }
        else if ((Input.GetKey(KeyCode.Space) || (ControllerInput != null && Input.GetButton(ControllerInput.Button("R")))) && !tetherDisabled)
        {
            attachTether = true;
            detachTether = false;
        }

        if (Input.GetKey(KeyCode.RightArrow) || (ControllerInput != null && Input.GetButton(ControllerInput.Button("A"))))
        {
            speedChangeDir = 1;
        }
        else if (Input.GetKey(KeyCode.LeftArrow) || (ControllerInput != null && Input.GetButton(ControllerInput.Button("B"))))
        {
            speedChangeDir = -1;
        }
        else
        {
            speedChangeDir = 0;
        }
    }

    void FixedUpdate()
    {
        //Atach tether
        if (attachTether)
        {
            attachTether = false;
            AttachTether();
        }

        //Detach tether
        if (detachTether)
        {
            detachTether = false;
            DetachTether();
        }

        //Reel tether
        if (reelRate > 0.0f && attachedPlanet != null)
        {
            Radius -= reelRate * Speed * Time.fixedDeltaTime;
        }

        //Change rotate speed
        if (speedChangeDir != 0)
        {
            Speed += SpeedChangeRate * speedChangeDir * Time.fixedDeltaTime;
        }

        //Move player and display the tether
        if (attachedPlanet != null && !tetherDisabled)
        {
            RotationalPhysics.RotateAroundPoint(rb, attachedPlanet.transform.position, Radius, Speed);
            lineRenderer.startColor = lineRenderer.endColor = EnabledTetherColor;
            lineRenderer.SetPosition(0, attachedPlanet.transform.position);
            lineRenderer.SetPosition(1, rb.position + rb.velocity * Time.fixedDeltaTime);
        }
        else
        {
            rb.velocity = rb.velocity.normalized * Speed;
            lineRenderer.startColor = lineRenderer.endColor = DisabledTetherColor;
            lineRenderer.SetPosition(0, getClosestPlanet().transform.position);
            lineRenderer.SetPosition(1, rb.position + rb.velocity * Time.fixedDeltaTime);
        }

        //Fix small or zero velocity
        if (rb.velocity.magnitude < MinSpeed)
        {
            rb.velocity = rb.velocity.normalized * MinSpeed;
            if (Mathf.Approximately(rb.velocity.magnitude, 0))
            {
                rb.velocity = Vector2.up * MinSpeed;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.GetComponent<Player>() != null)
        {
            StartCoroutine(DisableTether(CollisionDisableTetherTime));
        }
    }


    //----------------------------------------------------------------------------------------------

    Planet getClosestPlanet()
    {
        float shortestDistance = Mathf.Infinity;
        Planet closest = null;
        foreach (Planet cur in planets)
        {
            float dist = Vector2.Distance(cur.transform.position, rb.position);
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
        attachedPlanet = null;
        Radius = 0;

        yield return new WaitForSeconds(time);

        tetherDisabled = false;
        lineRenderer.enabled = true;
    }

    void AttachTether()
    {
        if (attachedPlanet != null)
        {
            return;
        }
        attachedPlanet = getClosestPlanet();
        Radius = Vector2.Distance(rb.position, attachedPlanet.transform.position);
        rb.velocity = Speed * RotationalPhysics.GetTangentUnitVector(rb.position, rb.velocity, attachedPlanet.transform.position);
    }

    void DetachTether()
    {
        attachedPlanet = null;
        Radius = 0;
    }
}
