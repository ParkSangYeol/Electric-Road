using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Command;
using ScriptableObjects.Stage;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace Stage
{
    public class WireTileHandler : MonoBehaviour
    {
        public PlaceState state;
        public bool canTracking;
        
        [SerializeField] 
        private EventSystem eventSystem;
        [SerializeField]
        private GraphicRaycaster raycaster;
        [SerializeField] 
        private CommandHistoryHandler commandHistoryHandler;
        
        private OrderedDictionary trackTiles;
        private Dictionary<ScriptableObjects.Stage.Tile, TileScriptableObject> tileDict = new Dictionary<ScriptableObjects.Stage.Tile, TileScriptableObject>();

        public UnityEvent onStartTrack;
        public UnityEvent onEndTrack;
        
        private void Awake()
        {
            state = PlaceState.IDLE;
            canTracking = false;
        }

        private void Start()
        {
            InitTileDict();
            trackTiles = new OrderedDictionary();
        }

        private void Update()
        {
            // TODO Change GetMouseButton to GetTouch.
            if (!canTracking)
            {
                return;
            }
            
            if (state.Equals(PlaceState.IDLE) && Input.GetMouseButtonDown(0))
            {
                var stageTile = GetStageTile(Input.mousePosition);
                if (stageTile == null)
                {
                    return;
                }
                // 추적 시작
                Debug.Log("Start Track.");
                state = PlaceState.ON_TRACK;
                trackTiles = new OrderedDictionary { { stageTile.transform.position, stageTile } };
            }
            else if (state.Equals(PlaceState.ON_TRACK) && Input.GetMouseButtonUp(0))
            {
                // 추적 종료
                Debug.Log("End Track.");
                PlaceTiles();
            }
            else if (state.Equals(PlaceState.ON_TRACK))
            {
                // 추적 중
                var stageTile = GetStageTile(Input.mousePosition);
                if (stageTile == null)
                {
                    Debug.Log("Out of Range! Fail.");
                    state = PlaceState.IDLE;
                    return;
                }
                
                Debug.Log("Tracking Tile.");
                var lastElement = trackTiles[trackTiles.Count - 1] as StageTile; // ^1 사용시 null 나옴
                if (trackTiles.Count > 0 && stageTile.Equals(lastElement))
                {
                    // 같은 타일을 가리킨 경우 넘어감.
                    Debug.Log("Same Tile, continue.");
                    return;
                }
                
                if (stageTile.tile.tileType == ScriptableObjects.Stage.Tile.FACTORY)
                {
                    Debug.Log("Find Factory, End Track.");
                    // 공장 타일에 도착. 조기 종료
                    PlaceTiles();
                    return;
                }
                
                if (stageTile.tile.tileType != ScriptableObjects.Stage.Tile.NONE 
                    || trackTiles.Contains(stageTile.transform.position))
                {
                    // 다른 타일에 부딪힘. 실패 처리 후 조기 종료.
                    Debug.Log("Tile Dup, Fail.");
                    state = PlaceState.IDLE;
                    return;
                }
                
                // 빈 공간에 도착. 처리 진행.
                Debug.Log("Add Tile.");
                trackTiles.Add(stageTile.transform.position, stageTile);
            }
        }
        
        private void InitTileDict()
        {
            string[] guids = AssetDatabase.FindAssets($"t:{nameof(TileScriptableObject)}");
            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                TileScriptableObject tileData = AssetDatabase.LoadAssetAtPath<TileScriptableObject>(assetPath);
                if (tileData != null)
                {
                    tileDict.Add(tileData.tileType, tileData);
                }
            }
        }
        
        private void PlaceTiles()
        {
            state = PlaceState.ON_PLACE;
            if (trackTiles == null || trackTiles.Count == 0)
            {
                return;
            }

            int idx = 1;
            var firstElement = trackTiles[0] as StageTile;
            if (firstElement == null)
            {
                return;
            }
            
            if (firstElement.tile.tileType is ScriptableObjects.Stage.Tile.LINE
                or ScriptableObjects.Stage.Tile.CORNER_LEFT or ScriptableObjects.Stage.Tile.CORNER_RIGHT)
            {
                // 분배기로 변경
                ICommand command = new TilePlaceCommand(firstElement, tileDict[ScriptableObjects.Stage.Tile.DISTRIBUTOR], Direction.NONE, firstElement.electricType);
                command.Execute();
                commandHistoryHandler.AddCommand(command);
            }
            else if (firstElement.tile.tileType.Equals(ScriptableObjects.Stage.Tile.NONE))
            {
                idx = 0;
            }

            for (; idx < trackTiles.Count; idx++)
            {
                // 타일 배치
                // 다음 타일의 위치를 보고, 방향 결정. 만약 마지막 인덱스면 이전 방향 유지.
                StageTile prevTile = idx == 0? null : trackTiles[idx - 1] as StageTile;
                StageTile currentTile = trackTiles[idx] as StageTile;
                StageTile nextTile = idx == trackTiles.Count - 1 ? null : trackTiles[idx + 1] as StageTile;
                if (currentTile == null)
                {
                    Debug.LogError("잘못된 값이 저장되었습니다. currentIdx: " + idx + ", 저장된 값: " + trackTiles[idx]);
                }
                // 방향 설정
                Direction dir;
                if (idx == trackTiles.Count - 1)
                {
                    // 마지막 인덱스
                    if (trackTiles.Count == 1)
                    {
                        dir = Direction.UP;
                    }
                    else
                    {
                        dir = GetDirection(prevTile.transform.position, currentTile.transform.position);
                    }
                }
                else
                {
                    if (nextTile == null)
                    {
                        Debug.LogError("잘못된 값이 저장되었습니다. currentIdx: " + (idx + 1) + ", 저장된 값: " + trackTiles[idx + 1]);
                    }
                    dir = GetDirection(currentTile.transform.position, nextTile.transform.position);
                }
                
                // 타일 배치
                TileScriptableObject tileData;
                if (idx == 0)
                {
                    tileData = tileDict[ScriptableObjects.Stage.Tile.LINE];
                }
                else
                {
                    Direction lastDir = GetDirection(prevTile.transform.position, currentTile.transform.position);
                    tileData = tileDict[GetNextTile(lastDir, dir)];
                }
                
                ICommand command = new TilePlaceCommand(currentTile, tileData, dir, currentTile.electricType);
                command.Execute();
                commandHistoryHandler.AddCommand(command);
            }

            state = PlaceState.IDLE;
        }

        private Direction GetDirection(Vector2 currPos, Vector2 nextPos)
        {
            if (currPos.x != nextPos.x && currPos.y != nextPos.y)
            {
                Debug.LogError("대각선에 위치한 타일이 연속적으로 저장되었습니다.");
                return Direction.NONE;
            }

            if (currPos.x < nextPos.x)
            {
                return Direction.RIGHT;
            }
            if (currPos.x > nextPos.x)
            {
                return Direction.LEFT;
            }

            if (currPos.y < nextPos.y)
            {
                return Direction.UP;
            }
            if (currPos.y > nextPos.y)
            {
                return Direction.DOWN;
            }
            
            Debug.LogError("같은 타일이 두 번 저장되었습니다.");
            return Direction.NONE;
        }
        
        private ScriptableObjects.Stage.Tile GetNextTile(Direction lastDir, Direction currentDir)
        {
            ScriptableObjects.Stage.Tile ret = ScriptableObjects.Stage.Tile.NONE;
            switch (lastDir)
            {
                case Direction.UP:
                    if (currentDir is Direction.UP or Direction.DOWN)
                    {
                        ret = ScriptableObjects.Stage.Tile.LINE;
                    }
                    else if (currentDir is Direction.LEFT)
                    {
                        ret = ScriptableObjects.Stage.Tile.CORNER_LEFT;
                    }
                    else if (currentDir is Direction.RIGHT)
                    {
                        ret = ScriptableObjects.Stage.Tile.CORNER_RIGHT;
                    }
                    break;
                case Direction.DOWN:
                    if (currentDir is Direction.UP or Direction.DOWN)
                    {
                        ret = ScriptableObjects.Stage.Tile.LINE;
                    }
                    else if (currentDir is Direction.LEFT)
                    {
                        ret = ScriptableObjects.Stage.Tile.CORNER_RIGHT;
                    }
                    else if (currentDir is Direction.RIGHT)
                    {
                        ret = ScriptableObjects.Stage.Tile.CORNER_LEFT;
                    }
                    break;
                case Direction.LEFT:
                    if (currentDir is Direction.LEFT or Direction.RIGHT)
                    {
                        ret = ScriptableObjects.Stage.Tile.LINE;
                    }
                    else if (currentDir is Direction.DOWN)
                    {
                        ret = ScriptableObjects.Stage.Tile.CORNER_LEFT;
                    }
                    else if (currentDir is Direction.UP)
                    {
                        ret = ScriptableObjects.Stage.Tile.CORNER_RIGHT;
                    }
                    break;
                case Direction.RIGHT:
                    if (currentDir is Direction.LEFT or Direction.RIGHT)
                    {
                        ret = ScriptableObjects.Stage.Tile.LINE;
                    }
                    else if (currentDir is Direction.DOWN)
                    {
                        ret = ScriptableObjects.Stage.Tile.CORNER_RIGHT;
                    }
                    else if (currentDir is Direction.UP)
                    {
                        ret = ScriptableObjects.Stage.Tile.CORNER_LEFT;
                    }
                    break;
                case Direction.NONE:
                default:
                    ret = ScriptableObjects.Stage.Tile.LINE;
                    break;
            }
            return ret;
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
    }

    public enum PlaceState
    {
        IDLE,
        ON_TRACK,
        ON_PLACE
    }
}