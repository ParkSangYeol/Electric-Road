using System.Collections;
using System.Collections.Generic;
using ScriptableObjects.Stage;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Assets/ScriptableObject/GameStage")]
public class GameStageScriptableObject : SerializedScriptableObject
{
    [HideLabel, PreviewField(55)]
    public Sprite thumbnail;
    public string stageName;
    public List<StageScriptableObject> stageData;
}
