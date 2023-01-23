using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowOptions : MonoBehaviour
{
    public void Show()
    {
        UIAccessor accessor = FindObjectOfType<UIAccessor>();
        accessor.Options.SetActive(true);
        accessor.Pause.SetActive(false);
    }
}
