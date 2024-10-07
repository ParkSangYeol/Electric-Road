using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ScriptableObjects.Stage;
using StageBuilder;
using UnityEngine;

namespace Stage
{
    public class StageHandler : MonoBehaviour
    {
        [SerializeField]
        private StageEditor stageEditor;

        [SerializeField] 
        private StageScriptableObject stageData;
        
        [SerializeField] 
        private StageArea stageArea;
        
        public int maxElectric;
        
        private void Start()
        {
            if (stageEditor == null)
            {
                stageEditor = GetComponent<StageEditor>();
            }

            if (maxElectric == 0)
            {
                maxElectric = 6;
            }

            ResetStage();
        }

        public void ResetStage()
        {
            if (stageData == null)
            {
                Debug.LogError("Stage Data가 존재하지 않습니다!");
                return;
            }
            
            stageEditor.ClearStage();
            stageEditor.CreateStageInScene(stageData.map);
        }

        public void CheckAnswer()
        {
            // 맵 정보를 가져오기
            StageTile[,] stageMatrix = MakeStageMatrix();
            TileStruct[,] map = MakeStageToTileStruct(ref stageMatrix);
            
            // 어떤 정답을 체크할지를 확인해야함.
            // 정답 여부 확인하기
            bool isSuccess = stageData.stageType switch
            {
                StageType.DEFAULT => CheckDefaultMapAnswer(map),
                StageType.AMPLIFIER => CheckAmplifierMapAnswer(map),
                StageType.MODULATOR => CheckModulatorMapAnswer(map),
                StageType.AMP_MOD => CheckAmpAndModMapAnswer(map),
                _ => false
            };

            // 정답이 맞을 경우 전체 비용 계산하기
            if (!isSuccess) return;
            
            int cost = (from StageTile stageTile in stageMatrix 
                where stageTile.tile.tileType is not 
                    (ScriptableObjects.Stage.Tile.FACTORY 
                    or ScriptableObjects.Stage.Tile.OBSTACLE 
                    or ScriptableObjects.Stage.Tile.GENERATOR)
                select (int)stageTile.tile.cost).Sum();
            
            Debug.Log("정답! 비용: " + cost);
        }

        private bool CheckDefaultMapAnswer(in TileStruct[,] map)
        {
            Vector2Int startPoint = GetStartPoint(map);
            int numOfFactories = GetFactoriesCount(map);
            int width = stageArea.width;
            int height = stageArea.height;
            
            bool[,] visit = new bool[width, height];
            int[] dX = { 0, 1, 0, -1 };
            int[] dY = { 1, 0, -1, 0 };
            if (startPoint is { x: -1, y: -1 })
            {
                // 발전기를 찾지 못함.
                Debug.LogError("발전기를 찾을 수 없습니다.");
                return false;
            }

            Queue<Vector2Int> queue = new Queue<Vector2Int>();
            queue.Enqueue(startPoint);
            visit[startPoint.x, startPoint.y] = true;
            
            while (queue.Count > 0)
            {
                Vector2Int curr = queue.Dequeue();
                Debug.Log("현재 위치: " + curr + ", 타일 종류: " + map[curr.x,curr.y].tile.tileType + ", 타일 방향: " + map[curr.x, curr.y].dir);
                int nX, nY;
                switch (map[curr.x, curr.y].tile.tileType) 
                {
                    case ScriptableObjects.Stage.Tile.FACTORY:
                        numOfFactories--;
                        break;
                    case ScriptableObjects.Stage.Tile.GENERATOR:
                    case ScriptableObjects.Stage.Tile.DISTRIBUTOR:
                        for (int i = 0; i < 4; i++)
                        {
                            nX = curr.x + dX[i];
                            nY = curr.y + dY[i];
                            if (nX < 0 || nX >= width || nY < 0 || nY >= height || visit[nX, nY])
                            {
                                continue;
                            }
                            if (map[nX, nY].tile.tileType is ScriptableObjects.Stage.Tile.NONE
                                or ScriptableObjects.Stage.Tile.OBSTACLE
                                or ScriptableObjects.Stage.Tile.GENERATOR)
                            {
                                continue;
                            }
                            queue.Enqueue(new Vector2Int(nX, nY));
                            visit[nX, nY] = true;
                        }
                        break;    
                    case ScriptableObjects.Stage.Tile.OBSTACLE:
                        break;
                    default:
                        int dir = GetDirToInt(map[curr.x, curr.y].dir);
                        nX = curr.x + dX[dir];
                        nY = curr.y + dY[dir];
                        if (nX < 0 || nX >= width || nY < 0 || nY >= height || visit[nX, nY])
                        {
                            Debug.Log("범위 밖 " + width + ", " + height);
                            continue;
                        }
                        if (map[nX, nY].tile.tileType is ScriptableObjects.Stage.Tile.NONE
                            or ScriptableObjects.Stage.Tile.OBSTACLE
                            or ScriptableObjects.Stage.Tile.GENERATOR)
                        {
                            continue;
                        }
                        queue.Enqueue(new Vector2Int(nX, nY));
                        visit[nX, nY] = true;
                        break;
                }
            }
            
            if (numOfFactories == 0)
            {
                Debug.Log("정답!");
                return true;
            }
            
            Debug.Log("오답! " + numOfFactories);
            return false;
        }
        
        private bool CheckAmplifierMapAnswer(in TileStruct[,] map)
        {
            Debug.Log("증폭기 문제 풀이여부 확인 시작!");
            Vector2Int startPoint = GetStartPoint(map);
            int numOfFactories = GetFactoriesCount(map);
            int width = stageArea.width;
            int height = stageArea.height;
            
            bool[,] visit = new bool[width, height];
            int[] dX = { 0, 1, 0, -1 };
            int[] dY = { 1, 0, -1, 0 };
            if (startPoint is { x: -1, y: -1 })
            {
                // 발전기를 찾지 못함.
                Debug.LogError("발전기를 찾을 수 없습니다.");
                return false;
            }

            Queue<TileSearchNode> queue = new Queue<TileSearchNode>();
            queue.Enqueue(new TileSearchNode(startPoint, maxElectric, 0));
            visit[startPoint.x, startPoint.y] = true;
            
            while (queue.Count > 0)
            {
                TileSearchNode curr = queue.Dequeue();
                Debug.Log("현재 위치: " + curr + ", 타일 종류: " + map[curr.x,curr.y].tile.tileType + ", 타일 방향: " + map[curr.x, curr.y].dir + " 남은 전기량: " + curr.remainElectric);
                if (curr.remainElectric < 0)
                {
                    continue;
                }

                int nX, nY;
                switch (map[curr.x, curr.y].tile.tileType) 
                {
                    case ScriptableObjects.Stage.Tile.FACTORY:
                        numOfFactories--;
                        break;
                    case ScriptableObjects.Stage.Tile.GENERATOR:
                    case ScriptableObjects.Stage.Tile.DISTRIBUTOR:
                        for (int i = 0; i < 4; i++)
                        {
                            nX = curr.x + dX[i];
                            nY = curr.y + dY[i];
                            if (nX < 0 || nX >= width || nY < 0 || nY >= height || visit[nX, nY])
                            {
                                continue;
                            }
                            if (map[nX, nY].tile.tileType is ScriptableObjects.Stage.Tile.NONE
                                or ScriptableObjects.Stage.Tile.OBSTACLE
                                or ScriptableObjects.Stage.Tile.GENERATOR)
                            {
                                continue;
                            }
                            queue.Enqueue(new TileSearchNode(nX, nY, curr.remainElectric - 1, curr.electricType));
                            visit[nX, nY] = true;
                        }
                        break;    
                    case ScriptableObjects.Stage.Tile.OBSTACLE:
                        break;
                    default:
                        if (map[curr.x, curr.y].tile.tileType == ScriptableObjects.Stage.Tile.AMPLIFIER)
                        {
                            curr.remainElectric = maxElectric;
                        }
                        int dir = GetDirToInt(map[curr.x, curr.y].dir);
                        nX = curr.x + dX[dir];
                        nY = curr.y + dY[dir];
                        if (nX < 0 || nX >= width || nY < 0 || nY >= height || visit[nX, nY])
                        {
                            Debug.Log("범위 밖 " + width + ", " + height);
                            continue;
                        }
                        if (map[nX, nY].tile.tileType is ScriptableObjects.Stage.Tile.NONE
                            or ScriptableObjects.Stage.Tile.OBSTACLE
                            or ScriptableObjects.Stage.Tile.GENERATOR)
                        {
                            continue;
                        }
                        queue.Enqueue(new TileSearchNode(nX, nY, curr.remainElectric - 1, curr.electricType));
                        visit[nX, nY] = true;
                        break;
                }
            }
            
            if (numOfFactories == 0)
            {
                Debug.Log("정답!");
                return true;
            }
            
            Debug.Log("오답! " + numOfFactories);
            return false;
        }
        
        private bool CheckModulatorMapAnswer(in TileStruct[,] map)
        {
            Debug.Log("변환기 문제 풀이여부 확인 시작!");
            Vector2Int startPoint = GetStartPoint(map);
            int numOfFactories = GetFactoriesCount(map);
            int width = stageArea.width;
            int height = stageArea.height;
            
            bool[,] visit = new bool[width, height];
            int[] dX = { 0, 1, 0, -1 };
            int[] dY = { 1, 0, -1, 0 };
            if (startPoint is { x: -1, y: -1 })
            {
                // 발전기를 찾지 못함.
                Debug.LogError("발전기를 찾을 수 없습니다.");
                return false;
            }

            Queue<TileSearchNode> queue = new Queue<TileSearchNode>();
            queue.Enqueue(new TileSearchNode(startPoint, 0, 0));
            visit[startPoint.x, startPoint.y] = true;
            
            while (queue.Count > 0)
            {
                TileSearchNode curr = queue.Dequeue();
                Debug.Log("현재 위치: " + curr + ", 타일 종류: " + map[curr.x,curr.y].tile.tileType + ", 타일 방향: " + map[curr.x, curr.y].dir + " 현재 전기 종류: " + curr.electricType);

                int nX, nY;
                switch (map[curr.x, curr.y].tile.tileType) 
                {
                    case ScriptableObjects.Stage.Tile.FACTORY:
                        if (map[curr.x, curr.y].electricType == curr.electricType)
                        {
                            numOfFactories--;
                        }
                        break;
                    case ScriptableObjects.Stage.Tile.GENERATOR:
                    case ScriptableObjects.Stage.Tile.DISTRIBUTOR:
                        for (int i = 0; i < 4; i++)
                        {
                            nX = curr.x + dX[i];
                            nY = curr.y + dY[i];
                            if (nX < 0 || nX >= width || nY < 0 || nY >= height || visit[nX, nY])
                            {
                                continue;
                            }
                            if (map[nX, nY].tile.tileType is ScriptableObjects.Stage.Tile.NONE
                                or ScriptableObjects.Stage.Tile.OBSTACLE
                                or ScriptableObjects.Stage.Tile.GENERATOR)
                            {
                                continue;
                            }
                            queue.Enqueue(new TileSearchNode(nX, nY, curr.remainElectric, curr.electricType));
                            visit[nX, nY] = true;
                        }
                        break;    
                    case ScriptableObjects.Stage.Tile.OBSTACLE:
                        break;
                    default:
                        if (map[curr.x, curr.y].tile.tileType == ScriptableObjects.Stage.Tile.MODULATOR)
                        {
                            curr.electricType = map[curr.x, curr.y].electricType;
                        }
                        int dir = GetDirToInt(map[curr.x, curr.y].dir);
                        nX = curr.x + dX[dir];
                        nY = curr.y + dY[dir];
                        if (nX < 0 || nX >= width || nY < 0 || nY >= height || visit[nX, nY])
                        {
                            Debug.Log("범위 밖 " + width + ", " + height);
                            continue;
                        }
                        if (map[nX, nY].tile.tileType is ScriptableObjects.Stage.Tile.NONE
                            or ScriptableObjects.Stage.Tile.OBSTACLE
                            or ScriptableObjects.Stage.Tile.GENERATOR)
                        {
                            continue;
                        }
                        queue.Enqueue(new TileSearchNode(nX, nY, curr.remainElectric, curr.electricType));
                        visit[nX, nY] = true;
                        break;
                }
            }
            
            if (numOfFactories == 0)
            {
                Debug.Log("정답!");
                return true;
            }
            
            Debug.Log("오답! " + numOfFactories);
            return false;
        }
        
        private bool CheckAmpAndModMapAnswer(in TileStruct[,] map)
        {
            Debug.Log("증폭기, 변환기 문제 풀이여부 확인 시작!");
            Vector2Int startPoint = GetStartPoint(map);
            int numOfFactories = GetFactoriesCount(map);
            int width = stageArea.width;
            int height = stageArea.height;
            
            bool[,] visit = new bool[width, height];
            int[] dX = { 0, 1, 0, -1 };
            int[] dY = { 1, 0, -1, 0 };
            if (startPoint is { x: -1, y: -1 })
            {
                // 발전기를 찾지 못함.
                Debug.LogError("발전기를 찾을 수 없습니다.");
                return false;
            }

            Queue<TileSearchNode> queue = new Queue<TileSearchNode>();
            queue.Enqueue(new TileSearchNode(startPoint, maxElectric, 0));
            visit[startPoint.x, startPoint.y] = true;
            
            while (queue.Count > 0)
            {
                TileSearchNode curr = queue.Dequeue();
                Debug.Log("현재 위치: " + curr + ", 타일 종류: " + map[curr.x,curr.y].tile.tileType + ", 타일 방향: " + map[curr.x, curr.y].dir + " 현재 전기 종류: " + curr.electricType);
                int nX, nY;
                switch (map[curr.x, curr.y].tile.tileType)
                {
                    case ScriptableObjects.Stage.Tile.FACTORY:
                        if (map[curr.x, curr.y].electricType == curr.electricType)
                        {
                            numOfFactories--;
                        }
                        break;
                    case ScriptableObjects.Stage.Tile.GENERATOR:
                    case ScriptableObjects.Stage.Tile.DISTRIBUTOR:
                        for (int i = 0; i < 4; i++)
                        {
                            nX = curr.x + dX[i];
                            nY = curr.y + dY[i];
                            if (nX < 0 || nX >= width || nY < 0 || nY >= height || visit[nX, nY])
                            {
                                continue;
                            }
                            if (map[nX, nY].tile.tileType is ScriptableObjects.Stage.Tile.NONE
                                or ScriptableObjects.Stage.Tile.OBSTACLE
                                or ScriptableObjects.Stage.Tile.GENERATOR)
                            {
                                continue;
                            }
                            queue.Enqueue(new TileSearchNode(nX, nY, curr.remainElectric - 1, curr.electricType));
                            visit[nX, nY] = true;
                        }
                        break;    
                    case ScriptableObjects.Stage.Tile.OBSTACLE:
                        break;
                    default:
                        if (map[curr.x, curr.y].tile.tileType == ScriptableObjects.Stage.Tile.AMPLIFIER)
                        {
                            curr.remainElectric = maxElectric;
                        }
                        if (map[curr.x, curr.y].tile.tileType == ScriptableObjects.Stage.Tile.MODULATOR)
                        {
                            curr.electricType = map[curr.x, curr.y].electricType;
                        }
                        int dir = GetDirToInt(map[curr.x, curr.y].dir);
                        nX = curr.x + dX[dir];
                        nY = curr.y + dY[dir];
                        if (nX < 0 || nX >= width || nY < 0 || nY >= height || visit[nX, nY])
                        {
                            Debug.Log("범위 밖 " + width + ", " + height);
                            continue;
                        }
                        if (map[nX, nY].tile.tileType is ScriptableObjects.Stage.Tile.NONE
                            or ScriptableObjects.Stage.Tile.OBSTACLE
                            or ScriptableObjects.Stage.Tile.GENERATOR)
                        {
                            continue;
                        }
                        queue.Enqueue(new TileSearchNode(nX, nY, curr.remainElectric - 1, curr.electricType));
                        visit[nX, nY] = true;
                        break;
                }
            }
            
            if (numOfFactories == 0)
            {
                Debug.Log("정답!");
                return true;
            }
            
            Debug.Log("오답! " + numOfFactories);
            return false;
        }

        private int GetDirToInt(Direction dir)
        {
            return  dir switch
            {
                Direction.UP => 2,
                Direction.RIGHT => 1,
                Direction.DOWN => 0,
                Direction.LEFT => 3
            };
        }
        private Vector2Int GetStartPoint(in TileStruct[,] map)
        {
            int width = stageArea.width;
            int height = stageArea.height;
            Vector2Int startPoint = new Vector2Int();
            
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (map[x, y].tile.tileType != ScriptableObjects.Stage.Tile.GENERATOR) 
                        continue;
                    return  new Vector2Int(x, y);
                }
            }

            return new Vector2Int(-1, -1);
        }

        private int GetFactoriesCount(in TileStruct[,] map)
        {
            int width = stageArea.width;
            int height = stageArea.height;
            int numOfFactories = 0;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (map[x, y].tile.tileType == ScriptableObjects.Stage.Tile.FACTORY)
                    {
                        numOfFactories++;
                    } 
                        
                }
            }

            return numOfFactories;
        }
        private StageTile[,] MakeStageMatrix()
        {
            int width = stageArea.width;
            int height = stageArea.height;

            StageTile[,] stageMat = new StageTile[width, height];
            
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    StageTile stageInstance = stageArea.transform.GetChild(i * width + j).GetComponent<StageTile>();
                    stageMat[j, i] = stageInstance;
                }
            }

            return stageMat;
        }
        
        private TileStruct[,] MakeStageToTileStruct(ref StageTile[,] stageMatrix)
        {
            int width = stageMatrix.GetLength(0);
            int height = stageMatrix.GetLength(1);

            TileStruct[,] map = new TileStruct[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    map[x, y] = new TileStruct(stageMatrix[x, y]);
                    map[x, y].electricType = stageMatrix[x, y].electricType;
                }
            }

            return map;
        }
    }

    public class TileSearchNode
    {
        public int x, y;
        public int remainElectric;
        public int electricType;

        public TileSearchNode(int x, int y, int remainElectric, int electricType)
        {
            this.x = x;
            this.y = y;
            this.remainElectric = remainElectric;
            this.electricType = electricType;
        }

        public TileSearchNode(Vector2Int pos, int remainElectric, int electricType)
        {
            this.x = pos.x;
            this.y = pos.y;
            this.remainElectric = remainElectric;
            this.electricType = electricType;
        }
    }
}
