
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor.Drawers;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
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
                Tile.CORNER => "코너 타일",
                Tile.DISTRIBUTOR => "분배기 타일",
                Tile.AMPLIFIER => "증폭기 타일",
                Tile.MODULATOR => "변환기 타일",
                Tile.OBSTACLE => "장애물 타일",
                Tile.FACTORY => "공장 타일",
                Tile.GENERATOR => "발전기 타일",
            };
        }
    }
    
    
    internal sealed class TileStructCellDrawer<TArray> : TwoDimensionalArrayDrawer<TArray, TileStruct>
        where TArray : System.Collections.IList
    {
        protected override TableMatrixAttribute GetDefaultTableMatrixAttributeSettings()
        {
            return new TableMatrixAttribute()
            {
                SquareCells = true,
                HideColumnIndices = true,
                HideRowIndices = true,
                ResizableColumns = false
            };
        }

        protected override TileStruct DrawElement(Rect rect, TileStruct value)
        {
            var id = DragAndDropUtilities.GetDragAndDropId(rect);
            
            value = DragAndDropUtilities.DropZone<TileStruct>(rect, value);
            value = DragAndDropUtilities.DragZone<TileStruct>(rect, value, false, false);
            
            DragAndDropUtilities.DrawDropZone(rect, value?.tile.Icon, null, id); 
            
            return value;
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            base.DrawPropertyLayout(label);

            // Draws a drop-zone where we can destroy items.
            var rect = GUILayoutUtility.GetRect(0, 40).Padding(2);
            var id = DragAndDropUtilities.GetDragAndDropId(rect);
            DragAndDropUtilities.DrawDropZone(rect, null as UnityEngine.Object, null, id);
            DragAndDropUtilities.DropZone<TileStruct>(rect, null, false, id);
        }
    }

}