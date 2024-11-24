using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Stage
{
    public class PalateTile : Tile
    {
        private Image background;
        [SerializeField]
        private TMP_Text costText;
        
        private void Awake()
        {
            base.Awake();
            onTileChange.AddListener((tile) =>
            {
                costText.text = tile.cost.ToString();
            });
            background = GetComponent<Image>();
        }
        
        public void Active(bool active)
        {
            if (active)
            {
                // 활성화
                background.color = image.color = Color.white;
            }
            else
            {
                // 비활성화
                background.color = image.color = Color.grey;
            }
        }
    }
}