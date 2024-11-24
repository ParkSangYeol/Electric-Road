using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class StageComponent : MonoBehaviour
    {
        public GameStageScriptableObject gameStageData;

        [SerializeField] 
        private TMP_Text title;
        
        [SerializeField] 
        private Image thumbnail;

        [SerializeField] 
        private AudioClip sfx;

        private void Start()
        {
            GetComponent<Button>().onClick.AddListener(() =>
            {
                SoundManager.Instance.PlaySFX(sfx);
            });
        }

        public void SetComponent(GameStageScriptableObject data)
        {
            title.text = data.stageName;
            gameStageData = data;

            thumbnail.sprite = gameStageData.thumbnail;
            GetComponent<Button>().onClick.AddListener(() =>
            {
                GameManager.Instance.LoadStage(gameStageData);
            });
        }
    }
}
