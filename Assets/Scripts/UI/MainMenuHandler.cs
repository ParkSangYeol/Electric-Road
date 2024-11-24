using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class MainMenuHandler : MonoBehaviour
    {
        [SerializeField] 
        private Button startButton;
        [SerializeField] 
        private Button settingButton;
        [SerializeField] 
        private Button exitButton;

        private void Start()
        {
            startButton.onClick.AddListener(() =>
            {
                GameManager.Instance.LoadScene("StageSelectMenu");
            });
            
            exitButton.onClick.AddListener(() =>
            {
                #if UNITY_EDITOR
                
                    UnityEditor.EditorApplication.isPlaying = false;
                
                #else
                
                    Application.Quit();
                
                #endif
            });
        }
    }
}