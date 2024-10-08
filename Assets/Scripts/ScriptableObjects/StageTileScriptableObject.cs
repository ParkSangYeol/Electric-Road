using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using Stage;
using UnityEditor;
using UnityEngine;

namespace ScriptableObjects.Stage
{
    [CreateAssetMenu(menuName = "Assets/ScriptableObject/Stage")]
    public class StageScriptableObject : SerializedScriptableObject
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

        public void MakeMapByStageTiles(TileStruct[,] map, TileStruct[,] ansMap)
        {
            width = map.GetLength(0);
            height = map.GetLength(1);

            this.map = map;
            this.answerMap = ansMap;

            ans = 0;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    ans += (int)ansMap[x, y].tile.cost == 9999? 0 : (int)ansMap[x, y].tile.cost;
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