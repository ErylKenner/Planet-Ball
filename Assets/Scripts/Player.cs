using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int PlayerNumber;

    public PlayerInput ControllerInput = null;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(ControllerInput != null)
        {
            //add controls here
            if (Input.GetButtonDown(ControllerInput.Button("A")))
            {
                Debug.Log(name);
            }
        }
    }
}
