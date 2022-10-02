using UnityEngine;

public static class ConfigManager
{
    public static bool UseSteamworks
    {
        get
        {
            return SteamManager.Initialized && !Application.isEditor && !Debug.isDebugBuild;
        }
    }
}
