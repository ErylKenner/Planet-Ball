using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateShockwave : MonoBehaviour
{
    public GameObject PlayerShockwavePrefab;
    public GameObject BallShockwavePrefab;
    public float SPEED_RADIUS_RATIO = 0.9f;

    private PlayerPlanetController PlayerPlanetController;

    void Start()
    {
        PlayerPlanetController = GetComponent<PlayerPlanetController>();
    }

    private void CreateShockwaveAtCollision(Collision2D collision, GameObject shockwavePrefab)
    {
        float collisionSpeedRatio = Mathf.Clamp01((collision.relativeVelocity.magnitude - PlayerPlanetController.MIN_SPEED) / (PlayerPlanetController.MAX_SPEED - PlayerPlanetController.MIN_SPEED));
        float magnitude = 1 + SPEED_RADIUS_RATIO * Mathf.Pow(collisionSpeedRatio, 3);

        Vector3 contactPoint = new Vector3(collision.GetContact(0).point.x, collision.GetContact(0).point.y, 0);
        GameObject shockwave = Instantiate(shockwavePrefab, contactPoint, Quaternion.identity);
        shockwave.GetComponent<Shockwave>().SetMagnitude(magnitude);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Ball")
        {
            CreateShockwaveAtCollision(collision, BallShockwavePrefab);
        }
        else
        if (collision.gameObject.tag == "Player")
        {
            CreateShockwaveAtCollision(collision, PlayerShockwavePrefab);
        }
    }
}
