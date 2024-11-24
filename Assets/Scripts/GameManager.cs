using System;
using System.Collections;
using System.Collections.Generic;
using GameStage;
using ScriptableObjects.Stage;
using Sirenix.OdinInspector;
using Stage;
using Unity.VisualScripting;
using UnityEditor.SceneManagement;
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
    
    private void Start()
    {
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        SceneManager.sceneLoaded += SetupPuzzle;
        SceneManager.sceneLoaded += SetupStage;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= SetupPuzzle;
        SceneManager.sceneLoaded -= SetupStage;
    }

    public void LoadScene(string sceneName)
    {
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

    #endregion
    
}
