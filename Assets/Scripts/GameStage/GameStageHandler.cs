using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameStage
{
    public class GameStageHandler : MonoBehaviour
    {
        [SerializeField] 
        private TMP_Text title;
        
        [SerializeField] 
        private Button backButton;
        
        [SerializeField] 
        private List<PuzzleComponent> puzzleComponents;

        [SerializeField] 
        private AudioClip sfx;
        
        public GameStageScriptableObject gameStageData;

        private void Start()
        {
            backButton.onClick.AddListener(() =>
            {
                GameManager.Instance.LoadScene("StageSelectMenu");
            });
            backButton.onClick.AddListener(() =>
            {
                SoundManager.Instance.PlaySFX(sfx);
            });
        }

        public void SetupData()
        {
            title.text = gameStageData.stageName;
            for (int i = 0; i < puzzleComponents.Count; i++)
            {
                PuzzleComponent component = puzzleComponents[i];
                if (PlayerPrefs.HasKey(gameStageData.stageData[i].name))
                {
#if UNITY_EDITOR
                    Debug.Log(i + " component has Key! value: " + PlayerPrefs.GetInt(gameStageData.stageData[i].name));
#endif
                    component.SetStar(PlayerPrefs.GetInt(gameStageData.stageData[i].name));
                }
                component.puzzleData = gameStageData.stageData[i];
                component.idx = i;
                component.GetComponent<Button>().onClick.AddListener(() =>
                {
                    GameManager.Instance.LoadPuzzle(component.idx);
                });
            }    
            SoundManager.Instance.PlayBGM(gameStageData.bgm, 1f);
        }
    }
}
