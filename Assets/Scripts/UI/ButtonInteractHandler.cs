using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class ButtonInteractHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, 
        IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField]
        private DOTweenAnimation doTweenAnimation;

        [SerializeField] 
        private AudioClip sfx;
        
        [ShowInInspector]
        private bool isEnter, isDown;
        private void Awake()
        {
            isEnter = isDown = false;
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            #if UNITY_STANDALONE_WIN || UNITY_STANDALONE
            isEnter = true;
            if (!isDown)
            {
                doTweenAnimation.DORestartById("Enter");
            }
            #endif
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            #if UNITY_STANDALONE_WIN || UNITY_STANDALONE
            isEnter = false;
            if (!isDown)
            {
                doTweenAnimation.DOPlayBackwardsById("Enter");
            }
            #endif
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            #if UNITY_STANDALONE_WIN || UNITY_STANDALONE
            isDown = true;
            if (isEnter)
            {
                doTweenAnimation.DORestartById("Down");
                SoundManager.Instance.PlaySFX(sfx);
            }
            #elif UNITY_ANDROID || UNITY_IOS
            doTweenAnimation.DORestartById("Down");
            SoundManager.Instance.PlaySFX(sfx);
            #endif
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            #if UNITY_STANDALONE_WIN || UNITY_STANDALONE
            isDown = false;
            if (isEnter)
            {
                doTweenAnimation.DOPlayBackwardsById("Down");
            }
            else
            {
                doTweenAnimation.DOPlayBackwardsById("Enter");
                doTweenAnimation.DORestartById("Up");
            }
            #elif UNITY_ANDROID || UNITY_IOS
            doTweenAnimation.DOPlayBackwardsById("Down");
            #endif
        }
    }
}