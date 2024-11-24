using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Stage
{
    public class EditModeHandler : MonoBehaviour
    {
        [SerializeField] 
        private Transform modeGuide;

        [SerializeField] 
        private WireTileHandler wireTileHandler;
        [SerializeField] 
        private TileHandler tileHandler;
        [SerializeField] 
        private EraseHandler eraseHandler;

        [SerializeField] 
        private Button drawButton;
        [SerializeField]
        private Button selectButton;
        [SerializeField]
        private Button eraseButton;

        [SerializeField] 
        private AudioClip sfx;
        
        private Vector3 poolPosition;
        private EditMode currentMode;

        private void Awake()
        {
            poolPosition = new Vector3(1000, 1000, 1000);
        }

        private void Start()
        {
            ChangeMode(EditMode.DRAW);
            drawButton.onClick.AddListener(() =>
            {
                SoundManager.Instance.PlaySFX(sfx);   
            });
            selectButton.onClick.AddListener(() =>
            {
                SoundManager.Instance.PlaySFX(sfx);   
            });
            eraseButton.onClick.AddListener(() =>
            {
                SoundManager.Instance.PlaySFX(sfx);   
            });
        }
        
        public void ChangeMode(int num)
        {
            EditMode targetMode = num switch
            {
                0 => EditMode.DRAW,
                1 => EditMode.SELECT,
                2 => EditMode.ERASE,
                _=> EditMode.DRAW
            };
            ChangeMode(targetMode);
        }
        
        [Button]
        public void ChangeMode(EditMode mode)
        {
            currentMode = mode;
            
            wireTileHandler.enabled = false;
            tileHandler.Active(false);
            eraseHandler.enabled = false;
            
            switch (currentMode)
            {
                case EditMode.DRAW:
                    drawButton.enabled = selectButton.enabled = eraseButton.enabled = true;
                    modeGuide.transform.position = drawButton.transform.position;
                    wireTileHandler.enabled = true;
                    break;
                case EditMode.SELECT:
                    modeGuide.transform.position = selectButton.transform.position;
                    tileHandler.Active(true);
                    break;
                case EditMode.ERASE:
                    modeGuide.transform.position = eraseButton.transform.position;
                    eraseHandler.enabled = true;
                    break;
                case EditMode.STOP:
                    drawButton.enabled = selectButton.enabled = eraseButton.enabled = false;
                    modeGuide.transform.position = poolPosition;
                    break;
            }
        }
    }

    [Serializable]
    public enum EditMode
    {
        DRAW = 0,
        SELECT = 1,
        ERASE = 2,
        STOP
    }
}
