
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace ScriptableObjects.Stage
{
    public class TileScriptableObject : UnityEngine.ScriptableObject
    {
        [HideLabel, ReadOnly]
        public string tileTypeName;
        [HideLabel, PreviewField(55)]
        public Sprite Icon;
        [OnValueChanged("ChangeTileName")]
        public Tile tileType;
        public float cost;

        [OnInspectorInit]
        private void ChangeTileName()
        {
            tileTypeName = tileType switch
            {
                Tile.NONE => "공백 타일",
                Tile.LINE => "직선 타일",
                Tile.CORNER_LEFT => "왼쪽 90도 코너 타일",
                Tile.CORNER_RIGHT => "오른쪽 90도 코너 타일",
                Tile.DISTRIBUTOR => "분배기 타일",
                Tile.AMPLIFIER => "증폭기 타일",
                Tile.MODULATOR => "변환기 타일",
                Tile.OBSTACLE => "장애물 타일",
                Tile.FACTORY => "공장 타일",
                Tile.GENERATOR => "발전기 타일",
            };
        }
    }
}