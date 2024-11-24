using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI
{
    public class SettingComponent : MonoBehaviour
    {
        [SerializeField] 
        private Button leftButton;
        [SerializeField] 
        private Button rightButton;

        [SerializeField] 
        private TMP_Text valueText;
        
        [SerializeField] 
        private AudioClip sfx;

        [SerializeField] 
        private Vector2 min_max;

        [SerializeField] 
        private int step;

        [SerializeField] 
        private string playerPrefKey;
        
        private int value;

        public UnityEvent<int> onValueChange;

        private void Start()
        {
            onValueChange.AddListener((num) =>
            {
                valueText.text = num.ToString();
                PlayerPrefs.SetInt(playerPrefKey, num);
                PlayerPrefs.Save();
            });
            value = PlayerPrefs.GetInt(playerPrefKey, 5);
            valueText.text = value.ToString();
        }

        public void OnClickLeftButton()
        {
            SoundManager.Instance.PlaySFX(sfx);
            value -= step;
            if (value < min_max.x)
            {
                value = (int)min_max.y;
            }
            onValueChange.Invoke(value);
        }
        
        public void OnClickRightButton()
        {
            SoundManager.Instance.PlaySFX(sfx);
            value += step;
            if (value > min_max.y)
            {
                value = (int)min_max.x;
            }
            onValueChange.Invoke(value);
        }
    }
}
