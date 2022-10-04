using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;

public class PlayerInputState
{
    public bool AttachTether = false;
    public bool WindTether = false;
    public bool UnwindTether = false;
    public bool SpeedBoost = false;
    public bool Heavy = false;
    public PlayerInputState(bool attachTether, bool windTether, bool unwindTether, bool speedBoost, bool heavy)
    {
        AttachTether = attachTether;
        WindTether = windTether;
        UnwindTether = unwindTether;
        SpeedBoost = speedBoost;
        Heavy = heavy;
    }
}

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerPlanetController : NetworkBehaviour
{
    // ----- Player State Needed for NetcodeManager Clientside-prediction Rewind -----
    // Other required state included in RigidBody2D:
    //  - Vector2 position
    //  - Vector2 velocity
    //  - float rotation
    //  - float angular velocity
    public bool IsTethered = false;
    public bool IsWindTether = false;
    public bool IsUnwindTether = false;
    public bool IsSpeedBoost = false;
    public bool IsHeavy = false;
    public float OrbitRadius = 0;
    public Vector2 CenterPoint = Vector2.zero;
    public float Speed = 12.0f;
    public float CurSpeedBoostCooldown = 0f;
    public float CurHeavyCooldown = 0f;

    // Const attributes - not state
    public float WIND_TETHER_RATIO = 0.11f;
    public float MIN_RADIUS = 0.5f;
    public float MAX_RADIUS = 9f;
    public float MIN_SPEED = 12f;
    public float MAX_SPEED = 40f;
    public float SPEED_FALLOFF = 0.62f;
    public float SPEED_BOOST_COOLDOWN = 10f;
    public float HEAVY_COOLDOWN = 10f;
    public float HEAVY_DURATION = 4f;
    public float HEAVY_MASS = 20f;

    // For testing. Input system callbacks set these which are then read in FixedUpdate
    private bool _attachTether = false;
    private bool _windTether = false;
    private bool _unwindTether = false;
    private bool _speedBoost = false;
    private bool _heavy = false;

    private Rigidbody2D body;


    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    // Start is called before the first frame update
    public void Start()
    {
        if (!isLocalPlayer)
        {
            // Disable any components that we don't want to compute since they belong to other players
            GetComponent<PlayerInput>().enabled = false;
        }
    }

    private void FixedUpdate()
    {
        PlayerInputState input = new PlayerInputState(_attachTether, _windTether, _unwindTether, _speedBoost, _heavy);
        PhysicsPreStep(input, Time.fixedDeltaTime);
    }

    public void PhysicsPreStep(PlayerInputState input, float dt)
    {
        SetStateFromInput(input, dt);
        SetRigidBodyVelocity(dt);
    }

    private void SetStateFromInput(PlayerInputState input, float dt)
    {
        bool wasTethered = IsTethered;
        IsTethered = (input == null && IsTethered) || (input != null && input.AttachTether);
        IsWindTether = (input == null && IsWindTether) || (input != null && input.WindTether);
        IsUnwindTether = (input == null && IsUnwindTether) || (input != null && input.UnwindTether);
        IsSpeedBoost = (input == null && IsSpeedBoost) || (input != null && input.SpeedBoost);
        IsHeavy = (input == null && IsHeavy) || (input != null && input.Heavy);
        if (IsTethered)
        {
            if (!wasTethered)
            {
                // This is the first tick where the tether is attached. Find the nearest planet to attach to
                Planet nearestPlanet = NearestPlanet();
                if (nearestPlanet == null)
                {
                    // No Planet could be tethered. Set IsTethered to false and try again next tick
                    IsTethered = false;
                    OrbitRadius = 0;
                }
                else
                {
                    CenterPoint = nearestPlanet.transform.position;
                    OrbitRadius = Vector2.Distance(body.position, CenterPoint);
                }
            }
            if (IsWindTether)
            {
                // Wind in the same orbit shape regardless of speed via exponential falloff of the radius
                float windRate = WIND_TETHER_RATIO * OrbitRadius * Speed;
                OrbitRadius -= windRate * dt;
            }
            if (IsUnwindTether)
            {
                // Unwind in the same orbit shape regardless of speed via exponential falloff of the radius
                float windRate = WIND_TETHER_RATIO * OrbitRadius * Speed;
                // Double the unwind speed vs the wind speed since it feels better
                OrbitRadius += 2 * windRate * dt;
            }
        }
        else
        {
            CenterPoint = Vector2.zero;
            OrbitRadius = 0;
        }
        OrbitRadius = Mathf.Clamp(OrbitRadius, MIN_RADIUS, MAX_RADIUS);

        // Speed exponential falloff
        Speed -= SPEED_FALLOFF * (Speed - MIN_SPEED + 1) * dt;
        if (IsSpeedBoost && CurSpeedBoostCooldown <= 0f)
        {
            CurSpeedBoostCooldown = SPEED_BOOST_COOLDOWN;
            Speed = MAX_SPEED;
        }
        Speed = Mathf.Clamp(Speed, MIN_SPEED, MAX_SPEED);

        // Set mass
        if (IsHeavy && CurHeavyCooldown <= 0)
        {
            CurHeavyCooldown = HEAVY_COOLDOWN;
            body.mass = HEAVY_MASS;
        } else if (CurHeavyCooldown <= HEAVY_COOLDOWN - HEAVY_DURATION)
        {
            body.mass = 1;
        }

        // Decrement cooldown timers
        CurSpeedBoostCooldown = Mathf.Clamp(CurSpeedBoostCooldown - dt, 0, Mathf.Infinity);
        CurHeavyCooldown = Mathf.Clamp(CurHeavyCooldown - dt, 0, Mathf.Infinity);
    }

    private void SetRigidBodyVelocity(float dt)
    {
        if (!IsTethered)
        {
            body.velocity = Speed * body.velocity.normalized;
            return;
        }
        Vector2 diff = body.position - CenterPoint;
        float rotationDirection = Mathf.Sign(Vector2.Dot(new Vector2(diff.y, -diff.x), body.velocity));
        if (rotationDirection == 0)
        {
            rotationDirection = 1;
        }
        if (Mathf.Abs(diff.magnitude - OrbitRadius) > Speed * dt * 0.75f)
        {
            //Too large a distance to make in one step. Go towards new radius at 45 deg angle
            Vector2 tangentUnit = new Vector2(diff.y, -diff.x).normalized * rotationDirection;
            Vector2 radialChangeUnit = (diff.normalized * OrbitRadius - diff).normalized; // TODO: Could this just be -diff.normalized ?
            body.velocity = Speed * (tangentUnit + radialChangeUnit).normalized;
        }
        else
        {
            //Can make the radius change. So, solve for the new angle
            float currentAngle = Mathf.Atan2(diff.y, diff.x);
            float deltaAngle = (diff.magnitude * diff.magnitude + OrbitRadius * OrbitRadius - Mathf.Pow(Speed * dt, 2)) / (2 * diff.magnitude * OrbitRadius);
            deltaAngle = Mathf.Acos(deltaAngle);
            float newAngle = currentAngle + deltaAngle * -rotationDirection;
            Vector2 newPosition = CenterPoint + OrbitRadius * new Vector2(Mathf.Cos(newAngle), Mathf.Sin(newAngle));
            body.velocity = Speed * (newPosition - body.position).normalized;
        }
    }

    public Planet NearestPlanet()
    {
        float shortestDistance = Mathf.Infinity;
        Planet closest = null;
        foreach (Planet cur in FindObjectsOfType<Planet>())
        {
            float dist = Vector2.Distance(cur.transform.position, body.position);
            if (dist < shortestDistance && dist <= MAX_RADIUS)
            {
                shortestDistance = dist;
                closest = cur;
            }
        }
        return closest;
    }


    public void OnAttachTether(InputValue input)
    {
        _attachTether = input.isPressed;
    }

    public void OnWindTether(InputValue input)
    {
        _windTether = input.isPressed;
    }
    public void OnUnwindTether(InputValue input)
    {
        _unwindTether = input.isPressed;
    }

    public void OnSpeedBoost(InputValue input)
    {
        _speedBoost = input.isPressed;
    }

    public void OnHeavy(InputValue input)
    {
        _heavy = input.isPressed;
    }

}
