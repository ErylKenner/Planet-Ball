using NUnit.Framework;
using UnityEngine;

public class Test_RotationalPhysics
{
    float accumulator;
    private GameObject player;
    private Player pl;
    private GameObject planet;

    [SetUp]
    public void Setup()
    {
        accumulator = 0.0f;

        planet = new GameObject();
        planet.AddComponent<SpriteRenderer>();
        planet.AddComponent<Shadow>();
        planet.AddComponent<Planet>();
        planet.transform.position = new Vector2(0, 0);

        player = new GameObject();
        player.AddComponent<SpriteRenderer>();
        player.AddComponent<Rigidbody2D>();
        player.AddComponent<CircleCollider2D>();
        player.AddComponent<Shadow>();
        player.AddComponent<LineRenderer>();
        player.AddComponent<Player>();
        pl = player.GetComponent<Player>();
        pl.Body.position = new Vector2(100, 0);
        pl.Body.velocity = new Vector2(0, -pl.DefaultSpeed);
        pl.Speed = pl.Body.velocity.magnitude;

        pl.AttachTether();
    }

    [TearDown]
    public void Teardown()
    {

    }

    [Test]
    public void RotateAroundPoint_ReelMedium()
    {
        while (pl.AttachedPlanetRadius > 50.0f && accumulator < 0.35f)
        {
            accumulator += Time.fixedDeltaTime;
            RotationalPhysics.RotateAroundPoint(pl.Body, pl.AttachedPlanet.transform.position, 50, pl.Speed, Time.fixedDeltaTime);
            pl.Body.position += pl.Body.velocity * Time.fixedDeltaTime;
        }
        Assert.AreEqual(50, Vector2.Distance(pl.Body.position, planet.transform.position), 5);
    }


    [Test]
    public void RotateAroundPoint_NormalUseShort()
    {
        while (accumulator < 0.785398f)
        {
            accumulator += Time.fixedDeltaTime;
            RotationalPhysics.RotateAroundPoint(pl.Body, pl.AttachedPlanet.transform.position, pl.AttachedPlanetRadius, pl.Speed, Time.fixedDeltaTime);
            pl.Body.position += pl.Body.velocity * Time.fixedDeltaTime;
        }
        Assert.AreEqual(0, pl.Body.position.x, 5);
        Assert.AreEqual(-100, pl.Body.position.y, 5);
    }

    [Test]
    public void RotateAroundPoint_NormalUseMedium()
    {
        while (accumulator < 1.5708f)
        {
            accumulator += Time.fixedDeltaTime;
            RotationalPhysics.RotateAroundPoint(pl.Body, pl.AttachedPlanet.transform.position, pl.AttachedPlanetRadius, pl.Speed, Time.fixedDeltaTime);
            pl.Body.position += pl.Body.velocity * Time.fixedDeltaTime;
        }
        Assert.AreEqual(-100, pl.Body.position.x, 5);
        Assert.AreEqual(0, pl.Body.position.y, 5);
    }

    [Test]
    public void RotateAroundPoint_NormalUseLong()
    {
        while (accumulator < 15.708f)
        {
            accumulator += Time.fixedDeltaTime;
            RotationalPhysics.RotateAroundPoint(pl.Body, pl.AttachedPlanet.transform.position, pl.AttachedPlanetRadius, pl.Speed, Time.fixedDeltaTime);
            pl.Body.position += pl.Body.velocity * Time.fixedDeltaTime;
        }
        Assert.AreEqual(100, pl.Body.position.x, 5);
        Assert.AreEqual(0, pl.Body.position.y, 5);
    }


    [Test]
    public void PlayerSanity()
    {
        Assert.IsNotNull(player.GetComponent<Player>().Body);
        Assert.AreEqual(100, player.GetComponent<Player>().AttachedPlanetRadius);
    }


    [Test]
    public void ConvertToUnitTangentialVelocity_NormalUse()
    {
        Vector2 position = new Vector2(9, 13);
        Vector2 velocity = new Vector2(-2, -9);
        Vector2 center = new Vector2(0, 1);
        Vector2 output = RotationalPhysics.ConvertToUnitTangentialVelocity(position, velocity, center);
        Assert.AreEqual(new Vector2(0.8f, -0.6f), output);
    }

    [Test]
    public void ConvertToUnitTangentialVelocity_Zeros()
    {
        Vector2 position = new Vector2(0, 0);
        Vector2 velocity = new Vector2(0, 0);
        Vector2 center = new Vector2(0, 0);
        Vector2 output = RotationalPhysics.ConvertToUnitTangentialVelocity(position, velocity, center);
        Assert.AreEqual(new Vector2(0.0f, -0.0f), output);
    }

    [Test]
    public void ConvertToUnitTangentialVelocity_Horizontal()
    {
        Vector2 position = new Vector2(9, 0);
        Vector2 velocity = new Vector2(0, 1);
        Vector2 center = new Vector2(0, 0);
        Vector2 output = RotationalPhysics.ConvertToUnitTangentialVelocity(position, velocity, center);
        Assert.AreEqual(new Vector2(0.0f, 1.0f), output);
    }

    [Test]
    public void ConvertToUnitTangentialVelocity_VelocityAlongRadius()
    {
        Vector2 position = new Vector2(9, 0);
        Vector2 velocity = new Vector2(-3, 0);
        Vector2 center = new Vector2(0, 0);
        Vector2 output = RotationalPhysics.ConvertToUnitTangentialVelocity(position, velocity, center);
        Assert.AreEqual(new Vector2(0.0f, -1.0f), output);
    }

}