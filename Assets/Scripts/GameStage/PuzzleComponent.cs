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
    }
}

