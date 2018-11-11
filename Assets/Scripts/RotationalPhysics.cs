using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RotationalPhysics
{

    public static void RotateAroundPoint(Rigidbody2D body, Vector2 centerPoint, float radius, float speed, float minDistance)
    {
        radius = Mathf.Clamp(radius, minDistance, Mathf.Infinity);
        Vector2 distance = body.position - centerPoint;
        if (distance.magnitude + speed * Time.fixedDeltaTime >= radius)
        {
            float currentAngle = Mathf.Atan2(distance.y, distance.x);
            float rotationDirection = -Mathf.Sign(Vector2.Dot(new Vector2(distance.y, -distance.x), body.velocity));
            if (rotationDirection == 0)
            {
                rotationDirection = 1;
            }
            float deltaAngle = Mathf.Acos((distance.magnitude * distance.magnitude + radius * radius - Mathf.Pow(speed * Time.fixedDeltaTime, 2)) / (2 * distance.magnitude * radius));
            float newAngle = currentAngle + deltaAngle * rotationDirection;

            Vector2 newPosition = centerPoint + radius * new Vector2(Mathf.Cos(newAngle), Mathf.Sin(newAngle));
            body.velocity = speed * (newPosition - (Vector2)body.transform.position).normalized;
        }
        else
        {
            Vector2 tangentVelocity = new Vector2(distance.y, -distance.x);
            body.velocity = speed * (tangentVelocity.normalized + distance.normalized).normalized;
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
        return Vector2.Distance(body.transform.position, centerPoint);
    }
}
