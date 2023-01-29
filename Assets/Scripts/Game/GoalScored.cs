using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalScored : NetworkBehaviour
{
    public GameObject GoalExplosion;
    public Transform ExplosionLocation;
    public int TeamNumber;

    private Team team;
    private void Start()
    {
        team = ContextManager.instance.TeamManager.GetTeam(TeamNumber);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isServer)
        {
            return;
        }
        Ball ball = other.GetComponent<Ball>();
        if (ball != null)
        {
            // Play explosion
            RPCCreateExplosion();

            ScoreManager scoreManager = NetcodeManager.instance.GetComponent<ScoreManager>();
            scoreManager.TeamScored(TeamNumber);
        }
    }

    [ClientRpc]
    public void RPCCreateExplosion()
    {
        var explosion = Instantiate(GoalExplosion, ExplosionLocation.position, ExplosionLocation.rotation);
        var particleSystem = explosion.GetComponent<ParticleSystem>();
        var main = particleSystem.main;
        main.startColor = team.TeamColor;
        var trails = particleSystem.trails;
        trails.colorOverTrail = team.TeamColor;
        particleSystem.Play();
    }
}
