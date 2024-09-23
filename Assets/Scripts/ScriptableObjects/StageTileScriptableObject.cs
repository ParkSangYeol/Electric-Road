using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Demos.RPGEditor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using Stage;
using UnityEditor;
using UnityEngine;

namespace ScriptableObjects.Stage
{
    [CreateAssetMenu(menuName = "Assets/ScriptableObject/Stage")]
    public class StageTileScriptableObject : SerializedScriptableObject
    {
        [VerticalGroup("Split")]
        [PropertySpace(0, 10)]
        [LabelText("스테이지 이름")]
        public string stageName;

        [VerticalGroup("Split/Meta")]
        [ReadOnly, LabelText("가로")]
        public int width;
        [VerticalGroup("Split/Meta")]
        [ReadOnly, LabelText("세로")]
        public int height;
        
        [TabGroup("Split/Map","스테이지 구성", SdfIconType.Map)] 
        [ReadOnly, TableMatrix(SquareCells = true)]
        public TileStruct[,] map;
        
        [TabGroup("Split/Map", "정답 경로", SdfIconType.Exclamation)]
        [ReadOnly, TableMatrix(SquareCells = true)]
        public TileStruct[,] answerMap;

        [BoxGroup("Split/Answer")]
        [ReadOnly, LabelText("최단 비용")]
        public int ans;

        public void MakeMapByStageTiles(StageTile[,] stageMatrix)
        {
            width = stageMatrix.GetLength(0);
            height = stageMatrix.GetLength(1);

            map = new TileStruct[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    map[x, y] = new TileStruct(stageMatrix[x, y]);
                }
            }
        }
    }
    
    [Serializable]
    public class TileStruct
    {
        public Direction dir;
        public TileScriptableObject tile;

        public TileStruct(Direction dir, TileScriptableObject tile)
        {
            this.dir = dir;
            this.tile = tile;
        }

        public TileStruct()
        {
            this.dir = Direction.UP;
            this.tile = null;
        }

        public TileStruct(StageTile stageTile)
        {
            this.dir = stageTile.direction;
            this.tile = stageTile.tile;
        }
    }
    
    public enum Direction
    {
        UP,
        DOWN,
        RIGHT,
        LEFT,
        NONE
    }
}