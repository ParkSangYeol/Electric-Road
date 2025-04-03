using System;
using System.Collections;
using System.Collections.Generic;
using Stove.PCSDK.NET;
using UnityEngine;

public class StoveAchievementHandler
{
    internal static StovePCResult UnlockAchievement(String statId)
    {
        return StovePC.SetStat(statId.ToUpper(), 1);
    }

    internal static StovePCResult SetStat(string statId, int count)
    {
        return StovePC.SetStat(statId.Substring(0, statId.LastIndexOf("_")).ToUpper(), count);
    }
}
