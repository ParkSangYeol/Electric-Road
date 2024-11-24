using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Stage.UI
{
    public class SliderThresholdController : MonoBehaviour
    {
        [SerializeField] 
        private TMP_Text text;

        [SerializeField] 
        private Image image;

        private bool isPass;
        private Color defaultColor;
        public Color passColor;
        
        private void Awake()
        {
            isPass = false;
            defaultColor = image.color;
        }

        public void SetText(string newText)
        {
            text.text = newText;
        }
        
        public void Pass(bool pass)
        {
            if (isPass == pass)
            {
                return;
            }

            isPass = pass;
            if (pass)
            {
                // 역치 통과
                text.DOScale(0, 0.5f).SetEase(Ease.InBack);
                image.DOColor(passColor, 0.4f).SetEase(Ease.OutQuad);
            }
            else
            {
                // 리셋
                text.DOScale(1, 0.5f).SetEase(Ease.OutBack);
                image.DOColor(defaultColor, 0.4f).SetEase(Ease.InQuad);
            }
        }
    }
}
