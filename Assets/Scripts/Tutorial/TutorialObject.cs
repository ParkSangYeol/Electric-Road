using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace Tutorial
{
    public class TutorialObject : MonoBehaviour
    {
        [SerializeField]
        private DOTweenAnimation doTweenAnimation;
        
        [SerializeField] 
        private int loopCount;
        [SerializeField]
        private float delaySec;
        
        private IEnumerator Restart()
        {
            int count = loopCount;
            while (count-- > 0)
            {
                doTweenAnimation.DORestart();
                yield return new WaitForSeconds(delaySec + doTweenAnimation.duration);
            }
        }

        private void OnEnable()
        {
            StartCoroutine(Restart());
        }

        private void OnDisable()
        {
            gameObject.SetActive(false);
        }
    }
}

