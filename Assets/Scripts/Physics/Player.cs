using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(LineRenderer))]
public class Player : MonoBehaviour
{
    public float DefaultSpeed = 200.0f;
    public float ReelSpeedRatio = 0.75f;
    public Color DisabledTetherColor = new Color(0.5f, 0.5f, 0.5f, 0.1f);
    public Color EnabledTetherColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);

    public float Speed;
    public int PlayerNumber;
    public bool TetherDisabled { get; private set; } = false;
    public Rigidbody2D Body { get; private set; }
    public PlayerInput ControllerInput = null;

    private LineRenderer lineRenderer;
    private Planet[] planets;
    private Planet attachedPlanet = null;
    private float attachedPlanetRadius = 0f;

    private bool reelTether = false;


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
            DetatchTether();
        }
        else if ((Input.GetKeyDown(KeyCode.Space) || (ControllerInput != null && Input.GetButtonDown(ControllerInput.Button("R")))) && !TetherDisabled)
        {
            AttatchTether();
        }
        if (Input.GetKey(KeyCode.DownArrow) || (ControllerInput != null && Input.GetButton(ControllerInput.Button("A"))))
        {
            reelTether = true;
        }
        if (Input.GetKeyUp(KeyCode.DownArrow) || (ControllerInput != null && Input.GetButtonUp(ControllerInput.Button("A"))))
        {
            reelTether = false;
        }
    }

    void FixedUpdate()
    {
        if (attachedPlanet == null || TetherDisabled)
        {
            Body.velocity = Body.velocity.normalized * Speed;
        }
        else if (attachedPlanet != null)
        {
            if (reelTether)
            {
                attachedPlanetRadius -= ReelSpeedRatio * Speed * Time.deltaTime;
                attachedPlanetRadius = Mathf.Clamp(attachedPlanetRadius, attachedPlanet.minDistance, attachedPlanet.maxDistance);
            }
            RotationalPhysics.RotateAroundPoint(Body, attachedPlanet.transform.position, attachedPlanetRadius, Speed, attachedPlanet.minDistance);
        }

        //Display tether
        if (attachedPlanet == null)
        {
            lineRenderer.startColor = lineRenderer.endColor = DisabledTetherColor;
            lineRenderer.SetPosition(0, getClosestPlanet().transform.position);
        }
        else
        {
            lineRenderer.startColor = lineRenderer.endColor = EnabledTetherColor;
            lineRenderer.SetPosition(0, attachedPlanet.transform.position);
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
        foreach (Planet cur in planets)
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
        DetatchTether();
        yield return new WaitForSeconds(time);
        TetherDisabled = false;
        lineRenderer.enabled = true;
        if (Input.GetKey(KeyCode.Space) || (ControllerInput != null && Input.GetButton(ControllerInput.Button("R"))))
        {
            AttatchTether();
        }
    }

    void AttatchTether()
    {
        attachedPlanet = getClosestPlanet();
        attachedPlanetRadius = Vector2.Distance(Body.position, attachedPlanet.transform.position);
        Body.velocity = RotationalPhysics.ConvertToTangentialVelocity(Body, attachedPlanet.transform.position);
    }

    void DetatchTether()
    {
        attachedPlanet = null;
        attachedPlanetRadius = 0;
    }
}
