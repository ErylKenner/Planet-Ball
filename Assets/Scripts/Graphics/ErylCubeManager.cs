using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ErylCubeManager : MonoBehaviour
{

    public List<GameObject> ErylCubes;
    public float SPAWN_FREQUENCY = 5f;
    public Transform spawnLocation;


    float timer = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.unscaledDeltaTime;
        if (timer >= SPAWN_FREQUENCY)
        {
            timer -= SPAWN_FREQUENCY;
            int i = Random.Range(0, ErylCubes.Count);
            Instantiate(ErylCubes[i], spawnLocation.position, spawnLocation.rotation);
        }
    }
}
