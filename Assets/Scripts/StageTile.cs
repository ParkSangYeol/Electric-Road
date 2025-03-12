using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using ScriptableObjects.Stage;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Stage {
    public class StageTile : Stage.Tile
    {
        private Direction _direction;
        private Image backgroundImage;
        [ShowInInspector] 
        private Color highlightColor;
        
        [ShowInInspector, ReadOnly]
        public Direction direction
        {
            get => _direction;
            set
            {
                if (direction != value)
                {
                    _direction = value;
                    onChangeDirection.Invoke(_direction);
                }       
            }
        }
        public UnityEvent<Direction> onChangeDirection;
        public TileScriptableObject defaultTile;
        public int electricType;
        private Color[] colorByElectricType;
        
        private void Awake()
        {
            base.Awake();
            backgroundImage = gameObject.GetComponentInParent<Image>();
            
            onChangeDirection = new UnityEvent<Direction>();
            onChangeDirection.AddListener(ChangeTileDirection);

            colorByElectricType = new[]
            {
                new Color(1f, 0.89f, 0.29f),
                Color.cyan,
                Color.magenta,
                Color.green
            };
        }

        private void Start()
        {
            highlightColor = new Color(0.72f, 0.72f, 0.72f);
        }

        private void ChangeTileDirection(Direction dir)
        {
            transform.rotation = dir switch
            {
                Direction.UP => Quaternion.Euler(0, 0, 0),
                Direction.DOWN => Quaternion.Euler(0, 0, 180),
                Direction.LEFT => Quaternion.Euler(0, 0, 90),
                Direction.RIGHT => Quaternion.Euler(0, 0, -90),
                _ => Quaternion.Euler(0, 0, 0)
            };
        }

        public void EraseTile()
        {
            tile = defaultTile;
            image.color = Color.white;
        }

        [Button]
        public void SetActiveTile(bool isActive)
        {
            backgroundImage.color = isActive
                ? colorByElectricType[electricType] : Color.white;
        }

        public void SetHighlight(bool highlight)
        {
            if (highlight)
            {
                // 하이라이트
                image.color = backgroundImage.color = highlightColor;
            }
            else
            {
                // 하이라이트 취소
                image.color = backgroundImage.color = Color.white;
            }
        }
    }
}