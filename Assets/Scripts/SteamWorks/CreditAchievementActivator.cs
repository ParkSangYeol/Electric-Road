using UnityEngine;

public class CreditAchievementActivator : MonoBehaviour
{
    [SerializeField] private string creditAchievementKey;
    
    public void ActivateCreditAchievement()
    {
#if !DISABLESTEAMWORKS
        SteamAchievement.Instance.Achieve(creditAchievementKey);
#endif
#if !DISABLESTOVE
        StoveAchievementHandler.UnlockAchievement(creditAchievementKey);
#endif
    }
}
