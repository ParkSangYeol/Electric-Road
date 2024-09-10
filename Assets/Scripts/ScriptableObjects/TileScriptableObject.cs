
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
        [HideLabel, PreviewField(55)]
        public Texture Icon;
        public Tile tileType;
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