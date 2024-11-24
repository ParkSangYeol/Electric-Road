using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using ScriptableObjects.Stage;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Stage {
    public class StageTile : Stage.Tile
    {
        private Direction _direction;
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
        
        private void Awake()
        {
            base.Awake();
            
            onChangeDirection = new UnityEvent<Direction>();
            onChangeDirection.AddListener(ChangeTileDirection);
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
        public void ActiveTile()
        {
            image.color = electricType switch 
            {
                0 => Color.yellow,
                1 => Color.cyan,
                2 => Color.magenta,
                3 => Color.green
            };
        }
    }
}