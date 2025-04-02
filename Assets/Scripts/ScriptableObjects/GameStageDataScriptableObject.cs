using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "GameStageDataScriptableObject", menuName = "Scriptable Objects/GameStageDataScriptableObject")]
public class GameStageDataScriptableObject : ScriptableObject
{
    public List<GameStageScriptableObject> gameStageList;
    public string allClearAchievementKey;
    
    #if UNITY_EDITOR
    [Button]
    private void CreateGameStageList()
    {
        gameStageList = new List<GameStageScriptableObject>();
        string[] guids = AssetDatabase.FindAssets("t:GameStageScriptableObject");
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameStageScriptableObject stageData = AssetDatabase.LoadAssetAtPath<GameStageScriptableObject>(path);
            if (stageData != null)
            {
                gameStageList.Add(stageData);
            }
        }
    }
    
    #endif
}
