using System.Collections;
using System.Collections.Generic;
using ScriptableObjects.Stage;
using Sirenix.OdinInspector;
using Tutorial;
using UnityEngine;

[CreateAssetMenu(menuName = "Assets/ScriptableObject/GameStage")]
public class GameStageScriptableObject : SerializedScriptableObject
{
    [HideLabel, PreviewField(55)]
    public Sprite thumbnail;
    public string stageName;
    public List<StageScriptableObject> stageData;
    [AssetsOnly]
    public GameObject puzzleComponentsPrefab;
    public AudioClip bgm;
    public bool hasTutorial;
    [ShowIf("hasTutorial")] 
    public TutorialScriptableObject tutorial;
    
#if !DISABLESTEAMWORKS
    public string ClearAchievementKey;
    public string PerfectClearAchievementKey;
#endif
}
