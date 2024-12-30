using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tutorial
{
    [CreateAssetMenu(menuName = "Assets/ScriptableObject/TutorialData", fileName = "TutorialData")]
    public class TutorialScriptableObject : ScriptableObject
    {
        [InfoBox("튜토리얼이 사용되는 스테이지 이름")]
        public string stageName; 
        public List<TutorialUnitScriptableObject> tutorialData;
    }
}

