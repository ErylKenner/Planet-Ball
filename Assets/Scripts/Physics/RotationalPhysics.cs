using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RotationalPhysics
{

    public static void RotateAroundPoint(Rigidbody2D body, Vector2 centerPoint, float radius, float speed, float minDistance)
    {
        radius = Mathf.Clamp(radius, minDistance, Mathf.Infinity);
        Vector2 distance = body.position - centerPoint;
        if (Mathf.Abs(distance.magnitude - radius) > speed * Time.fixedDeltaTime)
        {
            //Too large a distance to make in one step. Go towards new radius at 45 deg angle
            Vector2 tangentVelocity = ConvertToTangentialVelocity(body, centerPoint);
            Vector2 radialChange = distance.normalized * radius - distance;
            body.velocity = speed * (tangentVelocity.normalized + radialChange.normalized).normalized;
        }
        else
        {
            //Can make the radius change. So, solve for the new angle.
            float currentAngle = Mathf.Atan2(distance.y, distance.x);
            float rotationDirection = -Mathf.Sign(Vector2.Dot(new Vector2(distance.y, -distance.x), body.velocity));
            if (rotationDirection == 0)
            {
                rotationDirection = 1;
            }
            float deltaAngle = (distance.magnitude * distance.magnitude + radius * radius - Mathf.Pow(speed * Time.fixedDeltaTime, 2)) / (2 * distance.magnitude * radius);
            deltaAngle = Mathf.Acos(deltaAngle);
            float newAngle = currentAngle + deltaAngle * rotationDirection;

            Vector2 newPosition = centerPoint + radius * new Vector2(Mathf.Cos(newAngle), Mathf.Sin(newAngle));
            body.velocity = speed * (newPosition - body.position).normalized;
        }
    }

    public static Vector2 ConvertToTangentialVelocity(Rigidbody2D body, Vector2 centerPoint)
    {
        Vector2 distanceVector = body.position - centerPoint;
        float rotationDirection = Mathf.Sign(Vector2.Dot(new Vector2(distanceVector.y, -distanceVector.x), body.velocity));
        if (rotationDirection == 0)
        {
            rotationDirection = 1;
        }
        Vector2 tangentVector = new Vector2(distanceVector.y, -distanceVector.x).normalized * rotationDirection;
        return body.velocity.magnitude * tangentVector;
    }

    public static float GetRadius(Rigidbody2D body, Vector2 centerPoint)
    {
        return Vector2.Distance(body.position, centerPoint);
    }
}
