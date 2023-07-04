using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;
using ClientServerPrediction;
using UnityEngine.Windows;
using UnityEngine.Playables;
using NaughtyAttributes;
using UnityEngine.UIElements;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerPlanetController : NetworkBehaviour
{
    [Dropdown("turningAlgorithms")]
    public string TurningAlgorithm;
    private List<string> turningAlgorithms { get { return new List<string>() { "TurnTowardRadius", "GSquaredContinuity", "TurnUntilTangent" }; } }

    public PlayerState playerState = new PlayerState();
    public PlayerControllerData settings;

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
        SetState(input, dt);
        SetRigidBody(dt);
    }

    private void SetState(Inputs input, float dt)
    {
        playerState.CurPosition = body.position;
        HandleTether(input, dt);
        HandleSpeedBoost(input, dt);
    }


    private void HandleTether(Inputs input, float dt)
    {
        bool isTethered = input.AttachTether && (playerState.TetherDisabledDuration <= 0f);
        if (isTethered)
        {
            if (playerState.IsTethered)
            {
                TryWindUnwindTether(input, dt);
            }
            else
            {
                TryAttachTether(input);
            }
        }
        else if (playerState.IsTethered)
        {
            playerState.IsTethered = false;
            playerState.OrbitRadius = 0f;
            ContextManager.instance.SoundManager.Play("Detach", 0.05f);
            // TODO: Figure out Wind and Unwind fade outs
            ContextManager.instance.SoundManager.Stop("Wind", 0.1f);
            ContextManager.instance.SoundManager.Stop("Unwind", 0.5f);
        }
        playerState.TetherDisabledDuration = Mathf.Clamp(playerState.TetherDisabledDuration - dt, 0f, Mathf.Infinity);
    }

    private void TryAttachTether(Inputs input)
    {
        Planet nearestPlanet = NearestPlanet();
        if (nearestPlanet == null)
        {
            // No Planet could be tethered
            playerState.IsTethered = false;
            playerState.OrbitRadius = 0;
            return;
        }

        ContextManager.instance.SoundManager.Play("Attach", 0.05f);
        if (input.WindTether < 0f)
        {
            ContextManager.instance.SoundManager.Play("Wind", 0.03f);
        }
        else if (input.WindTether > 0f)
        {
            ContextManager.instance.SoundManager.Play("Unwind", 0.03f);
        }

        playerState.IsTethered = true;
        playerState.CenterPoint = nearestPlanet.transform.position;

        // TODO: Use the spline calculation to determine what the radius should be (based on current velocity, position, and planet position)
        //playerState.OrbitRadius = Vector2.Distance(body.position, playerState.CenterPoint);
    }


    private void TryWindUnwindTether(Inputs input, float dt)
    {
        // Don't wind/unwind if player is still getting toward a radius
        if (playerState.OrbitRadius <= 0f)
        {
            return;
        }

        // Wind in the same orbit shape regardless of speed via exponential falloff of the radius
        float windRatio = 0f;
        if (input.WindTether < 0f)
        {
            if (!playerState.IsWinding)
            {
                // This is the first frame that winding started
                ContextManager.instance.SoundManager.Play("Wind", 0.03f);
            }
            windRatio = settings.WIND_TETHER_RATIO;
            playerState.IsWinding = true;
            playerState.IsUnwinding = false;
        }
        else if (input.WindTether > 0f)
        {
            if (!playerState.IsUnwinding)
            {
                // This is the first frame that unwinding started
                ContextManager.instance.SoundManager.Play("Unwind", 0.03f);
            }
            windRatio = settings.UNWIND_TETHER_RATIO;
            playerState.IsWinding = false;
            playerState.IsUnwinding = true;
        }
        else
        {
            // This is the first frame that winding or unwinding has stopped
            if (playerState.IsWinding)
            {
                ContextManager.instance.SoundManager.Stop("Wind", 0.1f);
            }
            else if (playerState.IsUnwinding)
            {
                ContextManager.instance.SoundManager.Stop("Unwind", 0.5f);
            }
            playerState.IsWinding = false;
            playerState.IsUnwinding = false;
        }

        // Perform wind/unwind
        float signedWindSpeed = Mathf.Sign(input.WindTether) * Mathf.Pow(Mathf.Abs(input.WindTether), 2);
        float baseWindRate = windRatio * playerState.OrbitRadius * playerState.Speed;
        playerState.OrbitRadius += signedWindSpeed * baseWindRate * dt;

        // Check if the orbit radius has hit its limit
        if (playerState.OrbitRadius <= settings.RADIUS[0])
        {
            ContextManager.instance.SoundManager.Stop("Wind", 0.1f);
        }
        else if (playerState.OrbitRadius >= settings.RADIUS[1])
        {
            ContextManager.instance.SoundManager.Stop("Unwind", 0.5f);
        }

        playerState.OrbitRadius = Mathf.Clamp(playerState.OrbitRadius, settings.RADIUS[0], settings.RADIUS[1]);
    }


    private void HandleSpeedBoost(Inputs input, float dt)
    {
        playerState.Speed = Mathf.Clamp(body.velocity.magnitude, settings.SPEED[0], settings.SPEED[1]);
        float gasDrainPerTick = dt / settings.GAS_DRAIN_TIME;
        bool isBoosting = input.SpeedBoost && playerState.CurGas >= gasDrainPerTick;
        if (isBoosting)
        {
            if (playerState.Speed < settings.BOOST_SPEED_MINIMUM)
            {
                // This is the first frame of speed boost
                ContextManager.instance.SoundManager.Play("Boost", 0.065f);
                playerState.Speed = settings.BOOST_SPEED_MINIMUM;
            }

            // Apply speed boost while draining gas. Also reset cooldown value so that it's set once boost is released
            playerState.Speed += settings.SPEED_BOOST_RAMP * (playerState.Speed - settings.SPEED[0] + 1) * dt;
            playerState.CurGas -= gasDrainPerTick;
            playerState.IsSpeedBoost = true;
        }
        else
        {
            // Speed exponential falloff
            playerState.Speed -= settings.SPEED_FALLOFF * (playerState.Speed - settings.SPEED[0] + 1) * dt;
            playerState.IsSpeedBoost = false;
            ContextManager.instance.SoundManager.Stop("Boost", 0.5f);
            if (playerState.CurSpeedBoostCooldown <= 0)
            {
                //Increase gas
                playerState.CurGas += dt / settings.GAS_INCREASE_TIME;
            }
        }

        // If out of gas then don't regen gas for a bit
        if (playerState.CurGas <= 0 && playerState.CurSpeedBoostCooldown <= 0)
        {
            playerState.CurSpeedBoostCooldown = settings.SPEED_BOOST_COOLDOWN;
        }

        playerState.CurSpeedBoostCooldown = Mathf.Clamp(playerState.CurSpeedBoostCooldown - dt, 0, settings.SPEED_BOOST_COOLDOWN);
        playerState.CurGas = Mathf.Clamp(playerState.CurGas, 0, 1);
        playerState.Speed = Mathf.Clamp(playerState.Speed, settings.SPEED[0], settings.SPEED[1]);
    }

    private void SetRigidBody(float dt)
    {
        float speedRatio = (playerState.Speed - settings.SPEED[0]) / (settings.SPEED[1] - settings.SPEED[0]);
        body.mass = 1 + speedRatio * settings.SPEED_MASS_MULTIPLIER;
        if (!playerState.IsTethered)
        {
            body.velocity = playerState.Speed * body.velocity.normalized;
            body.angularVelocity = 0f;
            return;
        }

        switch (TurningAlgorithm)
        {
            case "TurnTowardRadius":
                TurnTowardRadius(dt);
                break;
            case "GSquaredContinuity":
                GSquaredContinuity(dt);
                break;
            case "TurnUntilTangent":
                TurnUntilTangent(dt);
                break;
            default:
                break;
        }
    }

    private void TurnTowardRadius(float dt)
    {
        // Current state of the body
        Vector2 diff = body.position - playerState.CenterPoint;
        Vector2 tangent = new Vector2(diff.y, -diff.x).normalized;
        float rotatingClockwise = Mathf.Sign(Vector2.Dot(tangent, body.velocity)) < 0 ? -1 : 1;
        float curTheta = Mathf.Atan2(body.velocity.y, body.velocity.x);

        // Calculate where player would end up next frame if turning rate was unlimited
        float futureTheta = Mathf.Atan2(diff.y, diff.x) + playerState.Speed / playerState.OrbitRadius * dt * -rotatingClockwise;
        Vector2 unitFuturePosLocal = new Vector2(Mathf.Cos(futureTheta), Mathf.Sin(futureTheta));
        Vector2 futurePos = playerState.CenterPoint + playerState.OrbitRadius * unitFuturePosLocal;

        // Calculate a steering angle to apply to the current velocity
        Vector2 desiredDir = futurePos - body.position;
        float desiredAngle = (Mathf.Atan2(desiredDir.y, desiredDir.x) + 2 * Mathf.PI) % (2 * Mathf.PI);
        float steer = (desiredAngle - curTheta + 2 * Mathf.PI) % (2 * Mathf.PI);
        if (steer >= Mathf.PI)
        {
            steer -= 2 * Mathf.PI;  // Normalize to [-pi, pi]
        }
        float maxSteer = settings.STEER_RATE * playerState.Speed / settings.RADIUS[0] * dt;
        steer = Mathf.Clamp(steer, -maxSteer, maxSteer);

        // Apply steer to body
        float newTheta = curTheta + steer;
        Vector2 newDir = new Vector2(Mathf.Cos(newTheta), Mathf.Sin(newTheta));
        body.velocity = playerState.Speed * newDir;
        body.angularVelocity = steer;
    }

    private void GSquaredContinuity(float dt)
    {
        // TODO: Use G^2 Continuity to smoothly go from current velocity in a straight line to orbitting the planet at a radius
        // Treat our current velocity as being on a large circle with radius proportional to our max turning rate
    }

    private void TurnUntilTangent(float dt)
    {
        // TODO: Turn at max turning rate until velocity is perpendicular to vector connecting player and planet

        // 1. Get current angle difference between velocity and tangent
        // If larger than max steer rate, rotate velocity by max steer rate
        // Otherwise, if orbit radius == 0, then set it to current radius
        // Rotate velocity so that the next position (extrapolated based on velocity and dt) is on the circle with radius == orbit radius

        Vector2 diff = playerState.CenterPoint - body.position;
        Vector2 perpendicular = Vector2.Perpendicular(diff);
        Vector2 tangent = perpendicular * (Mathf.Sign(Vector2.Dot(body.velocity, perpendicular)) < 0 ? -1 : 1);
        float angleDiff = Vector2.SignedAngle(body.velocity, tangent) * Mathf.Deg2Rad;


        // If we are travelling approximately tangent to the centerpoint we can set our radius
        float maxSteer = settings.STEER_RATE * playerState.Speed / settings.RADIUS[0] * dt;
        if (Mathf.Abs(angleDiff) < maxSteer && playerState.OrbitRadius <= 0)
        {
            playerState.OrbitRadius = diff.magnitude;
        }

        Vector2 newDir;
        if (playerState.OrbitRadius <= 0)
        {
            // Keep turning at max steer rate towards the tangent
            angleDiff = Mathf.Sign(angleDiff) * Mathf.Min(Mathf.Abs(angleDiff), maxSteer);
            newDir = new Vector2(body.velocity.x * Mathf.Cos(angleDiff) - body.velocity.y * Mathf.Sin(angleDiff), body.velocity.x * Mathf.Sin(angleDiff) + body.velocity.y * Mathf.Cos(angleDiff));
        }
        else
        {
            // Calculate where player would end up next frame if turning rate was unlimited
            float rotatingClockwise = Mathf.Sign(Vector2.Dot(perpendicular, body.velocity)) < 0 ? -1 : 1;
            float futureTheta = Mathf.Atan2(-diff.y, -diff.x) - rotatingClockwise * playerState.Speed / playerState.OrbitRadius * dt;
            Vector2 unitFuturePosLocal = new Vector2(Mathf.Cos(futureTheta), Mathf.Sin(futureTheta));
            Vector2 futurePos = playerState.CenterPoint + playerState.OrbitRadius * unitFuturePosLocal;
            newDir = futurePos - body.position;
        }

        body.velocity = playerState.Speed * newDir.normalized;
        body.angularVelocity = Mathf.Sign(angleDiff) * maxSteer;
    }


    public Planet NearestPlanet()
    {
        float shortestDistance = Mathf.Infinity;
        Planet closest = null;
        foreach (Planet cur in FindObjectsOfType<Planet>())
        {
            float dist = Vector2.Distance(cur.transform.position, body.position);
            if (dist < shortestDistance && dist <= settings.RADIUS[1] && dist <= cur.Radius)
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
        }
        else
        if (collision.gameObject.tag == "Player")
        {
            if (isLocalPlayer)
            {
                PlaySound(collision);
            }
            playerState.TetherDisabledDuration = settings.COLLISION_TETHER_DISABLED_DURATION;
        }
        else
        if (collision.gameObject.tag == "Wall" || collision.gameObject.tag == "Goal")
        {
            PlaySound(collision, "WallHit", 0.1f);
            if (playerState.IsTethered)
            {
                body.position = playerState.CurPosition;
                body.velocity = collision.relativeVelocity;
            }
        }
    }

    private void PlaySound(Collision2D collision, string name = "Kick", float gainQualifier = 1.0f)
    {
        // Quadratic volume
        float t = (collision.relativeVelocity.magnitude - settings.SPEED[0]) / (settings.SPEED[1] - settings.SPEED[0]);
        float volume = gainQualifier * t * t;

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
