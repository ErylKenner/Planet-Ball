using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;
using ClientServerPrediction;
using UnityEngine.Windows;
using UnityEngine.Playables;


[RequireComponent(typeof(Rigidbody2D))]
public class PlayerPlanetController : NetworkBehaviour
{
    public PlayerState playerState = new PlayerState();
    public PlayerControllerData playerControllerData;

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
        playerState.TetherDisabledDuration = Mathf.Clamp(playerState.TetherDisabledDuration - dt, 0f, Mathf.Infinity);
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
            ContextManager.instance.SoundManager.Play("Detach", 0.05f);
            // TODO: Figure out Wind and Unwind fade outs
            ContextManager.instance.SoundManager.Stop("Wind", 0.1f);
            ContextManager.instance.SoundManager.Stop("Unwind", 0.5f);
        }
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
        playerState.OrbitRadius = Vector2.Distance(body.position, playerState.CenterPoint);
    }


    private void TryWindUnwindTether(Inputs input, float dt)
    {
        // Wind in the same orbit shape regardless of speed via exponential falloff of the radius
        float windRatio = 0f;
        if (input.WindTether < 0f)
        {
            if (!playerState.IsWinding)
            {
                // This is the first frame that winding started
                ContextManager.instance.SoundManager.Play("Wind", 0.03f);
            }
            windRatio = playerControllerData.WIND_TETHER_RATIO;
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
            windRatio = playerControllerData.UNWIND_TETHER_RATIO;
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
        if (playerState.OrbitRadius <= playerControllerData.RADIUS[0])
        {
            ContextManager.instance.SoundManager.Stop("Wind", 0.1f);
        }
        else if (playerState.OrbitRadius >= playerControllerData.RADIUS[1])
        {
            ContextManager.instance.SoundManager.Stop("Unwind", 0.5f);
        }

        playerState.OrbitRadius = Mathf.Clamp(playerState.OrbitRadius, playerControllerData.RADIUS[0], playerControllerData.RADIUS[1]);
    }


    private void HandleSpeedBoost(Inputs input, float dt)
    {
        playerState.Speed = Mathf.Clamp(body.velocity.magnitude, playerControllerData.SPEED[0], playerControllerData.SPEED[1]);
        float gasDrainPerTick = dt / playerControllerData.GAS_DRAIN_TIME;
        bool isBoosting = input.SpeedBoost && playerState.CurGas >= gasDrainPerTick;
        if (isBoosting)
        {
            if (playerState.Speed < playerControllerData.BOOST_SPEED_MINIMUM)
            {
                // This is the first frame of speed boost
                ContextManager.instance.SoundManager.Play("Boost", 0.065f);
                playerState.Speed = playerControllerData.BOOST_SPEED_MINIMUM;
            }

            // Apply speed boost while draining gas. Also reset cooldown value so that it's set once boost is released
            playerState.Speed += playerControllerData.SPEED_BOOST_RAMP * (playerState.Speed - playerControllerData.SPEED[0] + 1) * dt;
            playerState.CurGas -= gasDrainPerTick;
            playerState.IsSpeedBoost = true;
        }
        else
        {
            // Speed exponential falloff
            playerState.Speed -= playerControllerData.SPEED_FALLOFF * (playerState.Speed - playerControllerData.SPEED[0] + 1) * dt;
            playerState.IsSpeedBoost = false;
            ContextManager.instance.SoundManager.Stop("Boost", 0.5f);
            if (playerState.CurSpeedBoostCooldown <= 0)
            {
                //Increase gas
                playerState.CurGas += dt / playerControllerData.GAS_INCREASE_TIME;
            }
        }

        // If out of gas then don't regen gas for a bit
        if (playerState.CurGas <= 0 && playerState.CurSpeedBoostCooldown <= 0)
        {
            playerState.CurSpeedBoostCooldown = playerControllerData.SPEED_BOOST_COOLDOWN;
        }

        playerState.CurSpeedBoostCooldown = Mathf.Clamp(playerState.CurSpeedBoostCooldown - dt, 0, playerControllerData.SPEED_BOOST_COOLDOWN);
        playerState.CurGas = Mathf.Clamp(playerState.CurGas, 0, 1);
        playerState.Speed = Mathf.Clamp(playerState.Speed, playerControllerData.SPEED[0], playerControllerData.SPEED[1]);
    }

    private void SetRigidBody(float dt)
    {
        float speedRatio = (playerState.Speed - playerControllerData.SPEED[0]) / (playerControllerData.SPEED[1] - playerControllerData.SPEED[0]);
        body.mass = 1 + speedRatio * playerControllerData.SPEED_MASS_MULTIPLIER;
        if (!playerState.IsTethered)
        {
            body.velocity = playerState.Speed * body.velocity.normalized;
            body.angularVelocity = 0f;
            return;
        }
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
        float maxSteer = playerControllerData.STEER_RATE * playerState.Speed / playerControllerData.RADIUS[0] * dt;
        steer = Mathf.Clamp(steer, -maxSteer, maxSteer);

        // Apply steer to body
        float newTheta = curTheta + steer;
        Vector2 newDir = new Vector2(Mathf.Cos(newTheta), Mathf.Sin(newTheta));
        body.velocity = playerState.Speed * newDir;
        body.angularVelocity = steer;
    }


    public Planet NearestPlanet()
    {
        float shortestDistance = Mathf.Infinity;
        Planet closest = null;
        foreach (Planet cur in FindObjectsOfType<Planet>())
        {
            float dist = Vector2.Distance(cur.transform.position, body.position);
            if (dist < shortestDistance && dist <= playerControllerData.RADIUS[1] && dist <= cur.Radius)
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
            playerState.TetherDisabledDuration = playerControllerData.TETHER_DISABLED_DURATION;
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
        float volume = Mathf.Clamp01((collision.relativeVelocity.magnitude - playerControllerData.SPEED[0]) / (playerControllerData.SPEED[1] - playerControllerData.SPEED[0]) * gainQualifier);
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
