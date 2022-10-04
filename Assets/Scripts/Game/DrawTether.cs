using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class DrawTether : MonoBehaviour
{

    public PlayerPlanetController Player;
    public Color DisabledTetherColor = new Color(0.5f, 0.5f, 0.5f, 0.1f);
    public Color EnabledTetherColor = new Color(0.02352941f, 0.02352941f, 0.02352941f, 1.0f);

    private LineRenderer lineRenderer;
    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = lineRenderer.endColor = DisabledTetherColor;
        lineRenderer.widthMultiplier = 0.1f;
        lineRenderer.positionCount = 2;
    }

    // Update is called once per frame
    void Update()
    {
        lineRenderer.SetPosition(1, Player.transform.position);
        if (Player.IsTethered)
        {
            lineRenderer.startColor = lineRenderer.endColor = EnabledTetherColor;
            lineRenderer.SetPosition(0, Player.CenterPoint);
        }
        else
        {
            Planet nearestPlanet = Player.NearestPlanet();
            if (nearestPlanet == null)
            {
                // No Planet could be tethered
                Color invisibleColor = new Color(0, 0, 0, 0);
                lineRenderer.startColor = lineRenderer.endColor = invisibleColor;
            }
            else
            {
                lineRenderer.startColor = lineRenderer.endColor = DisabledTetherColor;
                lineRenderer.SetPosition(0, nearestPlanet.transform.position);
            }
        }
    }
}
