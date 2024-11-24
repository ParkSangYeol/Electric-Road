using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class StageComponent : MonoBehaviour
    {
        public GameStageScriptableObject gameStageData;

        [SerializeField] 
        private TMP_Text title;
        
        [SerializeField] 
        private Image thumbnail;
        
        public void SetComponent(GameStageScriptableObject data)
        {
            title.text = data.stageName;
            gameStageData = data;

            thumbnail.sprite = gameStageData.thumbnail;
            GetComponent<Button>().onClick.AddListener(() =>
            {
                GameManager.Instance.LoadStage(gameStageData);
            });            
        }
    }
}
