using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TeamManager))]
public class ContextManager : MonoBehaviour
{
    public static ContextManager instance = null;
    public TeamManager TeamManager;

    void Start()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);

            // Add each manager here
            TeamManager = GetComponent<TeamManager>();
        }
        else
        {
            Debug.LogWarning("There can only be one ContextManager");
            Destroy(this.gameObject);
            return;
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}
