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
    public float TETHER_DISABLED_DURATION = 1f;

    public float kP = 0.1f;
    public float kI = 0f;
    public float kD = 0.01f;
    float STEER_RATE = 1f;

    // For testing. Input system callbacks set these which are then read in FixedUpdate
    private bool _attachTether = false;
    private float _windTether = 0f;
    private bool _speedBoost = false;
    private bool _kick = false;

    private Rigidbody2D body;


    public Inputs GetInputs()
    {
        return new Inputs
        {
            AttachTether = _attachTether,
            WindTether = _windTether,
            SpeedBoost = _speedBoost,
            Kick = _kick
        };
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        //Physics.defaultMaxDepenetrationVelocity = 0.0001f;
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
        SetRigidBodyVelocity4(dt);
    }

    private void SetStateFromInput(Inputs input, float dt)
    {
        playerState.TetherDisabledDuration = Mathf.Clamp(playerState.TetherDisabledDuration -= dt, 0f, Mathf.Infinity);

        bool wasInputTethered = playerState.InputIsTethered;
        bool wasInputSpeedBoost = playerState.InputIsSpeedBoost;
        float previousWind = playerState.InputWindTether;
        if (input != null)
        {
            playerState.InputIsTethered = input.AttachTether && (playerState.TetherDisabledDuration <= 0f);
            playerState.InputWindTether = input.WindTether;
            playerState.InputIsSpeedBoost = input.SpeedBoost;
            playerState.InputIsKick = input.Kick;
        }


        SetSpeedAndMass();
        HandleTether(wasInputTethered, previousWind, dt);
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

    private void HandleTether(bool wasInputTethered, float previousWind, float dt)
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
                    ContextManager.instance.SoundManager.Play("Attach", 0.05f);

                    // TODO: Clean up
                    if(playerState.InputWindTether < 0f)
                    {
                        ContextManager.instance.SoundManager.Play("Wind", 0.03f);
                    }

                    if (playerState.InputWindTether > 0f)
                    {
                        ContextManager.instance.SoundManager.Play("Unwind", 0.03f);
                    }

                    playerState.CenterPoint = nearestPlanet.transform.position;
                    playerState.OrbitRadius = Vector2.Distance(body.position, playerState.CenterPoint);
                }
            }

            // Wind in the same orbit shape regardless of speed via exponential falloff of the radius
            float windRatio = 0f;
            if (playerState.InputWindTether < 0f)
            {
                if(previousWind >= 0)
                {
                    ContextManager.instance.SoundManager.Play("Wind", 0.03f);
                }
                windRatio = WIND_TETHER_RATIO;
            }
            else
            if (playerState.InputWindTether > 0f)
            {
                if (previousWind <= 0)
                {
                    ContextManager.instance.SoundManager.Play("Unwind", 0.03f);
                }
                windRatio = UNWIND_TETHER_RATIO;
            }
            
            float signedWindSpeed = Mathf.Sign(playerState.InputWindTether) * Mathf.Pow(Mathf.Abs(playerState.InputWindTether), 2);
            float baseWindRate = windRatio * playerState.OrbitRadius * playerState.Speed;
            playerState.OrbitRadius += signedWindSpeed * baseWindRate * dt;

            if ((previousWind != 0 && playerState.InputWindTether == 0) || playerState.OrbitRadius <= MIN_RADIUS || playerState.OrbitRadius >= MAX_RADIUS)
            {
                ContextManager.instance.SoundManager.Stop("Wind", 0.1f);
                ContextManager.instance.SoundManager.Stop("Unwind", 0.5f);
            }
        }
        else
        {
            if(wasInputTethered)
            {
                ContextManager.instance.SoundManager.Play("Detach", 0.05f);
                // TODO: Figure out Wind and Unwind fade outs
                ContextManager.instance.SoundManager.Stop("Wind", 0.1f);
                ContextManager.instance.SoundManager.Stop("Unwind", 0.5f);
            }
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
                ContextManager.instance.SoundManager.Play("Boost", 0.065f);
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
            ContextManager.instance.SoundManager.Stop("Boost", 0.5f);
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

    private void SetRigidBodyVelocity2(float dt)
    {
        // Based on force
        if (!playerState.InputIsTethered)
        {
            body.velocity = playerState.Speed * body.velocity.normalized;
            return;
        }
        Vector2 diff = body.position - playerState.CenterPoint;
        Vector2 tangent = new Vector2(diff.y, -diff.x).normalized;
        float rotatingClockwise = Mathf.Sign(Vector2.Dot(tangent, body.velocity)) < 0 ? -1 : 1;
        Vector2 desiredTangent = tangent * rotatingClockwise * playerState.Speed;


        float radialForce = body.mass * playerState.Speed * playerState.Speed / playerState.OrbitRadius;
        Vector2 impulseRadial = -diff.normalized * radialForce * dt;
        body.AddForce(impulseRadial, ForceMode2D.Impulse);

        Vector2 impulseTangent = desiredTangent * dt;
        body.AddForce(impulseTangent, ForceMode2D.Impulse);

        Vector2 radialCorection = diff.normalized * (playerState.OrbitRadius - diff.magnitude);

        body.velocity = body.velocity.normalized * playerState.Speed;
        //body.velocity = playerState.Speed * (desiredTangent + desiredRadial).normalized;

    }

    private void SetRigidBodyVelocity3(float dt)
    {
        // Based on angular velocity
        if (!playerState.InputIsTethered)
        {
            body.velocity = playerState.Speed * body.velocity.normalized;
            return;
        }
        Vector2 diff = body.position - playerState.CenterPoint;
        float curTheta = Mathf.Atan2(diff.y, diff.x);
        float newTheta = curTheta + playerState.Speed * dt;
        Vector2 newPos = playerState.CenterPoint + new Vector2(playerState.OrbitRadius * Mathf.Cos(newTheta), playerState.OrbitRadius * Mathf.Sin(newTheta));
        body.velocity = (newPos - body.position).normalized * playerState.Speed;
    }

    private void SetRigidBodyVelocity4(float dt)
    {
        // Limit steer rate
        if (!playerState.InputIsTethered)
        {
            body.velocity = playerState.Speed * body.velocity.normalized;
            return;
        }
        Vector2 diff = body.position - playerState.CenterPoint;
        Vector2 tangent = new Vector2(diff.y, -diff.x).normalized;
        float rotatingClockwise = Mathf.Sign(Vector2.Dot(tangent, body.velocity)) < 0 ? -1 : 1;
        float curTheta = Mathf.Atan2(body.velocity.y, body.velocity.x);

        float futureTheta = Mathf.Atan2(diff.y, diff.x) + playerState.Speed / playerState.OrbitRadius * dt * -rotatingClockwise;
        Vector2 futurePos = playerState.CenterPoint + new Vector2(playerState.OrbitRadius * Mathf.Cos(futureTheta), playerState.OrbitRadius * Mathf.Sin(futureTheta));
        Vector2 desiredTangent = futurePos - body.position;

        float desiredAngle = (Mathf.Atan2(desiredTangent.y, desiredTangent.x) + 2 * Mathf.PI) % (2 * Mathf.PI);
        float steer = (desiredAngle - curTheta + 2 * Mathf.PI) % (2 * Mathf.PI);
        if (steer >= Mathf.PI)
        {
            steer = steer - 2 * Mathf.PI;
        }
        float steerRate = STEER_RATE * playerState.Speed / MIN_RADIUS * dt;
        steer = Mathf.Clamp(steer, -steerRate, steerRate);
        float newDir = curTheta + steer;
        body.velocity = new Vector2(playerState.Speed * Mathf.Cos(newDir), playerState.Speed * Mathf.Sin(newDir));
        body.angularVelocity = steer;
        Debug.Log("Current error: " + (playerState.OrbitRadius - diff.magnitude) + " Desired radius: " + playerState.OrbitRadius + ", Current Radius: " + diff.magnitude);
    }


    private void SetRigidBodyVelocity5(float dt)
    {
        // Use PID to try to follow orbit at set radius
        if (!playerState.InputIsTethered)
        {
            body.velocity = playerState.Speed * body.velocity.normalized;
            return;
        }
        Vector2 diff = body.position - playerState.CenterPoint;
        Vector2 tangent = new Vector2(diff.y, -diff.x).normalized;
        float rotatingClockwise = Mathf.Sign(Vector2.Dot(tangent, body.velocity)) < 0 ? -1 : 1;

        float crossTrackError = playerState.OrbitRadius - diff.magnitude;
        float crossTrackErrorRate = Vector2.Dot(body.velocity, diff) / diff.magnitude;

        float curTheta = Mathf.Atan2(body.velocity.y, body.velocity.x);



        float futureTheta = Mathf.Atan2(diff.y, diff.x) + playerState.Speed / playerState.OrbitRadius * dt * -rotatingClockwise;
        Vector2 futurePos = playerState.CenterPoint + new Vector2(playerState.OrbitRadius * Mathf.Cos(futureTheta), playerState.OrbitRadius * Mathf.Sin(futureTheta));
        Vector2 desiredTangent = futurePos - body.position;

        //Vector2 desiredTangent = tangent * rotatingClockwise * playerState.Speed;
        float desiredAngle = (Mathf.Atan2(desiredTangent.y, desiredTangent.x) + 2 * Mathf.PI) % (2 * Mathf.PI);
        float feedforward = (desiredAngle - curTheta + 2 * Mathf.PI) % (2 * Mathf.PI);
        if (feedforward >= Mathf.PI)
        {
            feedforward = feedforward - 2 * Mathf.PI;
        }
        //Debug.Log("Desired andle: " + desiredAngle * 180 / Mathf.PI + ", feed forward: " + feedforward * 180 / Mathf.PI);
        feedforward = Mathf.Clamp(feedforward, -STEER_RATE * 20f / 180f * Mathf.PI, STEER_RATE * 20f / 180f * Mathf.PI);

        float steer = feedforward + (kP * crossTrackError - kD * crossTrackErrorRate) * rotatingClockwise;
        steer = Mathf.Clamp(steer, -STEER_RATE * 20f / 180f * Mathf.PI, STEER_RATE * 20f / 180f * Mathf.PI);
        float newDir = curTheta + steer;
        body.velocity = new Vector2(playerState.Speed * Mathf.Cos(newDir), playerState.Speed * Mathf.Sin(newDir));
        Debug.Log("Current error: " + crossTrackError + "Desired radius: " + playerState.OrbitRadius + ", Current Radius: " + diff.magnitude);
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
            PlaySound(collision);

            Vector2 normal = collision.GetContact(0).normal;
            if (collision.contactCount == 1 && collision.relativeVelocity.magnitude >= MIN_SPEED / 2 && Vector2.Angle(normal, collision.relativeVelocity) <= 45)
            {
                // Head on collision
                //body.velocity = collision.relativeVelocity;
            }
            else
            {
                // Shallow glancing collision
            }
        }
        else
        if (collision.gameObject.tag == "Player")
        {
            if(isLocalPlayer)
            {
                PlaySound(collision);
            }
            playerState.TetherDisabledDuration = TETHER_DISABLED_DURATION;
        }
        else
        if (collision.gameObject.tag == "Wall" || collision.gameObject.tag == "Goal")
        {
            PlaySound(collision, "WallHit", 0.1f);
            if (playerState.InputIsTethered)
            {
                // Reset position to what we set it in ApplyInput since there's no way to customize Unity's depenetration function
                body.position = playerState.CurPosition;
                body.velocity = collision.relativeVelocity;
            }
        }
    }

    private void PlaySound(Collision2D collision, string name="Kick", float gainQualifier=1.0f)
    {
        float volume = Mathf.Clamp01((collision.relativeVelocity.magnitude - MIN_SPEED) / (MAX_SPEED - MIN_SPEED) * gainQualifier);
        ContextManager.instance.SoundManager.Play(name, 0.1f, volume);
    }

    public void OnAttachTether(InputValue input)
    {
        _attachTether = input.isPressed;
    }

    public void OnWindTether(InputValue input)
    {
        _windTether = input.Get<float>();
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
