using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Stage.UI
{
    public class CostSlider : MonoBehaviour
    {
        [SerializeField] 
        private List<SliderThresholdController> thresholds;

        [SerializeField] 
        private Image sliderImage;

        [SerializeField] 
        private TMP_Text costText;

        private List<int> thresholdAmounts;
        private Coroutine fillRoutine;
        private void Awake()
        {
            sliderImage.fillAmount = 0f;
        }

        private void Start()
        {
            foreach (var threshold in thresholds)
            {
                threshold.passColor = sliderImage.color;
            }
        }

        public void SetThresholdAmount(List<int> amounts)
        {
            thresholdAmounts = amounts;
            for (int i = 0; i < amounts.Count; i++)
            {
                thresholds[i].SetText(amounts[i].ToString());
            }
        }
        
        public void UpdateCostText(float value)
        {
            DOTween.Kill("costTween");
            if (fillRoutine != null)
            {
                StopCoroutine(fillRoutine);
            }
            costText.DOText(value.ToString(), 0.4f, true, ScrambleMode.Numerals).SetId("costTween");

            float fillAmount = GetRatio(value, 0, thresholdAmounts[0])
                               + GetRatio(value, thresholdAmounts[0], thresholdAmounts[1])
                               + GetRatio(value, thresholdAmounts[1], thresholdAmounts[2])
                               + GetRatio(value, thresholdAmounts[2], thresholdAmounts[2] + 100);
            fillRoutine = StartCoroutine(FillSlider(fillAmount, 0.4f));

            switch (fillAmount)
            {
                case <= 0.25f:
                    thresholds[0].Pass(false);
                    thresholds[1].Pass(false);
                    thresholds[2].Pass(false);
                    return;
                case <= 0.5f:
                    thresholds[0].Pass(true);
                    thresholds[1].Pass(false);
                    thresholds[2].Pass(false);
                    return;
                case <= 0.75f:
                    thresholds[0].Pass(true);
                    thresholds[1].Pass(true);
                    thresholds[2].Pass(false);
                    break;
                default:
                    thresholds[0].Pass(true);
                    thresholds[1].Pass(true);
                    thresholds[2].Pass(true);
                    break;
            }
        }

        private IEnumerator FillSlider(float fillAmount, float duration)
        {
            float baseValue = sliderImage.fillAmount;
            float diff = fillAmount - sliderImage.fillAmount;
            float time = 0f;

            while (time < duration)
            {
                time += Time.deltaTime;
                sliderImage.fillAmount = baseValue + diff * (time) / duration;
                yield return null;
            }
            sliderImage.fillAmount = fillAmount;
        }

        private float GetRatio(float value, float minimum, float maximum)
        {
            return (Mathf.Clamp(value, minimum, maximum) - minimum) * 0.25f / (maximum - minimum);
        }
    }
}
