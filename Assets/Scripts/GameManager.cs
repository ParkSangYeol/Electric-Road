using System;
using System.Collections;
using System.Collections.Generic;
using GameStage;
using ScriptableObjects.Stage;
using Sirenix.OdinInspector;
using Stage;
using Tutorial;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : com.kleberswf.lib.core.Singleton<GameManager>
{
    [ShowInInspector, ReadOnly]
    private GameStageScriptableObject currentStageData;
    [ShowInInspector, ReadOnly]
    private StageScriptableObject currentPuzzleData;
    [ShowInInspector, ReadOnly]
    private int puzzleIdx;

#if UNITY_STANDALONE_WIN
    private float targetAspectRatio;
    private int lastWidth;
    private int lastHeight;
#endif
    
    [SerializeField] 
    private AudioClip mainBGM;
    
    private void Start()
    {
#if UNITY_STANDALONE_WIN
        Screen.SetResolution(800, 375, FullScreenMode.Windowed);
        targetAspectRatio = (float)800 / 375;
        lastWidth = Screen.width;
        lastHeight = Screen.height;
#endif
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        SceneManager.sceneLoaded += SetupPuzzle;
        SceneManager.sceneLoaded += SetupStage;
        SceneManager.sceneLoaded += SetupTutorial;
        
        SoundManager.Instance.PlayBGM(mainBGM, 1f);
    }

#if UNITY_STANDALONE_WIN
    private void Update()
    {
        if (Screen.width != lastWidth || Screen.height != lastHeight)
        {
            float currentAspect = (float)Screen.width / Screen.height;
            
            if (Mathf.Abs(currentAspect - targetAspectRatio) > 0.01f)
            {
                // 현재 너비를 기준으로 높이를 조정
                int newHeight = Mathf.RoundToInt(Screen.width / targetAspectRatio);
                Screen.SetResolution(Screen.width, newHeight, false);
            }

            lastWidth = Screen.width;
            lastHeight = Screen.height;
        }
    }
#endif
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= SetupPuzzle;
        SceneManager.sceneLoaded -= SetupStage;
    }

    public void LoadScene(string sceneName)
    {
        if (sceneName == "StageSelectMenu" && SoundManager.Instance.currentBGM != mainBGM)
        {
            SoundManager.Instance.PlayBGM(mainBGM, 1f);
        }
        SceneManager.LoadScene(sceneName);
    }

    #region About Puzzle

    [Button]
    public void LoadPuzzle(StageScriptableObject data)
    {
        currentPuzzleData = data;
        SceneManager.LoadScene("PuzzleScene");
    }
    
    public void LoadPuzzle(int idx)
    {
        puzzleIdx = idx;
        currentPuzzleData = currentStageData.stageData[puzzleIdx];
        SceneManager.LoadScene("PuzzleScene");
    }

    public void LoadNextPuzzle()
    {
        if (++puzzleIdx >= 10)
        {
            LoadStage();
        }
        else
        {
            LoadPuzzle(puzzleIdx);
        }
    }
    public void RestartPuzzle()
    {
        LoadPuzzle(currentPuzzleData);
    }

    public void LoadTutorialPuzzle()
    {
        if (!currentStageData.hasTutorial)
        {
            return;
        }
        puzzleIdx = -1;
        SceneManager.LoadScene("TutorialScene");
    }

    #endregion

    #region About Stage

    public void LoadStage()
    {
        LoadStage(currentStageData);
    }

    [Button]
    public void LoadStage(GameStageScriptableObject data)
    {
        currentStageData = data;
        SceneManager.LoadScene("StageScene");
    }

    #endregion

    #region About Event

    private void SetupPuzzle(Scene scene, LoadSceneMode sceneMode)
    {
        if (scene.name != "PuzzleScene")
        {
            return;
        }
        
        StageHandler stageHandler = GameObject.Find("StageManager").GetComponent<StageHandler>();
        stageHandler.stageData = currentPuzzleData;
        stageHandler.ResetStage();
    }
    
    private void SetupStage(Scene scene, LoadSceneMode sceneMode)
    {
        if (scene.name != "StageScene")
        {
            return;
        }
        
        GameStageHandler gameStageHandler = GameObject.Find("GameStageManager").GetComponent<GameStageHandler>();
        gameStageHandler.gameStageData = currentStageData;
        gameStageHandler.SetupData();
    }

    private void SetupTutorial(Scene scene, LoadSceneMode sceneMode)
    {
        if (scene.name != "TutorialScene")
        {
            return;
        }

        TutorialHandler tutorialHandler = GameObject.Find("TutorialHandler").GetComponent<TutorialHandler>();
        tutorialHandler.SetTutorial(currentStageData.tutorial);
    }

    #endregion
    
}
