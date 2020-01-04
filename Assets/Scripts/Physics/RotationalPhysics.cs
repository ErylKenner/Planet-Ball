using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RotationalPhysics
{

    public static void RotateAroundPoint(Rigidbody2D rb, Vector2 centerPoint, float radius, float speed)
    {
        Vector2 diff = rb.position - centerPoint;
        if (Mathf.Abs(diff.magnitude - radius) > speed * Time.fixedDeltaTime)
        {
            //Too large a distance to move to the correct radius in one step. Go towards new radius at 45 deg angle
            Vector2 radialChange = diff.normalized * radius - diff;
            rb.velocity = speed * (GetTangentUnitVector(rb.position, rb.velocity, centerPoint) + radialChange.normalized).normalized;
        }
        else
        {
            //Player can make the full radius change this frame. So, solve for the new position using law of cosines
            float curRadius = diff.magnitude;
            float toTravel = speed * Time.fixedDeltaTime;
            float deltaAngle = Mathf.Acos((curRadius * curRadius + radius * radius - toTravel * toTravel) / (2.0f * curRadius * radius));

            float currentAngle = Mathf.Atan2(diff.y, diff.x);
            int rotationDirection = (int)Mathf.Sign(Vector2.Dot(new Vector2(diff.y, -diff.x), rb.velocity));
            if (rotationDirection == 0)
            {
                rotationDirection = 1;
            }
            float newAngle = currentAngle - deltaAngle * rotationDirection;
            Vector2 newPosition = centerPoint + radius * new Vector2(Mathf.Cos(newAngle), Mathf.Sin(newAngle));
            rb.velocity = speed * (newPosition - rb.position).normalized;
        }
    }

    public static Vector2 GetTangentUnitVector(Vector2 position, Vector2 velocity, Vector2 centerPoint)
    {
        Vector2 diff = position - centerPoint;
        Vector2 tangent = new Vector2(diff.y, -diff.x);
        int rotationDirection = (int)Mathf.Sign(Vector2.Dot(tangent, velocity));
        if (rotationDirection == 0)
        {
            rotationDirection = 1;
        }
        return tangent.normalized * rotationDirection;
    }

}
