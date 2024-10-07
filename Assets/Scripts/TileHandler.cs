using System.Collections;
using System.Collections.Generic;
using ScriptableObjects.Stage;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Stage
{
    public class TileHandler : MonoBehaviour
    {
        // 마우스, 터치를 이용한 타일 배치, 삭제 기능을 지원
        [ShowInInspector, ReadOnly]
        private TileScriptableObject palateTileData;
        [ShowInInspector, ReadOnly] 
        private StageTile currentSelectTile;

        [SerializeField] 
        private GameObject palateSelectGameObject;
        [SerializeField] 
        private GameObject stageTileSelectGameObject;

        [SerializeField] 
        private Button upButton;
        [SerializeField] 
        private Button downButton;
        [SerializeField] 
        private Button leftButton;
        [SerializeField] 
        private Button rightButton;
        [SerializeField] 
        private Button deleteButton;

        [SerializeField] 
        private GraphicRaycaster raycaster;
        [SerializeField]
        private EventSystem eventSystem;
        private Vector3 poolPosition;
        
        void Start()
        {
            poolPosition = new Vector3(-1000, -1000, 0);
            palateSelectGameObject.transform.position = 
                stageTileSelectGameObject.transform.position = poolPosition;
            SetButton();
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("Mouse Down");
                Tile selectTile= GetTile(Input.mousePosition);
                switch (selectTile)
                {
                    case StageTile stageTile:
                        SelectStageTile(stageTile);
                        break;
                    case PalateTile palateTile:
                        SetPalateTile(palateTile);
                        break;
                }
            }
        }

        private Tile GetTile(Vector3 position)
        {
            PointerEventData pointerEventData = new PointerEventData(eventSystem);
            pointerEventData.position = position;

            List<RaycastResult> results = new List<RaycastResult>();
            LayerMask layerMask = LayerMask.NameToLayer("Tile");
            raycaster.Raycast(pointerEventData, results);

            Tile returnTile = null;
            
            foreach (RaycastResult result in results)
            {
                if (result.gameObject.layer == layerMask)
                {
                    returnTile = result.gameObject.GetComponent<Tile>();
                    break;
                }
            }
            
            return returnTile;
        }

        private void SetPalateTile(PalateTile tile)
        {
            palateTileData = tile.tile;
            
            // 선택 UI 표시
            palateSelectGameObject.transform.position = tile.transform.position;
        }

        private void SetStageTile(StageTile tile, Direction dir)
        {
            if (palateTileData == null)
            {
                return;
            }
            tile.tile = palateTileData;
            tile.direction = dir;

            stageTileSelectGameObject.transform.position = poolPosition;
        }

        private void SetDefaultStageTile(StageTile tile)
        {
            tile.tile = tile.defaultTile;
            stageTileSelectGameObject.transform.position = poolPosition;
        }

        private void SelectStageTile(StageTile tile)
        {
            if (palateTileData == null)
            {
                return;
            }
            
            currentSelectTile = tile;

            stageTileSelectGameObject.transform.position = tile.transform.position;
        }

        private void SetButton()
        {
            upButton.onClick.AddListener(() =>
            {
                SetStageTile(currentSelectTile, Direction.UP);
            });
            downButton.onClick.AddListener(() =>
            {
                SetStageTile(currentSelectTile, Direction.DOWN);
            });
            leftButton.onClick.AddListener(() =>
            {
                SetStageTile(currentSelectTile, Direction.LEFT);
            });
            rightButton.onClick.AddListener(() =>
            {
                SetStageTile(currentSelectTile, Direction.RIGHT);
            });
            deleteButton.onClick.AddListener(() =>
            {
                SetDefaultStageTile(currentSelectTile);
            });
        }
    }
}