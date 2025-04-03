using System.Collections;
using System.Collections.Generic;
using com.kleberswf.lib.core;
using UnityEngine;
#if !DISABLESTEAMWORKS
using Steamworks;
#endif

public class SteamAchievement : Singleton<SteamAchievement>
{
    #if !DISABLESTEAMWORKS
    public void Achieve(string apiName)
    {
        if (SteamManager.Initialized)
        {
            if (!IsAchieved(apiName))
            {
                SteamUserStats.SetAchievement(apiName);
                SteamUserStats.StoreStats();
            }
        }
    }

    public bool IsAchieved(string apiName)
    {
        if (!SteamManager.Initialized)
        {
            return false;
        }
        
        Steamworks.SteamUserStats.GetAchievement(apiName, out bool isAchieved);
        return isAchieved;

    }
    #endif
}