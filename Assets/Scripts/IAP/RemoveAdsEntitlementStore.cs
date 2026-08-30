using System;
using UnityEngine;

public static class RemoveAdsEntitlementStore
{
    private const string RemoveAdsKey = "iap_remove_ads_entitled";

    public static event Action<bool> OnEntitlementChanged;

    public static bool HasRemoveAds => PlayerPrefs.GetInt(RemoveAdsKey, 0) == 1;

    public static void SetRemoveAds(bool hasRemoveAds)
    {
        if (HasRemoveAds == hasRemoveAds)
        {
            return;
        }

        PlayerPrefs.SetInt(RemoveAdsKey, hasRemoveAds ? 1 : 0);
        PlayerPrefs.Save();
        OnEntitlementChanged?.Invoke(hasRemoveAds);
    }
}
