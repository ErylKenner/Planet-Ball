using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;
using ClientServerPrediction;


[RequireComponent(typeof(Rigidbody2D))]
public class PlayerPlanetController : NetworkBehaviour
{
    public GameObject PlayerShockwavePrefab;
    public GameObject BallShockwavePrefab;
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
    public float TETHER_DISABLED_DURATION = 1f;

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

    public void Start()
    {
        if (!isLocalPlayer)
        {
            // Disable any components that we don't want to compute since they belong to other players
            GetComponent<PlayerInput>().enabled = false;
        }
    }

    public void ApplyInput(Inputs input, float dt)
    {
        SetStateFromInput(input, dt);
        SetRigidBodyVelocity(dt);
    }

    private void SetStateFromInput(Inputs input, float dt)
    {
        playerState.TetherDisabledDuration = Mathf.Clamp(playerState.TetherDisabledDuration -= dt, 0f, Mathf.Infinity);

        bool wasInputTethered = playerState.InputIsTethered;
        bool wasInputSpeedBoost = playerState.InputIsSpeedBoost;
        playerState.InputIsTethered = ((input == null && playerState.InputIsTethered) || (input != null && input.AttachTether)) && (playerState.TetherDisabledDuration <= 0f);
        playerState.InputIsWindTether = (input == null && playerState.InputIsWindTether) || (input != null && input.WindTether);
        playerState.InputIsUnwindTether = (input == null && playerState.InputIsUnwindTether) || (input != null && input.UnwindTether);
        playerState.InputIsSpeedBoost = (input == null && playerState.InputIsSpeedBoost) || (input != null && input.SpeedBoost);
        playerState.InputIsKick = (input == null && playerState.InputIsKick) || (input != null && input.Kick);


        SetSpeedAndMass();
        HandleTether(wasInputTethered, dt);
        HandleSpeedBoost(wasInputSpeedBoost, dt);
        //HandleKick(dt);
        playerState.CurPosition = body.position;
    }

    private void SetSpeedAndMass()
    {
        playerState.Speed = Mathf.Clamp(body.velocity.magnitude, MIN_SPEED, MAX_SPEED);
        float speedRatio = (playerState.Speed - MIN_SPEED) / (MAX_SPEED - MIN_SPEED);
        body.mass = 1 + speedRatio * SPEED_MASS_MULTIPLIER;
    }

    private void SetBounciness(float bounciness)
    {
        CircleCollider2D coll = GetComponent<CircleCollider2D>();
        PhysicsMaterial2D material = coll.sharedMaterial;
        material.bounciness = bounciness;
        coll.sharedMaterial = material;
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
                    SetBounciness(0f);
                }
            }
            if (playerState.InputIsWindTether)
            {
                // Wind in the same orbit shape regardless of speed via exponential falloff of the radius
                float windRate = WIND_TETHER_RATIO * playerState.OrbitRadius * playerState.Speed;
                playerState.OrbitRadius -= windRate * dt;
            }
            if (playerState.InputIsUnwindTether && playerState.WallCollisionCount == 0)
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
            SetBounciness(1f);
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
        if (collision.gameObject.tag == "Ball")
        {
            body.velocity = collision.relativeVelocity;
            Vector3 contactPoint = new Vector3(collision.GetContact(0).point.x, collision.GetContact(0).point.y, 0);
            Instantiate(BallShockwavePrefab, contactPoint, Quaternion.identity);
            return;
        }
        if (collision.gameObject.tag == "Player")
        {
            Vector3 contactPoint = new Vector3(collision.GetContact(0).point.x, collision.GetContact(0).point.y, 0);
            Instantiate(PlayerShockwavePrefab, contactPoint, Quaternion.identity);
            playerState.TetherDisabledDuration = TETHER_DISABLED_DURATION;
        }
        if (collision.gameObject.tag == "Wall" || collision.gameObject.tag == "Goal")
        {
            playerState.WallCollisionCount += 1;
            if (playerState.InputIsTethered)
            {
                // Reset position to what we set it in ApplyInput since there's no way to customize Unity's depenetration function
                body.position = playerState.CurPosition;

                // Only bounce backward if this is the only wall collision
                if (playerState.WallCollisionCount == 1)
                {
                    body.velocity = collision.relativeVelocity;
                }
                else if (playerState.InputIsUnwindTether)
                {
                    // Hard set radius to prevent the target radius differing too much from the rendered radius.
                    // Also add a small offset to the radius so that the collision exit trigger only happens once the player
                    // intentionally stops tetering or winds the tether, and not because of Unity collision detection precision.
                    playerState.OrbitRadius = 0.2f + Vector2.Distance(body.position, playerState.CenterPoint);
                }
            }
            return;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Wall" || collision.gameObject.tag == "Goal")
        {
            // Set bounciness to 1 to handle the case where we collided with 2+ walls at once and never got to bounce backward
            SetBounciness(1f);
            playerState.WallCollisionCount -= 1;
            return;
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
