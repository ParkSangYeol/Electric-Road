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
        private Transform canvasTransform;
        
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
            bool isUnLocklastComponent = true;
            CreatePuzzleComponents();
            
            for (int i = 0; i < puzzleComponents.Count; i++)
            {
                PuzzleComponent component = puzzleComponents[i];
                if (i == 0 && gameStageData.hasTutorial)
                {
                    component.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        GameManager.Instance.LoadTutorialPuzzle();
                    });
                }
                else
                {
                    var buttonComponent = component.GetComponent<Button>();
                    if (PlayerPrefs.HasKey(gameStageData.stageData[stageIdx].name))
                    {
                        // 클리어 기록 있음
#if UNITY_EDITOR
                        Debug.Log(i + " component has Key! value: " + PlayerPrefs.GetInt(gameStageData.stageData[stageIdx].name));
#endif
                        component.SetStar(PlayerPrefs.GetInt(gameStageData.stageData[stageIdx].name));
                        isUnLocklastComponent = true;
                    }
                    else if (isUnLocklastComponent)
                    {
                        // 클리어 기록 없음, 이전 항목은 락이 없음
                        isUnLocklastComponent = false;
                    }
                    else
                    {
                        // 클리어 기록 없음, 이전 항목 락이 되어있음.
                        component.SetLock(true);
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

        private void CreatePuzzleComponents()
        {
            // 퍼즐 Components 생성
            GameObject puzzleComponentsObject = Instantiate(gameStageData.puzzleComponentsPrefab, canvasTransform);
            
            // 퍼즐 Component 변수 설정
            puzzleComponents = new List<PuzzleComponent>();
            foreach (var puzzleComponent in puzzleComponentsObject.GetComponentsInChildren<PuzzleComponent>())
            {
                puzzleComponents.Add(puzzleComponent);
            }
        }
    }
}
