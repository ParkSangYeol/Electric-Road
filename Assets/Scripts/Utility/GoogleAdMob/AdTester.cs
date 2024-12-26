using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Utility;

namespace MyNamespace
{
    public class AdTester : MonoBehaviour
    {
#if UNITY_EDITOR
        [Button]
        public void TestAd()
        {
            AdMobHandler.Instance.ShowInterstitialAd();
        }
#endif
    }
}

