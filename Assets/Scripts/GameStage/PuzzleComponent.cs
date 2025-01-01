using System;
using System.Collections;
using System.Collections.Generic;
using ScriptableObjects.Stage;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace GameStage
{
    public class PuzzleComponent : MonoBehaviour
    {
        [SerializeField] 
        private List<StarController> stars;

        [ShowInInspector, ReadOnly]
        public StageScriptableObject puzzleData;

        [ShowInInspector, ReadOnly]
        public int idx;
        
        [SerializeField] 
        private AudioClip sfx;

        [SerializeField] 
        private Image lockImage;

        private Button buttonComponent;

        private void Awake()
        {
            buttonComponent = GetComponent<Button>();
        }

        private void Start()
        {
            GetComponent<Button>().onClick.AddListener(() =>
            {
                SoundManager.Instance.PlaySFX(sfx);
            });
        }

        public void SetStar(int numOfStar)
        {
            foreach (var star in stars)
            {
                star.SetStar(numOfStar-- > 0);
            }
        }

        public void SetLock(bool locked)
        {
            lockImage.gameObject.SetActive(locked);
            buttonComponent.enabled = !locked;
        }
    }
}

