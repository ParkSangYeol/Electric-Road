using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Stage;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace Tutorial
{
    public class TutorialHandler : MonoBehaviour
    {
        public TutorialScriptableObject tutorialData;
        [SerializeField] 
        private StageHandler stageHandler;

        [SerializeField] 
        private TMP_Text titleText;
        [SerializeField] 
        private TMP_Text descriptionText;
        
        [SerializeField] 
        private Button leftButton;
        [SerializeField] 
        private Button rightButton;
        
        [SerializeField] 
        private AudioClip buttonSFX;

        [SerializeField] 
        private Transform spawnPoint;
        
        private List<GameObject> tutorialAnims;
        private int currentIdx;

        [Button]
        public void SetTutorial(TutorialScriptableObject tutorialData)
        {
            this.tutorialData = tutorialData;
            
            // 버튼 설정
            leftButton.onClick.AddListener(() =>
            {
                if (currentIdx <= 0)
                {
                    return;
                } 
                SetTutorialUnit(currentIdx - 1);
            });
            leftButton.onClick.AddListener(() => SoundManager.Instance.PlaySFX(buttonSFX));
            
            rightButton.onClick.AddListener(() =>
            {
                if (currentIdx >= tutorialData.tutorialData.Count -1)
                {
                    return;
                } 
                SetTutorialUnit(currentIdx + 1);
            });
            rightButton.onClick.AddListener(() => SoundManager.Instance.PlaySFX(buttonSFX));
            
            // 애니메이션 오브젝트들 생성.
            tutorialAnims = new List<GameObject>();
            foreach (var tutorialUnit in this.tutorialData.tutorialData)
            {
                if (tutorialUnit.animPrefab != null)
                {
                    var anim = Instantiate(tutorialUnit.animPrefab, spawnPoint);
                    tutorialAnims.Add(anim);
                    anim.SetActive(false);
                }
                else
                {
                    tutorialAnims.Add(null);
                }
            }
            
            // 0번 튜토리얼 활성화
            SetTutorialUnit(0);
        }

        private void SetTutorialUnit(int idx)
        {
            // 텍스트 교체
            var titleLocalizedString = new LocalizedString("TutorialTitleTable", tutorialData.tutorialData[idx].tutorialKey);
            var descriptionLocalizedString = new LocalizedString("TutorialDescriptionTable", tutorialData.tutorialData[idx].tutorialKey);

            titleLocalizedString.StringChanged += value =>
            {
                titleText.text = value;
            };
            descriptionLocalizedString.StringChanged += value =>
            {
                descriptionText.text = value;
            };
            
            titleText.text = titleLocalizedString.GetLocalizedString();
            descriptionText.text = descriptionLocalizedString.GetLocalizedString();
            
            // 애니메이션 오브젝트 교체
            tutorialAnims[currentIdx]?.SetActive(false);
            tutorialAnims[idx]?.SetActive(true);
            
            // 맵 교체 필요시 업데이트
            if (tutorialData.tutorialData[idx].isChangeMap)
            {
                stageHandler.stageData = tutorialData.tutorialData[idx].mapData;
                stageHandler.ResetStage(tutorialData.tutorialData[idx].isClearAble);
            }
            
            // 인덱스 업데이트
            currentIdx = idx;
        }
    }
}