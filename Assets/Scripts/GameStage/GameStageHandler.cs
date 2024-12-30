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
        public GameStageScriptableObject gameStageData;

        private void Start()
        {
            backButton.onClick.AddListener(() =>
            {
                GameManager.Instance.LoadScene("StageSelectMenu");
            });
        }

        public void SetupData()
        {
            title.text = gameStageData.stageName;
            int stageIdx = 0;
            for (int i = 0; i < puzzleComponents.Count; i++)
            {
                PuzzleComponent component = puzzleComponents[i];
                if (i == 0 && !gameStageData.stageName.Equals("Town"))
                {
                    component.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        GameManager.Instance.LoadTutorialPuzzle();
                    });
                }
                else
                {
                    if (PlayerPrefs.HasKey(gameStageData.stageData[stageIdx].name))
                    {
#if UNITY_EDITOR
                        Debug.Log(i + " component has Key! value: " + PlayerPrefs.GetInt(gameStageData.stageData[stageIdx].name));
#endif
                        component.SetStar(PlayerPrefs.GetInt(gameStageData.stageData[stageIdx].name));
                    }
                    component.puzzleData = gameStageData.stageData[stageIdx];
                    component.idx = stageIdx;
                    stageIdx++;
                    component.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        GameManager.Instance.LoadPuzzle(component.idx);
                    }); 
                }
            }    
            SoundManager.Instance.PlayBGM(gameStageData.bgm, 1f);
        }
    }
}
