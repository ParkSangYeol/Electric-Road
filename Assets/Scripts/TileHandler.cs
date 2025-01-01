using System;
using System.Collections;
using System.Collections.Generic;
using Command;
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
        private GraphicRaycaster raycaster;
        [SerializeField]
        private EventSystem eventSystem;
        [SerializeField] 
        private CommandHistoryHandler commandHistoryHandler;

        private bool _active;
        
        // Tile Palate 관련 변수들
        [SerializeField, SceneObjectsOnly] 
        private Transform tilePalateArea;
        
        [SerializeField, AssetsOnly] 
        private GameObject tilePalatePrefab;
        
        [SerializeField, AssetsOnly] 
        private List<TileScriptableObject> tilePlateDatas;
        private Dictionary<TileScriptableObject, PalateTile> palateTileDict;
        private List<PalateTile> palateTiles;
        
        public int numActivePalate;
        
        // 가이드 관련 변수들
        private Vector3 poolPosition;
        
        [SerializeField] 
        private GameObject palateSelectGameObject;
        
        [SerializeField] 
        private GameObject stageTileSelectGameObject; 
        
        [SerializeField] 
        private GameObject modulatorGameObject;
        
        [SerializeField] 
        private Button upButton;
        [SerializeField] 
        private Button downButton;
        [SerializeField] 
        private Button leftButton;
        [SerializeField] 
        private Button rightButton;

        [SerializeField] 
        private AudioClip buttonSFX;
        
        private void Awake()
        {
            palateTiles = new List<PalateTile>();
            palateTileDict = new Dictionary<TileScriptableObject, PalateTile>();
            _active = true;
        }

        void Start()
        {
            poolPosition = new Vector3(-1000, -1000, 0);
            palateSelectGameObject.transform.position = 
                stageTileSelectGameObject.transform.position = 
                    modulatorGameObject.transform.position = poolPosition;
            SetButton();
            CreateTilePalate();
            commandHistoryHandler.onExecuteCommand.AddListener(SetPalateEvent);
            commandHistoryHandler.onUndoCommand.AddListener(SetPalateEvent);
        }

        void Update()
        {
            if (!_active)
            {
                return;
            }
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
            if (tile.locked)
            {
                return;
            }
            
            SoundManager.Instance.PlaySFX(buttonSFX);
            if (palateTileData == tile.tile)
            {
                palateSelectGameObject.transform.position = poolPosition;
                palateTileData = null;
            }
            palateTileData = tile.tile;
            
            // Stage Tile에 배치된 가이드들의 위치를 리셋
            stageTileSelectGameObject.transform.position = poolPosition;
            modulatorGameObject.transform.position = poolPosition;
            currentSelectTile = null;
            
            // 선택 UI 표시
            palateSelectGameObject.transform.position = tile.transform.position;
        }

        private void SetStageTile(StageTile tile, Direction dir)
        {
            if (palateTileData == null || !tile.isEditAble)
            {
                return;
            }

            ICommand command = new TilePlaceCommand(tile, palateTileData, dir, tile.electricType);
            commandHistoryHandler.ExecuteCommand(command);
            
            currentSelectTile = null;

            stageTileSelectGameObject.transform.position = poolPosition;
        }

        public void SetModulator(int num)
        {
            if (palateTileData == null || !currentSelectTile.isEditAble)
            {
                return;
            }

            if (currentSelectTile.tile.tileType is not (ScriptableObjects.Stage.Tile.MODULATOR or ScriptableObjects.Stage.Tile.FACTORY))
            {
                return;
            }
            ICommand command = new TilePlaceCommand(currentSelectTile, currentSelectTile.tile, currentSelectTile.direction, num);
            commandHistoryHandler.ExecuteCommand(command);
            currentSelectTile = null;
            
            modulatorGameObject.transform.position = poolPosition;    
        }

        private void SelectStageTile(StageTile tile)
        {
            if (palateTileData == null || !tile.isEditAble)
            {
                return;
            }

            if (tile == currentSelectTile)
            {
                currentSelectTile = null;
                stageTileSelectGameObject.transform.position = poolPosition;
                modulatorGameObject.transform.position = poolPosition;
                return;
            }
            stageTileSelectGameObject.transform.position = poolPosition;
            modulatorGameObject.transform.position = poolPosition;

            currentSelectTile = tile;
            if (tile.tile.tileType == palateTileData.tileType 
                && tile.tile.tileType is ScriptableObjects.Stage.Tile.FACTORY or ScriptableObjects.Stage.Tile.MODULATOR)
            {
                // 만약 선택한 타일과 타일 팔레트의 타일이 모두 변환기인 경우
                modulatorGameObject.transform.position = tile.transform.position;
            }
            else
            {
                stageTileSelectGameObject.transform.position = tile.transform.position;
            }
        }

        #region Init

        public void CreateTilePalate()
        {
            for (int i = 0; i < palateTiles.Count; i++)
            {
                Destroy(palateTiles[i].gameObject);
            }
            palateTiles.Clear();
            palateTileDict.Clear();

            int palateNum = 1;
            foreach (var tileData in tilePlateDatas)
            {
                GameObject instantiateObject = Instantiate(tilePalatePrefab, tilePalateArea.transform);
                PalateTile tileComponent = instantiateObject.GetComponent<PalateTile>();
                tileComponent.tile = tileData;
                tileComponent.isEditAble = false;
                tileComponent.Active(_active);
                if (palateNum++ > numActivePalate)
                {
                    tileComponent.Lock(true);
                }
                
                palateTiles.Add(tileComponent);
                palateTileDict.Add(tileData, tileComponent);
            }
        }

        private void SetPalateEvent(ICommand command)
        {
            switch (command)
            {
                case TilePlaceCommand placeCommand:
                {
                    if (palateTileDict.TryGetValue(placeCommand.beforeTileData, out var before))
                    {
                        before.ChangeNumOfTile(-1);
                    }

                    if (palateTileDict.TryGetValue(placeCommand.targetTileData, out var target))
                    {
                        target.ChangeNumOfTile(+1);
                    }
                    break;
                }
                case TileRemoveCommand removeCommand:
                {
                    if (!palateTileDict.TryGetValue(removeCommand.beforeTileData, out var before)) return;
                    before.ChangeNumOfTile(-1);
                    break;
                }
            }
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
        }

        #endregion
        
        public void Active(bool active)
        {
            if (active == _active)
            {
                return;
            }
            _active = active;
            
            if (active)
            {
                // 활성화
                foreach (var palateTile in palateTiles)
                {
                    palateTile.Active(true);
                }
            }
            else
            {
                // 비활성화
                foreach (var palateTile in palateTiles)
                {
                    palateTile.Active(false);
                }

                palateSelectGameObject.transform.position =
                    stageTileSelectGameObject.transform.position =
                        modulatorGameObject.transform.position = poolPosition;

                currentSelectTile = null;
            }
        }
    }
}