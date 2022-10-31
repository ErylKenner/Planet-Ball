using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrawGas : MonoBehaviour
{
    public PlayerPlanetController Player;
    public GameObject GasImage;

    private float originalHeight;
    // Start is called before the first frame update
    void Start()
    {
        originalHeight = GasImage.GetComponent<RectTransform>().sizeDelta.y;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 sizeDelta = GasImage.GetComponent<RectTransform>().sizeDelta;
        sizeDelta.y = Player.CurGas * originalHeight;
        GasImage.GetComponent<RectTransform>().sizeDelta = sizeDelta;
    }
}
