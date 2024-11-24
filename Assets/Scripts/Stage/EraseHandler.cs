using System;
using System.Collections;
using System.Collections.Generic;
using Command;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Stage
{
    public class EraseHandler : MonoBehaviour
    {
        [SerializeField] 
        private CommandHistoryHandler commandHistoryHandler;
        [SerializeField] 
        private EventSystem eventSystem;
        [SerializeField]
        private GraphicRaycaster raycaster;

        private void Update()
        {
            EraseCheck();
        }

        private void EraseCheck()
        {
            if (!Input.GetMouseButton(0)) return;
            
            var stageTile = GetStageTile(Input.mousePosition);
            if (stageTile != null && stageTile.tile != stageTile.defaultTile)
            {
                SetDefaultStageTile(stageTile);
            }
        }
        
        private StageTile GetStageTile(Vector3 position)
        {
            PointerEventData pointerEventData = new PointerEventData(eventSystem);
            pointerEventData.position = position;

            List<RaycastResult> results = new List<RaycastResult>();
            LayerMask layerMask = LayerMask.NameToLayer("Tile");
            raycaster.Raycast(pointerEventData, results);

            StageTile returnTile = null;
            
            foreach (RaycastResult result in results)
            {
                if (result.gameObject.layer == layerMask)
                {
                    returnTile = result.gameObject.GetComponent<StageTile>();
                    if (returnTile != null)
                    {
                        break;
                    }
                }
            }
            
            return returnTile;
        }
        
        private void SetDefaultStageTile(StageTile tile)
        {
            ICommand command = new TileRemoveCommand(tile);
            commandHistoryHandler.ExecuteCommand(command);
        }
    }
}