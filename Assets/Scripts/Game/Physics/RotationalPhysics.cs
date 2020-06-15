using UnityEngine;

public static class RotationalPhysics
{
    public static void RotateAroundPoint(Rigidbody2D body, Vector2 centerPoint, float desiredRadius, float currentSpeed, float dt)
    {
        Vector2 diff = body.position - centerPoint;
        if (Mathf.Abs(diff.magnitude - desiredRadius) > currentSpeed * dt)
        {
            //Too large a distance to make in one step. Go towards new radius at 45 deg angle
            Vector2 tangentVelocity = ConvertToUnitTangentialVelocity(body.position, body.velocity, centerPoint);
            Vector2 radialChange = diff.normalized * desiredRadius - diff;
            body.velocity = currentSpeed * (tangentVelocity + radialChange.normalized).normalized;
        }
        else
        {
            //Can make the radius change. So, solve for the new angle
            float currentAngle = Mathf.Atan2(diff.y, diff.x);
            float rotationDirection = -Mathf.Sign(Vector2.Dot(new Vector2(diff.y, -diff.x), body.velocity));
            float deltaAngle = (diff.magnitude * diff.magnitude + desiredRadius * desiredRadius - Mathf.Pow(currentSpeed * dt, 2)) / (2 * diff.magnitude * desiredRadius);
            deltaAngle = Mathf.Acos(deltaAngle);
            float newAngle = currentAngle + deltaAngle * rotationDirection;

            Vector2 newPosition = centerPoint + desiredRadius * new Vector2(Mathf.Cos(newAngle), Mathf.Sin(newAngle));
            body.velocity = currentSpeed * (newPosition - body.position).normalized;
        }
    }

    public static Vector2 ConvertToUnitTangentialVelocity(Vector2 position, Vector2 velocity, Vector2 centerPoint)
    {
        Vector2 diff = position - centerPoint;
        Vector2 tangent = new Vector2(diff.y, -diff.x).normalized;
        float rotationDirection = Mathf.Sign(Vector2.Dot(tangent, velocity));
        return tangent * rotationDirection;
    }

}
