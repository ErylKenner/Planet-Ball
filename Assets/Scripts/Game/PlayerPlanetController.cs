using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;
using ClientServerPrediction;


[RequireComponent(typeof(Rigidbody2D))]
public class PlayerPlanetController : NetworkBehaviour
{
    public PlayerState playerState = new PlayerState();

    // Const attributes - not state
    public float WIND_TETHER_RATIO = 0.12f;
    public float UNWIND_TETHER_RATIO = 0.29f;
    public float MIN_RADIUS = 1f;
    public float MAX_RADIUS = 9f;
    public float MIN_SPEED = 12f;
    public float MAX_SPEED = 35f;
    public float SPEED_FALLOFF = 0.85f;
    public float SPEED_BOOST_RAMP = 0.4f;
    public float SPEED_BOOST_COOLDOWN = 2f;
    public float HEAVY_COOLDOWN = 0.5f;
    public float HEAVY_DURATION = 1f;
    public float HEAVY_MASS = 10f;
    public float GAS_INCREASE_TIME = 20f;
    public float GAS_DRAIN_TIME = 3f;
    public float BOOST_SPEED_MINIMUM = 20f;
    public float SPEED_MASS_MULTIPLIER = 0.5f;

    // For testing. Input system callbacks set these which are then read in FixedUpdate
    private bool _attachTether = false;
    private bool _windTether = false;
    private bool _unwindTether = false;
    private bool _speedBoost = false;
    private bool _kick = false;

    private Rigidbody2D body;


    public Inputs GetInputs()
    {
        return new Inputs
        {
            AttachTether = _attachTether,
            WindTether = _windTether,
            UnwindTether = _unwindTether,
            SpeedBoost = _speedBoost,
            Kick = _kick
        };
    }


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
        //PlayerInputState input = new PlayerInputState(_attachTether, _windTether, _unwindTether, _speedBoost, _kick);
        //PhysicsPreStep(input, Time.fixedDeltaTime);
    }

    public void ApplyInput(Inputs input, float dt)
    {
        SetStateFromInput(input, dt);
        SetRigidBodyVelocity(dt);
    }

    private void SetStateFromInput(Inputs input, float dt)
    {
        bool wasInputTethered = playerState.InputIsTethered;
        bool wasInputSpeedBoost = playerState.InputIsSpeedBoost;
        playerState.InputIsTethered = (input == null && playerState.InputIsTethered) || (input != null && input.AttachTether);
        playerState.InputIsWindTether = (input == null && playerState.InputIsWindTether) || (input != null && input.WindTether);
        playerState.InputIsUnwindTether = (input == null && playerState.InputIsUnwindTether) || (input != null && input.UnwindTether);
        playerState.InputIsSpeedBoost = (input == null && playerState.InputIsSpeedBoost) || (input != null && input.SpeedBoost);
        playerState.InputIsKick = (input == null && playerState.InputIsKick) || (input != null && input.Kick);


        SetSpeed();
        HandleTether(wasInputTethered, dt);
        HandleSpeedBoost(wasInputSpeedBoost, dt);
        //HandleKick(dt);
    }

    private void SetSpeed()
    {
        playerState.Speed = Mathf.Clamp(body.velocity.magnitude, MIN_SPEED, MAX_SPEED);
        float speedRatio = (playerState.Speed - MIN_SPEED) / (MAX_SPEED - MIN_SPEED);
        body.mass = 1 + speedRatio * SPEED_MASS_MULTIPLIER;
    }

    private void HandleTether(bool wasInputTethered, float dt)
    {
        if (playerState.InputIsTethered)
        {
            if (!wasInputTethered)
            {
                // This is the first tick where the tether is attached. Find the nearest planet to attach to
                Planet nearestPlanet = NearestPlanet();
                if (nearestPlanet == null)
                {
                    // No Planet could be tethered. Set IsTethered to false and try again next tick
                    playerState.InputIsTethered = false;
                    playerState.OrbitRadius = 0;
                }
                else
                {
                    playerState.CenterPoint = nearestPlanet.transform.position;
                    playerState.OrbitRadius = Vector2.Distance(body.position, playerState.CenterPoint);
                }
            }
            if (playerState.InputIsWindTether)
            {
                // Wind in the same orbit shape regardless of speed via exponential falloff of the radius
                float windRate = WIND_TETHER_RATIO * playerState.OrbitRadius * playerState.Speed;
                playerState.OrbitRadius -= windRate * dt;
            }
            if (playerState.InputIsUnwindTether)
            {
                // Unwind in the same orbit shape regardless of speed via exponential falloff of the radius
                float windRate = UNWIND_TETHER_RATIO * playerState.OrbitRadius * playerState.Speed;
                playerState.OrbitRadius += windRate * dt;
            }
        }
        else
        {
            playerState.CenterPoint = Vector2.zero;
            playerState.OrbitRadius = 0;
        }
        playerState.OrbitRadius = Mathf.Clamp(playerState.OrbitRadius, MIN_RADIUS, MAX_RADIUS);
    }

    private void HandleSpeedBoost(bool wasInputSpeedBoost, float dt)
    {
        float gasDrainPerTick = dt / GAS_DRAIN_TIME;
        bool isBoosting = playerState.InputIsSpeedBoost && playerState.CurGas >= gasDrainPerTick;
        if (isBoosting)
        {
            if (playerState.Speed < BOOST_SPEED_MINIMUM)
            {
                // This is the first frame of speed boost
                playerState.Speed = BOOST_SPEED_MINIMUM;
            }

            // Apply speed boost while draining gas. Also reset cooldown value so that it's set once boost is released
            playerState.Speed += SPEED_BOOST_RAMP * (playerState.Speed - MIN_SPEED + 1) * dt;
            playerState.CurGas -= gasDrainPerTick;
            playerState.IsSpeedBoost = true;
        }
        else
        {
            // Speed exponential falloff
            playerState.Speed -= SPEED_FALLOFF * (playerState.Speed - MIN_SPEED + 1) * dt;
            playerState.IsSpeedBoost = false;
            if (playerState.CurSpeedBoostCooldown <= 0)
            {
                //Increase gas
                playerState.CurGas += dt / GAS_INCREASE_TIME;
            }
        }

        // If out of gas then don't regen gas for a bit
        if (playerState.CurGas <= 0 && playerState.CurSpeedBoostCooldown <= 0)
        {
            playerState.CurSpeedBoostCooldown = SPEED_BOOST_COOLDOWN;
        }
        
        playerState.CurSpeedBoostCooldown = Mathf.Clamp(playerState.CurSpeedBoostCooldown - dt, 0, SPEED_BOOST_COOLDOWN);
        playerState.CurGas = Mathf.Clamp(playerState.CurGas, 0, 1);
        playerState.Speed = Mathf.Clamp(playerState.Speed, MIN_SPEED, MAX_SPEED);
    }

    private void HandleKick(float dt)
    {
        if (playerState.InputIsKick && playerState.CurKickCooldown <= 0)
        {
            playerState.CurKickCooldown = HEAVY_COOLDOWN;
            body.mass = HEAVY_MASS;
        }
        else if (playerState.CurKickCooldown <= HEAVY_COOLDOWN - HEAVY_DURATION)
        {
            body.mass = 1;
        }
        playerState.CurKickCooldown = Mathf.Clamp(playerState.CurKickCooldown - dt, 0, Mathf.Infinity);
    }

    private void SetRigidBodyVelocity(float dt)
    {
        if (!playerState.InputIsTethered)
        {
            body.velocity = playerState.Speed * body.velocity.normalized;
            return;
        }
        Vector2 diff = body.position - playerState.CenterPoint;
        float rotationDirection = Mathf.Sign(Vector2.Dot(new Vector2(diff.y, -diff.x), body.velocity));
        if (rotationDirection == 0)
        {
            rotationDirection = 1;
        }
        if (Mathf.Abs(diff.magnitude - playerState.OrbitRadius) > playerState.Speed * dt * 0.75f)
        {
            //Too large a distance to make in one step. Go towards new radius at 45 deg angle
            Vector2 tangentUnit = new Vector2(diff.y, -diff.x).normalized * rotationDirection;
            Vector2 radialChangeUnit = (diff.normalized * playerState.OrbitRadius - diff).normalized; // TODO: Could this just be -diff.normalized ?
            body.velocity = playerState.Speed * (tangentUnit + radialChangeUnit).normalized;
        }
        else
        {
            //Can make the radius change. So, solve for the new angle
            float currentAngle = Mathf.Atan2(diff.y, diff.x);
            float deltaAngle = (diff.magnitude * diff.magnitude + playerState.OrbitRadius * playerState.OrbitRadius - Mathf.Pow(playerState.Speed * dt, 2)) / (2 * diff.magnitude * playerState.OrbitRadius);
            deltaAngle = Mathf.Acos(deltaAngle);
            float newAngle = currentAngle + deltaAngle * -rotationDirection;
            Vector2 newPosition = playerState.CenterPoint + playerState.OrbitRadius * new Vector2(Mathf.Cos(newAngle), Mathf.Sin(newAngle));
            body.velocity = playerState.Speed * (newPosition - body.position).normalized;
        }
    }

    public Planet NearestPlanet()
    {
        float shortestDistance = Mathf.Infinity;
        Planet closest = null;
        foreach (Planet cur in FindObjectsOfType<Planet>())
        {
            float dist = Vector2.Distance(cur.transform.position, body.position);
            if (dist < shortestDistance && dist <= MAX_RADIUS && dist <= cur.Radius)
            {
                shortestDistance = dist;
                closest = cur;
            }
        }
        return closest;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Wall" && playerState.InputIsTethered)
        {
            body.velocity = -body.velocity;
        }
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

    public void OnKick(InputValue input)
    {
        _kick = input.isPressed;
    }

}
