using System;
using System.Collections;
using System.Collections.Generic;
using ScriptableObjects.Stage;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Stage
{
    public abstract class Tile : MonoBehaviour
    {
        protected Image image;
        
        [ShowInInspector, ReadOnly]
        public TileScriptableObject tile
        {
            get => _tile;
            set
            {
                if (_tile != value)
                {
                    _tile = value;
                    onTileChange.Invoke(_tile);
                }
            }
        }
        private TileScriptableObject _tile;

        public UnityEvent<TileScriptableObject> onTileChange;
        public UnityEvent onTileClicked;

        public bool isEditAble;

        protected void Awake()
        {
            onTileChange = new UnityEvent<TileScriptableObject>();
            onTileChange.AddListener(ChangeTileImage);
            isEditAble = false;
            
            if (image == null)
            {
                image = transform.GetChild(0).GetComponent<Image>();
            }
        }

        private void ChangeTileImage(TileScriptableObject newTile)
        {
            image.sprite = newTile.Icon;
        }
    }
}