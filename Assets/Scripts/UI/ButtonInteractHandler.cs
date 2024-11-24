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
        [ShowInInspector]
        private bool isEnter, isDown;
        private void Awake()
        {
            isEnter = isDown = false;
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            isEnter = true;
            if (!isDown)
            {
                doTweenAnimation.DORestartById("Enter");
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isEnter = false;
            if (!isDown)
            {
                doTweenAnimation.DOPlayBackwardsById("Enter");
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            isDown = true;
            if (isEnter)
            {
                doTweenAnimation.DORestartById("Down");
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
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
        }
    }
}