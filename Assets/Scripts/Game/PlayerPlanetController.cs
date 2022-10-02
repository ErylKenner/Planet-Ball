using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;

public class PlayerInputState
{
    public bool AttachTether = false;
    public bool WindTether = false;
    public PlayerInputState(bool attachTether, bool windTether)
    {
        AttachTether = attachTether;
        WindTether = windTether;
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
    public float OrbitRadius = 0;
    public Vector2 CenterPoint = Vector2.zero;

    // Const attributes - not state
    public float WindTetherSpeed = 120.0f;
    public float Speed = 10.0f;

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
        if (IsTethered)
        {
            if (!wasTethered)
            {
                // This is the first tick where the tether is attached. Find the nearest planet to attach to
                CenterPoint = NearestPlanet();
                if (CenterPoint == null)
                {
                    // No Planet could be tethered. Set IsTethered to false and try again next tick
                    IsTethered = false;
                    OrbitRadius = 0;
                }
                else
                {
                    OrbitRadius = Vector2.Distance(body.position, CenterPoint);
                }
            }
            if (IsWindTether)
            {
                OrbitRadius -= WindTetherSpeed * dt;
            }
        }
        else
        {
            CenterPoint = Vector2.zero;
            OrbitRadius = 0;
        }
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
        if (Mathf.Abs(diff.magnitude - OrbitRadius) > Speed * dt)
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

    private Vector2 NearestPlanet()
    {
        float shortestDistance = Mathf.Infinity;
        Planet closest = null;
        foreach (Planet cur in FindObjectsOfType<Planet>())
        {
            float dist = Vector2.Distance(cur.transform.position, body.position);
            if (dist < shortestDistance)
            {
                shortestDistance = dist;
                closest = cur;
            }
        }
        if (closest != null)
        {
            return closest.transform.position;
        }
        return Vector2.zero;
    }

}
