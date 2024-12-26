using System;
using System.Collections;
using System.Collections.Generic;
using com.kleberswf.lib.core;
using Sirenix.OdinInspector;
using UnityEngine;
using Utility;

public class AdManager : Singleton<AdManager>
{
    [SerializeField]
    [InfoBox("스테이지에서 얼마나 많은 시간 머무를 때 다음 스테이지 진행시 광고를 호출할 지")] 
    private float adActiveSeconds;
    private float currentActiveSeconds;
    
    [SerializeField]
    [InfoBox("몇 개의 스테이지를 넘어갈 때 필수로 광고를 출력할지.")]
    private int maxCheckPuzzle;
    private int currentCheckPuzzle;
    private void Awake()
    {
        if (adActiveSeconds == 0)
        {
            adActiveSeconds = 300;
        }
        currentActiveSeconds = adActiveSeconds;

        if (maxCheckPuzzle == 0)
        {
            maxCheckPuzzle = 2;
        }
        currentCheckPuzzle = 0;
    }

    private void Update()
    {
        currentActiveSeconds -= Time.deltaTime;
    }

    /// <summary>
    /// 정답을 체크할 때 광고를 호출시킬지 판별 후 광고 출력
    /// </summary>
    public void CheckAd()
    {
        if (currentActiveSeconds < 0 || ++currentCheckPuzzle == maxCheckPuzzle)
        {
            // 광고 출력
            currentCheckPuzzle = 0;
            
            AdMobHandler.Instance.ShowInterstitialAd();
        }

        currentActiveSeconds = adActiveSeconds;
    }
}
