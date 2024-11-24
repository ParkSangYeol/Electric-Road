using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class StageSelector : MonoBehaviour
    {
        [SerializeField] 
        private List<StageComponent> stageComponents;

        [SerializeField] 
        private List<GameStageScriptableObject> stageData;

        [SerializeField] 
        private AudioClip sfx;
        
        [SerializeField] 
        private Button backButton;
        
        private void Start()
        {
            for (int i = 0; i < stageComponents.Count; i++)
            {
                stageComponents[i].SetComponent(stageData[i]);
            }
            
            backButton.onClick.AddListener(() =>
            {
                GameManager.Instance.LoadScene("MainMenu");
            });
            backButton.onClick.AddListener(() =>
            {
                SoundManager.Instance.PlaySFX(sfx);
            });
        }
    }
}

