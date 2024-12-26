using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utility;

namespace Stage.UI
{
    public class ResultPopup : MonoBehaviour
    {
        [SerializeField] 
        private List<DOTweenAnimation> stars; 
        
        [SerializeField] 
        private TMP_Text costText;

        [SerializeField] 
        private Button restartButton;
        [SerializeField] 
        private Button menuButton;
        [SerializeField] 
        private Button continueButton;

        [SerializeField] 
        private AudioClip sfx;
        [SerializeField] 
        private AudioClip buttonSfx;
        
        private void Start()
        {
            restartButton.enabled = menuButton.enabled = continueButton.enabled = false;

            restartButton.onClick.AddListener(GameManager.Instance.RestartPuzzle);
            menuButton.onClick.AddListener(GameManager.Instance.LoadStage);
            continueButton.onClick.AddListener(GameManager.Instance.LoadNextPuzzle);
            
            restartButton.onClick.AddListener(() =>
            {
                SoundManager.Instance.PlaySFX(buttonSfx);
            });
            menuButton.onClick.AddListener(() =>
            {
                SoundManager.Instance.PlaySFX(buttonSfx);
            });
            continueButton.onClick.AddListener(() =>
            {
                SoundManager.Instance.PlaySFX(buttonSfx);
            });
        }

        public IEnumerator Activate(int numOfStar, int cost)
        {
            yield return null;
            while (Time.timeScale == 0)
            {
                yield return null;
            }
            gameObject.SetActive(true);
            SoundManager.Instance.PlaySFX(sfx);
            yield return new WaitForSeconds(0.5f);
            costText.DOText(cost.ToString(), 2f, true, ScrambleMode.Numerals);
            for (int i = 0; i < numOfStar; i++)
            {
                stars[i].DORestart();
                yield return new WaitForSeconds(0.2f);
            }
            restartButton.enabled = menuButton.enabled = continueButton.enabled = true;
        }
    }
}
