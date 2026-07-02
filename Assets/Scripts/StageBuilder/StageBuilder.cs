using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Priority_Queue;
using ScriptableObjects.Stage;
using SFB;
using Sirenix.OdinInspector;
using Stage;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using Tile = ScriptableObjects.Stage.Tile;

#if UNITY_EDITOR
namespace StageBuilder
{
    public class StageBuilder : MonoBehaviour
    {
        [SerializeField] 
        private StageArea stageArea;

        [SerializeField] 
        private TMP_Text costText;

        [SerializeField] 
        private StageEditor stageEditor;

        private TileStruct[,] beginStage;

        [SerializeField] 
        private List<TileScriptableObject> tileList;
        private Dictionary<Tile, TileScriptableObject> tileDict = new Dictionary<Tile, TileScriptableObject>();
        private readonly int[] dX = {0, 1, 0, -1};
        private readonly int[] dY = {1, 0, -1, 0};
        private readonly string basePath = "Assets/ScriptableObjects/";
        private void Start()
        {
            InitTileDict();
        }

        #region Find Answer

        [Button]
        public void FindAnswer()
        {
            List<Vector2> factories = new List<Vector2>();
            List<Vector2> generators = new List<Vector2>();
            StageTile[,] stageMatrix = MakeStageMatrix();
            beginStage = MakeStageToTileStruct(ref stageMatrix);

            FindGenAndFactories(ref factories, ref generators, ref stageMatrix);
            if (generators.Count == 0)
            {
                Debug.LogError("발전기가 존재하지 않습니다.");
                costText.text = "Cost: Error";
                return;
            }

            if (factories.Count == 0)
            {
                Debug.LogError("공장이 존재하지 않습니다.");
                costText.text = "Cost: Error";
                return;
            }

            if (factories.Count > 1)
            {
                Debug.LogWarning(
                    "공장이 여러 개인 스테이지는 자동 최단 경로 계산을 생략합니다. " +
                    "초기 맵은 저장되었으므로 정답을 수동 편집한 뒤 스테이지 파일을 생성하세요.");
                costText.text = "Cost: Manual";
                return;
            }

            PathStruct[,,,] minPaths = null;
            InitMinPathMatrix(ref minPaths);

            PathStruct minPath = null;
            foreach (Vector2 generator in generators)
            {
                GetMinPaths(generator, ref stageMatrix, ref minPaths);
                PathStruct candidate = GetMinPath(generator, Tile.GENERATOR, factories, ref minPaths);
                if (candidate.cost >= float.MaxValue)
                {
                    continue;
                }

                if (minPath == null || candidate.cost < minPath.cost)
                {
                    minPath = candidate;
                }
            }

            if (minPath == null)
            {
                Debug.LogError("공장에 도달할 수 있는 발전기 경로가 없습니다.");
                costText.text = "Cost: Unreachable";
                return;
            }

            Debug.Log(minPath);
            costText.text = "Cost: " + minPath.cost;
            ApplyStage(minPath.path, ref stageMatrix);
        }
        

        private void GetMinPaths(
            Vector2 startPoint,
            ref StageTile[,] stageMatrix,
            ref PathStruct[,,,] minPath)
        {
            int width = stageArea.width;
            int height = stageArea.height;
            int startX = (int)startPoint.x;
            int startY = (int)startPoint.y;

            FastPriorityQueue<Node> priorityQueue = new FastPriorityQueue<Node>(width * height * 10);
            priorityQueue.Enqueue(new Node(startX, startY, Direction.NONE), 0);

            while (priorityQueue.Count != 0)
            {
                Node current = priorityQueue.Dequeue();
                for (int i = 0; i < 4; i++)
                {
                    int nX = current.x + dX[i];
                    int nY = current.y + dY[i];

                    if (nX < 0 || nX >= width || nY < 0 || nY >= height)
                    {
                        continue;
                    }

                    Tile targetTile = stageMatrix[nX, nY].tile.tileType;
                    if (targetTile is Tile.OBSTACLE or Tile.GENERATOR)
                    {
                        continue;
                    }

                    Direction nDir = IntToDirection(i);
                    Tile nTile = GetNextTile(current.dir, nDir);
                    float nCost = -current.Priority + tileDict[nTile].cost;

                    if (minPath[startX, startY, nX, nY].cost >= nCost)
                    {
                        Node nNode = new Node(nX, nY, current.root);
                        if (current.dir != Direction.NONE && nTile != Tile.NONE)
                        {
                            TileNode node = new TileNode(current.x, current.y, nDir, nTile);
                            nNode.AddLast(node);
                        }
                        nNode.dir = nDir;

                        minPath[startX, startY, nX, nY].cost = nCost;
                        minPath[startX, startY, nX, nY].path = nNode.root;

                        if (targetTile != Tile.FACTORY)
                        {
                            priorityQueue.Enqueue(nNode, -nCost);
                        }
                    }
                }
            }
        }

        private void FindGenAndFactories(
            ref List<Vector2> factories,
            ref List<Vector2> generators,
            ref StageTile[,] stageMatrix)
        {
            int width = stageArea.width;
            int height = stageArea.height;
            
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (stageMatrix[i, j].tile.tileType == Tile.FACTORY)
                    {
                        factories.Add(new Vector2(i, j));
                    }

                    if (stageMatrix[i, j].tile.tileType == Tile.GENERATOR)
                    {
                        generators.Add(new Vector2(i, j));
                    }
                }
            }
        }

        
        private PathStruct FindMinPath(Vector2 startPoint, Tile startTile, List<Vector2> factories, ref StageTile[,] stageMatrix, ref PathStruct[,,,] minPaths, int leastCost, bool dupCall = false)
        {
            if (factories == null || factories.Count == 0)
            {
                Debug.LogError("공장이 존재하지 않습니다.");
                return null;
            }

            if (leastCost <= 0)
            {
                return new PathStruct() { cost = 9999 };
            }
            int width = stageArea.width;
            int height = stageArea.height;
            PathStruct minPath = new PathStruct() {cost = leastCost};
                
            // 단순히 각 경로를 연결
            PathStruct direct = GetMinPath(startPoint, startTile, factories, ref minPaths);
            if (direct.cost <= minPath.cost)
            {
                minPath.path = direct.path;
                minPath.cost = direct.cost;
            }
            
            // 중간 경로를 추가하고 연결
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (x == (int)startPoint.x && y == (int)startPoint.y)
                    {
                        continue;
                    }
                    Tile tileType = stageMatrix[x, y].tile.tileType;
                    if (tileType is Tile.OBSTACLE or Tile.FACTORY or Tile.GENERATOR)
                    {
                        continue;
                    }
                    PathStruct tempPath = new PathStruct
                    {
                        path = new TileNode((int)startPoint.x, (int)startPoint.y, Direction.NONE, startTile),
                        cost = tileDict[startTile].cost
                    };
                    PathStruct transitPath = minPaths[(int)startPoint.x, (int)startPoint.y, x, y];
                    tempPath.cost += transitPath.cost;
                    if (tempPath.cost > minPath.cost)
                    {
                        continue;
                    }
                    
                    PathStruct factoriesDirectPath = GetMinPath(new Vector2(x, y), Tile.DISTRIBUTOR, factories, ref minPaths);
                    tempPath.cost += factoriesDirectPath.cost;
                    

                    if (tempPath.cost < minPath.cost)
                    {
                        tempPath.AddFirst(transitPath);
                        tempPath.AddLast(factoriesDirectPath);
                        minPath.path = tempPath.path;
                        minPath.cost = tempPath.cost;
                    }
                }
            }
            
            // 중간 경로를 사용하는 것과 그렇지 않은것을 구분
            if (factories.Count > 2)
            {
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        if (x == (int)startPoint.x && y == (int)startPoint.y)
                        {
                            continue;
                        }
                        Tile tileType = stageMatrix[x, y].tile.tileType;
                        if (tileType is Tile.OBSTACLE or Tile.FACTORY or Tile.GENERATOR)
                        {
                            continue;
                        }
                        for (int i = 0; i < factories.Count; i++)
                        {
                            PathStruct tempPath = new PathStruct
                            {
                                path = new TileNode((int)startPoint.x, (int)startPoint.y, Direction.NONE, startTile),
                                cost = tileDict[startTile].cost
                            };
                            PathStruct transitPath = minPaths[(int)startPoint.x, (int)startPoint.y, x, y];
                            tempPath.cost += transitPath.cost;
                            if (tempPath.cost > leastCost)
                            {
                                continue;
                            }
                            
                            List<Vector2> tempFactory = factories.ConvertAll(item => item);
                            tempFactory.RemoveAt(i);
                            PathStruct factoriesTransitPath = FindMinPath(new Vector2(x, y), Tile.DISTRIBUTOR,
                                tempFactory,ref stageMatrix, ref minPaths, (int)(minPath.cost - tempPath.cost));
                            tempPath.cost += factoriesTransitPath.cost;
                            if (tempPath.cost > leastCost)
                            {
                                continue;
                            }
                            
                            PathStruct leftFactoryPath = minPaths[(int)startPoint.x, (int)startPoint.y, (int)factories[i].x, (int)factories[i].y];
                            tempPath.cost += leftFactoryPath.cost;
                            
                            if (tempPath.cost < minPath.cost)
                            {
                                tempPath.AddFirst(transitPath);
                                tempPath.AddFirst(leftFactoryPath);
                                tempPath.AddLast(factoriesTransitPath);
                                minPath.path = tempPath.path;
                                minPath.cost = tempPath.cost;
                            }
                        }

                        if (dupCall) continue;
                        {
                            PathStruct tempPath = new PathStruct
                            {
                                path = new TileNode((int)startPoint.x, (int)startPoint.y, Direction.NONE, startTile),
                                cost = tileDict[startTile].cost
                            };
                            PathStruct transitPath = minPaths[(int)startPoint.x, (int)startPoint.y, x, y];
                            tempPath.cost += transitPath.cost;
                            if (tempPath.cost > leastCost)
                            {
                                continue;
                            }
                            
                            PathStruct factoriesTransitPath = FindMinPath(new Vector2(x, y), Tile.DISTRIBUTOR,
                                factories,ref stageMatrix, ref minPaths, (int)(minPath.cost - tempPath.cost),true);
                            tempPath.cost += factoriesTransitPath.cost;
                          
                            if (tempPath.cost < minPath.cost)
                            {
                                tempPath.AddFirst(transitPath);
                                tempPath.AddLast(factoriesTransitPath);
                                minPath.path = tempPath.path;
                                minPath.cost = tempPath.cost;
                            }
                        }
                    }
                }
            }

            return minPath;
        }

        private PathStruct GetMinPath(Vector2 startPoint, Tile startTile, List<Vector2> factories,
            ref PathStruct[,,,] minPaths)
        {
            // 단순히 각 경로를 연결
            PathStruct minPath = new PathStruct();
            minPath.cost = tileDict[startTile].cost;
            minPath.path = new TileNode((int)startPoint.x, (int)startPoint.y, Direction.NONE, startTile);
            foreach (var factory in factories)
            {
                PathStruct directPath = minPaths[(int)startPoint.x, (int)startPoint.y, (int)factory.x, (int)factory.y];
                if (directPath.path != null)
                {
                    minPath.path.nexts.Add(directPath.path);
                }
                minPath.cost += directPath.cost;
                
                // 경로 마지막에 Factory Tile을 추가하는 부분. 이를 추가하면 시간 복잡도가 크게 상승함.
                /*
                if (directPath.path != null)
                {
                    TileNode copyPath = directPath.path.Copy();
                    copyPath.GetLast().nexts.Add(new TileNode((int)factory.x, (int)factory.y, Direction.NONE, Tile.FACTORY));
                    minPath.path.nexts.Add(copyPath);
                    minPath.cost += directPath.cost;
                }
                else
                {
                    minPath.path.nexts.Add(new TileNode((int)factory.x, (int)factory.y, Direction.NONE, Tile.FACTORY));
                    minPath.cost += directPath.cost;
                }
                */
            }
            
            return minPath;
        }
        private PathStruct ApplyAmplifier()
        {
            // 경로를 전기를 포함한 경로로 변경
            // 공장에서 시작하는 경로 생성.
            // 길이가 가장 긴 공장에서 시작하여 증폭기를 적용.
            throw new NotImplementedException();
        }

        private PathStruct ApplyModulator()
        {
            throw new NotImplementedException();
        }

        private void ApplyStage(TileNode node ,ref StageTile[,] stageMatrix)
        {
            stageMatrix[node.x, node.y].tile = tileDict[node.tile];
            stageMatrix[node.x, node.y].direction = node.dir;

            foreach (var next in node.nexts)
            {
                ApplyStage(next, ref stageMatrix);
            }
        }

        #endregion
        
        #region Save And Load File
        
        public void MakeStageFile()
        {
#if UNITY_EDITOR
            if (beginStage == null)
            {
                Debug.LogError(
                    "초기 맵 정보가 없습니다. FindAnswer를 실행해 초기 맵을 캡처한 뒤 " +
                    "자동 또는 수동으로 정답 맵을 작성하세요.");
                return;
            }

            var path = StandaloneFileBrowser.SaveFilePanel("Save Stage File", basePath, "sample", "asset");
            if (path != "")
            {
                StageTile[,] map = MakeStageMatrix();
                StageScriptableObject stage = ScriptableObject.CreateInstance<StageScriptableObject>();
                stage.MakeMapByStageTiles(beginStage, MakeStageToTileStruct(ref map));
                stage.stageName = Path.GetFileNameWithoutExtension(path);
                
                string relativePath = "Assets" + path.Substring(Application.dataPath.Length);
                relativePath = relativePath.Replace('\\', '/');
                UnityEditor.AssetDatabase.CreateAsset(stage, relativePath);
                UnityEditor.AssetDatabase.SaveAssets();
            }
#endif
            return;
        }

        public void LoadStageFile()
        {
#if UNITY_EDITOR
            var paths = StandaloneFileBrowser.OpenFilePanel("Find Stage File", basePath, "asset", false);
            if (paths.Length > 0) {
                Debug.Log(paths[0]);
                string relativePath = "Assets" + paths[0].Substring(Application.dataPath.Length);
                relativePath = relativePath.Replace('\\', '/');
                Debug.Log(relativePath);
                StageScriptableObject stage = AssetDatabase.LoadAssetAtPath<StageScriptableObject>(relativePath);
                if (stage != null)
                {
                    stageEditor.CreateStageInScene(stage.map, true);
                    beginStage = stage.map;
                }
            }
#endif
            return;
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
#if UNITY_EDITOR
                    Debug.Log("x : " + x + " y : " + y + " type : " + map[x, y].electricType);
#endif
                    map[x, y].electricType = stageMatrix[x, y].electricType;
                }
            }

            return map;
        }
        
        #endregion

        #region Tools and Initialize
        
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
#if UNITY_EDITOR
                    Debug.Log("x : " + i + " y : " + j + " type : " + stageInstance.electricType);
#endif
                }
            }

            return stageMat;
        }
        
        private void InitMinPathMatrix(ref PathStruct[,,,] minPath)
        {
            int width = stageArea.width;
            int height = stageArea.height;
            minPath = new PathStruct[width,height, width,height];

            for (int i1 = 0; i1 < width; i1++)
            {
                for (int j1 = 0; j1 < height; j1++)
                {
                    for (int i2 = 0; i2 < width; i2++)
                    {
                        for (int j2 = 0; j2 < height; j2++)
                        {
                            minPath[i1, j1, i2, j2] = new PathStruct();
                            if (j1 == j2 && i1 == i2)
                            {
                                minPath[i1, j1, i2, j2].cost = 0;
                            }
                        }
                    }
                }
            }
        }

        private void InitTileDict()
        {
            foreach (var tileScriptableObject in tileList)
            {
                tileDict.Add(tileScriptableObject.tileType, tileScriptableObject);
            }
        }
        
        private void PrintPaths(ref PathStruct[,,,] minPath)
        {
            int width = stageArea.width;
            int height = stageArea.height;
            for (int i1 = 0; i1 < width; i1++)
            {
                for (int j1 = 0; j1 < height; j1++)
                {
                    for (int i2 = 0; i2 < width; i2++)
                    {
                        for (int j2 = 0; j2 < height; j2++)
                        {
                            Debug.Log(i1 + ", " + j1 + " -> " + i2 + ", "+ j2 + ": " + minPath[i1,j1,i2,j2]);
                        }
                    }
                }
            }
        }
        
        private Direction IntToDirection(int dir)
        {
            Direction direction = dir switch
            {
                0 => Direction.DOWN,
                1 => Direction.RIGHT,
                2 => Direction.UP,
                3 => Direction.LEFT,
                _ => Direction.NONE
            };
            return direction;
        }
        
        private Tile GetNextTile(Direction lastDir, Direction currentDir)
        {
            Tile ret = Tile.NONE;
            switch (lastDir)
            {
                case Direction.UP:
                    if (currentDir is Direction.UP or Direction.DOWN)
                    {
                        ret = Tile.LINE;
                    }
                    else if (currentDir is Direction.LEFT)
                    {
                        ret = Tile.CORNER_LEFT;
                    }
                    else if (currentDir is Direction.RIGHT)
                    {
                        ret = Tile.CORNER_RIGHT;
                    }
                    break;
                case Direction.DOWN:
                    if (currentDir is Direction.UP or Direction.DOWN)
                    {
                        ret = Tile.LINE;
                    }
                    else if (currentDir is Direction.LEFT)
                    {
                        ret = Tile.CORNER_RIGHT;
                    }
                    else if (currentDir is Direction.RIGHT)
                    {
                        ret = Tile.CORNER_LEFT;
                    }
                    break;
                case Direction.LEFT:
                    if (currentDir is Direction.LEFT or Direction.RIGHT)
                    {
                        ret = Tile.LINE;
                    }
                    else if (currentDir is Direction.DOWN)
                    {
                        ret = Tile.CORNER_LEFT;
                    }
                    else if (currentDir is Direction.UP)
                    {
                        ret = Tile.CORNER_RIGHT;
                    }
                    break;
                case Direction.RIGHT:
                    if (currentDir is Direction.LEFT or Direction.RIGHT)
                    {
                        ret = Tile.LINE;
                    }
                    else if (currentDir is Direction.DOWN)
                    {
                        ret = Tile.CORNER_RIGHT;
                    }
                    else if (currentDir is Direction.UP)
                    {
                        ret = Tile.CORNER_LEFT;
                    }
                    break;
                case Direction.NONE:
                default:
                    ret = Tile.NONE;
                    break;
            }
            return ret;
        }
        
        private class Node : FastPriorityQueueNode
        {
            public int x, y;
            public TileNode root;
            private TileNode last;
            public Direction dir = Direction.NONE;

            public Node(int x, int y, TileNode path)
            {
                this.x = x;
                this.y = y;
                if (path != null)
                {
                    root = new TileNode(path);
                    last = root;
                    TileNode node = path;
                    while(node != null)
                    {
                        last.nexts = node.nexts.ConvertAll(item => new TileNode(item));
                        last = last.nexts.Count < 1 ? last : last.nexts[0];
                        node = node.nexts.Count < 1? null : node.nexts[0];
                    }
                }
            }
            
            public Node(int x, int y, Direction direction)
            {
                this.x = x;
                this.y = y;
                dir = direction;
            }

            public void AddLast(TileNode node)
            {
                if (root == null)
                {
                    root = last = node;
                }
                else
                {
                    last.nexts.Add(node);
                    last = node;
                }
            }
        }
        
        #endregion
        
    }
}
#endif
