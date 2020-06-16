using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effects : MonoBehaviour
{
    public GameObject ShockwavePrefab;

    private void Awake()
    {
        Player.OnPlayerCollision += InstantiateShockwave;
    }

    private void InstantiateShockwave(Vector2 position)
    {
        Instantiate(ShockwavePrefab, position, Quaternion.identity);
    }
}
