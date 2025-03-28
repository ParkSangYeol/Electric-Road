using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace UI
{
    public class LocalizeSettingComponent : MonoBehaviour
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
        private List<string> components;
        
        private int idx;

        public UnityEvent<int> onValueChange;

        private void Start()
        {
            onValueChange.AddListener((idx) =>
            {
                valueText.text = components[idx];
            });
            onValueChange.AddListener((idx) =>
            {
                LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[idx];
            });
            idx = LocalizationSettings.AvailableLocales.Locales.IndexOf(LocalizationSettings.SelectedLocale);
            valueText.text = components[idx];
        }

        public void OnClickLeftButton()
        {
            SoundManager.Instance.PlaySFX(sfx);
            idx -= 1;
            if (idx < 0)
            {
                idx = components.Count - 1;
            }
            onValueChange.Invoke(idx);
        }
        
        public void OnClickRightButton()
        {
            SoundManager.Instance.PlaySFX(sfx);
            idx += 1;
            if (idx >= components.Count)
            {
                idx = 0;
            }
            onValueChange.Invoke(idx);
        }
    }
}