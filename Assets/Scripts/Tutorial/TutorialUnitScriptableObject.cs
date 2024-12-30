using System.Collections;
using System.Collections.Generic;
using ScriptableObjects.Stage;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tutorial
{
    [CreateAssetMenu(menuName = "Assets/ScriptableObject/TutorialUnit", fileName = "TutorialUnit")]
    public class TutorialUnitScriptableObject : ScriptableObject
    {
        [LabelText("튜토리얼 Key")]
        public string tutorialKey;
        
        [LabelText("맵 변경여부")]
        public bool isChangeMap;
        [ShowIf("isChangeMap")]
        [LabelText("맵 데이터")]
        public StageScriptableObject mapData;
        
        [LabelText("클리어 가능여부")]
        public bool isClearAble;
        
        [AssetsOnly]
        [LabelText("애니메이션 프리팹")]
        public GameObject animPrefab;
    }
}