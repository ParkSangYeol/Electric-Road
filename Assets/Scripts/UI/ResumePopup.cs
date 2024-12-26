using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Stage.UI
{
    public class ResumePopup : MonoBehaviour
    {
        [SerializeField]
        private AudioClip clickSFX;

        [SerializeField] 
        private TMP_Text titleTxt;
        
        [SerializeField] 
        private Button resumeBtn;
        [SerializeField] 
        private Button settingBtn;
        [SerializeField] 
        private Button quitBtn;

        private void Start()
        {
            // InitSFX();
            InitBtnEvent();
        }

        public void SetTitle(string title)
        {
            titleTxt.text = title;
        }
        
        private void InitSFX()
        {
            resumeBtn.onClick.AddListener(() =>
            {
                SoundManager.Instance.PlaySFX(clickSFX);
            });
            settingBtn.onClick.AddListener(() =>
            {
                SoundManager.Instance.PlaySFX(clickSFX);
            });
            quitBtn.onClick.AddListener(() =>
            {
                SoundManager.Instance.PlaySFX(clickSFX);
            });
        }

        private void InitBtnEvent()
        {
            quitBtn.onClick.AddListener(GameManager.Instance.LoadStage);
        }
    }
}
