using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RotationalPhysics
{

    public static void RotateAroundPoint(Rigidbody2D body, Vector2 centerPoint, float radius, float speed)
    {
        Vector2 distance = body.position - centerPoint;

        float currentAngle = Mathf.Atan2(distance.y, distance.x);
        float rotationDirection = -Mathf.Sign(Vector2.Dot(new Vector2(distance.y, -distance.x), body.velocity));
        float deltaAngle = Mathf.Acos((distance.magnitude * distance.magnitude + radius * radius - Mathf.Pow(speed * Time.fixedDeltaTime, 2)) / (2 * distance.magnitude * radius));
        float newAngle = currentAngle + deltaAngle * rotationDirection;

        Vector2 newPosition = centerPoint + radius * new Vector2(Mathf.Cos(newAngle), Mathf.Sin(newAngle));
        body.velocity = speed * (newPosition - (Vector2)body.transform.position).normalized;
    }

    public static Vector2 OnlyTangentialVelocity(Rigidbody2D body, Vector2 centerPoint)
    {
        Vector2 curPosition = (Vector2)body.transform.position;
        Vector2 distanceVector = curPosition - centerPoint;
        Vector2 tangentVector = new Vector2(distanceVector.y, -distanceVector.x).normalized;
        //return Vector2.Dot(body.velocity, tangentVector) * tangentVector;
        return body.velocity.magnitude * tangentVector;
    }

    public static float GetRadius(Rigidbody2D body, Vector2 centerPoint)
    {
        return Vector2.Distance(body.transform.position, centerPoint);
    }
}
