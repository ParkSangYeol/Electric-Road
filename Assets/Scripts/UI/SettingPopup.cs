using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class SettingPopup : MonoBehaviour
    {
        [SerializeField] 
        private Button backButton;
        
        [SerializeField] 
        private AudioClip backButtonSfx;

        [SerializeField]
        private SettingComponent bgmComponent;
        [SerializeField]
        private SettingComponent sfxComponent;

        private void Start()
        {
            backButton.onClick.AddListener(() =>
            {
                SoundManager.Instance.PlaySFX(backButtonSfx);
            });
            backButton.onClick.AddListener(() =>
            {
                gameObject.SetActive(false);
            });
            
            bgmComponent.onValueChange.AddListener((num) =>
            {
                SoundManager.Instance.ChangeBGMVolume((float)num / 5);
            });
             
            sfxComponent.onValueChange.AddListener((num) =>
            {
                SoundManager.Instance.ChangeSFXVolume((float)num / 5);
            });
        }
    }

}
