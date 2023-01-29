using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TeamManager))]
[RequireComponent(typeof(AdminManager))]
public class ContextManager : MonoBehaviour
{
    public static ContextManager instance = null;
    [HideInInspector]
    public TeamManager TeamManager;
    [HideInInspector]
    public AdminManager AdminManager;
    [HideInInspector]
    public SoundManager SoundManager;

    void Start()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);

            // Add each manager here
            TeamManager = GetComponent<TeamManager>();
            AdminManager = GetComponent<AdminManager>();
            SoundManager = transform.GetComponentInChildren<SoundManager>();
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
