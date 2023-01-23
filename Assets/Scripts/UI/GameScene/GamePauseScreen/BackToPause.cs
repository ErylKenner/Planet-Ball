using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackToPause : MonoBehaviour
{
    public void Back()
    {
        UIAccessor accessor = FindObjectOfType<UIAccessor>();
        accessor.Options.SetActive(false);
        accessor.Pause.SetActive(true);
    }
}
