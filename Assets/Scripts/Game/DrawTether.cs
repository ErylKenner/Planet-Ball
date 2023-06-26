using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct RopeSection
{
    public Vector3 pos;
    public Vector3 oldPos;

    //To write RopeSection.zero
    public static readonly RopeSection zero = new RopeSection(Vector3.zero);

    public RopeSection(Vector3 pos)
    {
        this.pos = pos;

        this.oldPos = pos;
    }
}

[RequireComponent(typeof(LineRenderer))]
public class DrawTether : MonoBehaviour
{

    public PlayerPlanetController Player;
    public Color DISABLED_TETHER_COLOR = new Color(0.5f, 0.5f, 0.5f, 0.1f);
    public Color ENABLED_TETHER_COLOR = new Color(0.02352941f, 0.02352941f, 0.02352941f, 1.0f);
    public int SECTION_COUNT = 10;
    public float PLAYER_HANDLE_TOWARD = 0.01f;
    public float PLAYER_HANDLE_SIDEWAYS = 0.01f;
    public float PLANET_HANDLE_TOWARD = 0.01f;
    public float PLANET_HANDLE_SIDEWAYS = 0.01f;
    public float LINE_Z = 1f;

    private LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = lineRenderer.endColor = DISABLED_TETHER_COLOR;
        lineRenderer.widthMultiplier = 0.1f;
        lineRenderer.positionCount = SECTION_COUNT;
    }

    // Update is called once per frame
    void Update()
    {
        if (!Player.isLocalPlayer && !Player.playerState.IsTethered)
        {
            lineRenderer.enabled = false;
            return;
        }
        lineRenderer.enabled = true;
        if (Player.playerState.IsTethered)
        {
            DrawConnectedTether();
            return;
        }
        Planet nearestPlanet = Player.NearestPlanet();
        if (nearestPlanet != null && Player.playerState.TetherDisabledDuration <= 0f)
        {
            DrawUnconnectedTether(nearestPlanet.transform.position);
            return;
        }
        lineRenderer.startColor = lineRenderer.endColor = new Color(0, 0, 0, 0);
    }

    private void DrawConnectedTether()
    {
        lineRenderer.startColor = lineRenderer.endColor = ENABLED_TETHER_COLOR;
        lineRenderer.SetPosition(0, new Vector3(transform.position.x, transform.position.y, LINE_Z));
        for (int i = 1; i < lineRenderer.positionCount - 1; ++i)
        {
            Vector2 A = transform.position;
            Vector2 D = Player.playerState.CenterPoint;
            Vector2 B = (1 + PLAYER_HANDLE_TOWARD) * A - PLAYER_HANDLE_TOWARD * D - PLAYER_HANDLE_SIDEWAYS * Player.playerState.OrbitRadius * Player.GetComponent<Rigidbody2D>().velocity;
            Vector2 C = (1 + PLANET_HANDLE_TOWARD) * D - PLANET_HANDLE_TOWARD * A - PLANET_HANDLE_SIDEWAYS * Player.playerState.OrbitRadius * Player.GetComponent<Rigidbody2D>().velocity;
            float t = (float)i / (lineRenderer.positionCount - 2);
            Vector2 position = DeCasteljausAlgorithm(A, B, C, D, t);
            lineRenderer.SetPosition(i, new Vector3(position.x, position.y, LINE_Z));
        }
        lineRenderer.SetPosition(lineRenderer.positionCount - 1, new Vector3(Player.playerState.CenterPoint.x, Player.playerState.CenterPoint.y, LINE_Z));
    }

    private void DrawUnconnectedTether(Vector2 nearestPlanetPosition)
    {
        lineRenderer.startColor = lineRenderer.endColor = DISABLED_TETHER_COLOR;
        lineRenderer.SetPosition(0, new Vector3(transform.position.x, transform.position.y, LINE_Z));
        for (int i = 1; i < lineRenderer.positionCount; ++i)
        {
            lineRenderer.SetPosition(i, new Vector3(nearestPlanetPosition.x, nearestPlanetPosition.y, LINE_Z));
        }
        lineRenderer.SetPosition(lineRenderer.positionCount - 1, new Vector3(nearestPlanetPosition.x, nearestPlanetPosition.y, LINE_Z));
    }


    private Vector2 DeCasteljausAlgorithm(Vector2 A, Vector2 B, Vector2 C, Vector2 D, float t)
    {
        //Layer 1
        Vector2 Q = (1f - t) * A + t * B;
        Vector2 R = (1f - t) * B + t * C;
        Vector2 S = (1f - t) * C + t * D;

        //Layer 2
        Vector2 P = (1f - t) * Q + t * R;
        Vector2 T = (1f - t) * R + t * S;

        //Final interpolated position
        return (1f - t) * P + t * T;
    }
}
