using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(LineRenderer))]
public class Player : MonoBehaviour
{
    public float DefaultSpeed = 200.0f;
    public float ReelSpeed = 120.0f;
    public Color DisabledTetherColor = new Color(0.5f, 0.5f, 0.5f, 0.1f);
    public Color EnabledTetherColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);

    public float Speed;
    public int PlayerNumber;
    public bool TetherDisabled { get; private set; } = false;
    public Rigidbody2D Body { get; private set; } = null;
    public PlayerInput ControllerInput = null;
    public float AttachedPlanetRadius {
        get {
            return attachedPlanetRadius;
        }
        private set {
            if (AttachedPlanet != null)
            {
                attachedPlanetRadius = Mathf.Clamp(value, AttachedPlanet.minDistance, AttachedPlanet.maxDistance);
            }
            else
            {
                attachedPlanetRadius = value;
            }
        }
    }
    public Planet AttachedPlanet { get; private set; } = null;

    private LineRenderer lineRenderer;
    private Planet[] planets;
    private float attachedPlanetRadius = 0.0f;
    public bool ReelTether = false;


    void Awake()
    {
        Body = GetComponent<Rigidbody2D>();
    }


    void Start()
    {
        lineRenderer = gameObject.GetComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = lineRenderer.endColor = DisabledTetherColor;
        lineRenderer.widthMultiplier = 1f;
        lineRenderer.positionCount = 2;

        planets = FindObjectsOfType<Planet>();
        Body.velocity = new Vector2(0, DefaultSpeed);
        Speed = DefaultSpeed;
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space) || (ControllerInput != null && Input.GetButtonUp(ControllerInput.Button("R"))))
        {
            DetachTether();
        }
        else if ((Input.GetKeyDown(KeyCode.Space) || (ControllerInput != null && Input.GetButtonDown(ControllerInput.Button("R")))) && !TetherDisabled)
        {
            AttachTether();
        }
        if (Input.GetKey(KeyCode.DownArrow) || (ControllerInput != null && Input.GetButton(ControllerInput.Button("B"))))
        {
            ReelTether = true;
        }
        if (Input.GetKeyUp(KeyCode.DownArrow) || (ControllerInput != null && Input.GetButtonUp(ControllerInput.Button("B"))))
        {
            ReelTether = false;
        }
    }

    void FixedUpdate()
    {
        if (AttachedPlanet == null || TetherDisabled)
        {
            Body.velocity = Body.velocity.normalized * Speed;
        }
        else if (AttachedPlanet != null)
        {
            if (ReelTether)
            {
                AttachedPlanetRadius -= ReelSpeed * Time.deltaTime;
            }
            RotationalPhysics.RotateAroundPoint(Body, AttachedPlanet.transform.position, AttachedPlanetRadius, Speed, Time.deltaTime);
        }

        //Display tether
        if (AttachedPlanet == null)
        {
            lineRenderer.startColor = lineRenderer.endColor = DisabledTetherColor;
            lineRenderer.SetPosition(0, getClosestPlanet().transform.position);
        }
        else
        {
            lineRenderer.startColor = lineRenderer.endColor = EnabledTetherColor;
            lineRenderer.SetPosition(0, AttachedPlanet.transform.position);
        }
        lineRenderer.SetPosition(1, Body.position + Body.velocity * Time.deltaTime);
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
        foreach (Planet cur in FindObjectsOfType<Planet>())
        {
            float dist = Vector2.Distance(cur.transform.position, Body.position);
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
        TetherDisabled = true;
        lineRenderer.enabled = false;
        DetachTether();
        yield return new WaitForSeconds(time);
        TetherDisabled = false;
        lineRenderer.enabled = true;
        if (Input.GetKey(KeyCode.Space) || (ControllerInput != null && Input.GetButton(ControllerInput.Button("R"))))
        {
            AttachTether();
        }
    }

    public void AttachTether()
    {
        AttachedPlanet = getClosestPlanet();
        AttachedPlanetRadius = Vector2.Distance(Body.position, AttachedPlanet.transform.position);
        Body.velocity = Speed * RotationalPhysics.ConvertToUnitTangentialVelocity(Body.position, Body.velocity, AttachedPlanet.transform.position);
    }

    void DetachTether()
    {
        AttachedPlanet = null;
        AttachedPlanetRadius = 0;
    }
}
