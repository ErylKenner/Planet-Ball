using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectMe : MonoBehaviour
{
    private void OnEnable()
    {
        GetComponent<Selectable>().Select();
    }
}
