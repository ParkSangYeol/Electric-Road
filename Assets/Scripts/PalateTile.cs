using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
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
        [SerializeField]
        private TMP_Text numText;

        private int numOfTile;
        
        private void Awake()
        {
            base.Awake();
            
            numOfTile = 0;
            ChangeNumOfTile(0);
            
            var numStringBuilder = new StringBuilder();
            numStringBuilder.Append("x ");
            numStringBuilder.Append(numOfTile.ToString());
            numText.text = numStringBuilder.ToString();
            
            onTileChange.AddListener((tile) =>
            {
                costText.text = tile.cost.ToString();
            });
            background = GetComponent<Image>();
        }

        public void ChangeNumOfTile(int num)
        {
            numOfTile += num;
            
            var numStringBuilder = new StringBuilder();
            numStringBuilder.Append("x ");
            numStringBuilder.Append(numOfTile.ToString());
            numText.text = numStringBuilder.ToString();
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