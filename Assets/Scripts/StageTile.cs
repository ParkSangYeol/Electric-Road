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
        private static readonly Color[] ElectricTypeColors =
        {
            new Color(1f, 0.89f, 0.29f),
            Color.cyan,
            Color.magenta,
            Color.green
        };

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
        private int _electricType;
        public int electricType
        {
            get => _electricType;
            set
            {
                _electricType = value;
                RefreshElectricTypeIndicator();
            }
        }

        private FixedTileBackground fixedTileBackground;
        private ElectricTypeIndicator electricTypeIndicator;
        private Image electricTypeIndicatorImage;
        private bool showElectricTypeIndicator;
        
        private void Awake()
        {
            base.Awake();
            backgroundImage = gameObject.GetComponentInParent<Image>();
            fixedTileBackground = GetComponentInChildren<FixedTileBackground>(true);
            electricTypeIndicator = GetComponentInChildren<ElectricTypeIndicator>(true);
            if (electricTypeIndicator != null)
            {
                electricTypeIndicator.TryGetComponent(out electricTypeIndicatorImage);
            }
            
            onChangeDirection = new UnityEvent<Direction>();
            onChangeDirection.AddListener(ChangeTileDirection);
            onTileChange.AddListener(OnTileChanged);
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

        public void SetFixedTileBackground(bool isFixed)
        {
            if (fixedTileBackground != null)
            {
                fixedTileBackground.gameObject.SetActive(isFixed);
            }
        }

        public void SetElectricTypeIndicatorVisible(bool isVisible)
        {
            showElectricTypeIndicator = isVisible;
            RefreshElectricTypeIndicator();
        }

        private void OnTileChanged(TileScriptableObject _)
        {
            RefreshElectricTypeIndicator();
        }

        private void RefreshElectricTypeIndicator()
        {
            if (electricTypeIndicator == null)
            {
                return;
            }

            bool supportsElectricType = tile != null &&
                                        tile.tileType is ScriptableObjects.Stage.Tile.FACTORY
                                            or ScriptableObjects.Stage.Tile.GENERATOR;
            bool hasColor = TryGetElectricTypeColor(electricType, out Color color);
            bool shouldShow = showElectricTypeIndicator && supportsElectricType &&
                              electricTypeIndicatorImage != null && hasColor;

            if (shouldShow)
            {
                electricTypeIndicatorImage.color = color;
            }

            electricTypeIndicator.gameObject.SetActive(shouldShow);
        }

        private static bool TryGetElectricTypeColor(int targetElectricType, out Color color)
        {
            if ((uint)targetElectricType < (uint)ElectricTypeColors.Length)
            {
                color = ElectricTypeColors[targetElectricType];
                return true;
            }

            color = Color.white;
            return false;
        }

        [Button]
        public void SetActiveTile(bool isActive, int targetElectricType = 0)
        {
            backgroundImage.color = isActive &&
                                    TryGetElectricTypeColor(targetElectricType, out Color color)
                ? color
                : Color.white;
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
