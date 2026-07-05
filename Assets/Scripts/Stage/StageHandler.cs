using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Command;
using ScriptableObjects.Stage;
using Sirenix.OdinInspector;
using Stage.UI;
using StageBuilder;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Stage
{
    public class StageHandler : MonoBehaviour
    {
        [SerializeField] private StageEditor stageEditor;

        public StageScriptableObject stageData;

        [SerializeField] private StageArea stageArea;

        [SerializeField] private EditModeHandler modeHandler;

        [SerializeField] private StarterHandler starterHandler;

        [SerializeField] private CommandHistoryHandler commandHistoryHandler;
        
        [SerializeField] private CostSlider costSlider;
        
        [SerializeField] 
        private Button undoButton;
        [SerializeField] 
        private Button redoButton;
        [SerializeField] 
        private TMP_Text title;
        
        [SerializeField] 
        private ResultPopup resultPopup;

        [SerializeField] 
        private ResumePopup resumePopup;
        
        [SerializeField] 
        private AudioClip backButtonSFX;
        [SerializeField] 
        private AudioClip buttonSFX;
        [SerializeField] 
        private AudioClip answerCheckSFX;
        
        public int maxElectric;
        public float trackTileDelay; // 정답 체크시, 각 타일에 머무는 시간
        public UnityEvent onResetStage;
        public UnityEvent<float> onCostChange;
        public List<int> thresholdAmounts;
        private float _cost;
        private bool canCheckAns;
        
        [ShowInInspector, ReadOnly]
        public float cost
        {
            get => _cost;
            set
            {
                if (_cost != value)
                {
                    onCostChange.Invoke(value);
                    _cost = value;
                }
            }
        }

        private void Awake()
        {
            _cost = 1;
            canCheckAns = true;
            // thresholdAmounts for test
            thresholdAmounts = new List<int> { 250, 260, 270 };
        }

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
            
            undoButton.onClick.AddListener(() =>
            {
                SoundManager.Instance.PlaySFX(buttonSFX);
            });
            redoButton.onClick.AddListener(() =>
            {
                SoundManager.Instance.PlaySFX(buttonSFX);
            });
            commandHistoryHandler.onExecuteCommand.AddListener(SetCostAfterExecuteCommand);
            commandHistoryHandler.onUndoCommand.AddListener(SetCostAfterUndoCommand);
        }

        [Button]
        public void ResetStage(bool canCheckAnswer = true)
        {
            canCheckAns = canCheckAnswer;
            if (stageData == null)
            {
                Debug.LogError("Stage Data가 존재하지 않습니다!");
                return;
            }

            stageEditor.ClearStage();
            bool showElectricType = stageData.stageType is StageType.MODULATOR or StageType.AMP_MOD;
            stageEditor.CreateStageInScene(stageData.map, showElectricType: showElectricType);

            modeHandler.TileHandler.numActivePalate = stageData.numOfPalate;
            onResetStage.Invoke();
            
            thresholdAmounts = stageData.thresholds;
            costSlider.SetThresholdAmount(thresholdAmounts);
            cost = 0;

            title.text = stageData.stageName;
            resumePopup.SetTitle(title.text);
        }

        // 커맨드 히스토리 관련 함수 모음.
        #region About CommandEvent Method

        private void SetCostAfterExecuteCommand(ICommand command)
        {
            switch (command)
            {
                case TileRemoveCommand removeCommand:
                    cost -= removeCommand.beforeTileData.cost;
                    break;
                case TilePlaceCommand placeCommand:
                    cost -= placeCommand.beforeTileData.cost;
                    cost += placeCommand.targetTileData.cost;
                    break;
            }
        }

        private void SetCostAfterUndoCommand(ICommand command)
        {
            switch (command)
            {
                case TileRemoveCommand removeCommand:
                    cost += removeCommand.beforeTileData.cost;
                    break;
                case TilePlaceCommand placeCommand:
                    cost += placeCommand.beforeTileData.cost;
                    cost -= placeCommand.targetTileData.cost;
                    break;
            }
        }

        #endregion

        // 정답 확인 관련 알고리즘 모음.
        // *주의!* 코드가 상당히 긺.
        #region About Check Answer

        public async void CheckAnswer()
        {
            // 정답 체크 가능 여부 확인
            if (!canCheckAns)
            {
                StartCoroutine(starterHandler.ResetHandler());
                return;
            }
            
            // 편집 중단.
            modeHandler.ChangeMode(EditMode.STOP);
            undoButton.enabled = redoButton.enabled = false;
            
            // 맵 정보를 가져오기
            StageTile[,] stageMatrix = MakeStageMatrix();
            TileStruct[,] map = MakeStageToTileStruct(ref stageMatrix);
            
            // 어떤 정답을 체크할지를 확인해야함.
            // 정답 여부 확인하기
            bool isSuccess = stageData.stageType switch
            {
                StageType.DEFAULT => await CheckDefaultMapAnswer(),
                StageType.AMPLIFIER => await CheckAmplifierMapAnswer(),
                StageType.MODULATOR => await CheckModulatorMapAnswer(),
                StageType.AMP_MOD => await CheckAmpAndModMapAnswer(),
                _ => false
            };
            
            int numOfStar = 0;
            if (cost <= thresholdAmounts[0])
            {
                numOfStar = 3;
            }
            else if (cost <= thresholdAmounts[1])
            {
                
                numOfStar = 2;
            }
            else if (cost <= thresholdAmounts[2])
            {
                
                numOfStar = 1;
            }
            
            // 광고 출력
#if UNITY_ANDROID|| UNITY_EDITOR
            AdManager.Instance.CheckAd();
#endif

            if (!canCheckAns || !isSuccess || numOfStar == 0)
            {
                // 실패
                ResetTilesColor();
                modeHandler.ChangeMode(EditMode.DRAW);
                undoButton.enabled = redoButton.enabled = true;
                StartCoroutine(starterHandler.ResetHandler());
                return;
            }
            
            // 정답
            Debug.Log("정답! 비용: " + cost);
            StartCoroutine(resultPopup.Activate(numOfStar, (int)cost));
            
            // 데이터 갱신
            if (PlayerDataHandler.Instance.HasKey(stageData.name))
            {
                PlayerDataHandler.Instance.SetInt(stageData.name, Mathf.Max(PlayerDataHandler.Instance.GetInt(stageData.name), numOfStar));
            }
            else
            {
                PlayerDataHandler.Instance.SetInt(stageData.name, numOfStar);
            }
            PlayerDataHandler.Instance.SaveData();
            
            // 도전과제 갱신
#if !DISABLESTEAMWORKS
            if (stageData.isAchievement)
            {
                if (stageData.isCheapestClearType)
                {
                    if (cost <= stageData.cheapestCost)
                    {
                        // 달성
                        SteamAchievement.Instance.Achieve(stageData.achievementKey);
                    }
                }
                else
                {
                    // 달성
                    SteamAchievement.Instance.Achieve(stageData.achievementKey);
                }
            }
            
            GameManager.Instance.CheckSteamStageClearAchievement();
#endif
#if !DISABLESTOVE
            if (stageData.isAchievement)
            {
                if (stageData.isCheapestClearType)
                {
                    if (cost <= stageData.cheapestCost)
                    {
                        // 달성
                        StoveAchievementHandler.UnlockAchievement(stageData.achievementKey);
                    }
                }
                else
                {
                    // 달성
                    StoveAchievementHandler.UnlockAchievement(stageData.achievementKey);
                }
            }

            GameManager.Instance.CheckStoveStageClearAchievement();
#endif
        }

        private void ResetTilesColor()
        {
            foreach (var stageTile in stageArea.GetComponentsInChildren<StageTile>())
            {
                   stageTile.SetActiveTile(false);
            }
        }

        private void SetActiveTileColor(in StageTile stageTile, int targetElectricType = 0 )
        {
            stageTile.SetActiveTile(true, targetElectricType );
            SoundManager.Instance.PlaySFX(answerCheckSFX);
        }
        
        private async Task<bool> CheckDefaultMapAnswer()
        {
            StageTile[,] stageMatrix = MakeStageMatrix();

            int numOfFactories = stageData.numOfFactories;
            int width = stageArea.width;
            int height = stageArea.height;
            
            bool[,] visit = new bool[width, height];
            int[] dX = { 0, 1, 0, -1 };
            int[] dY = { 1, 0, -1, 0 };
            if (!TryGetGeneratorPositions(stageMatrix, out List<Vector2Int> generatorPositions))
            {
                return false;
            }

            foreach (Vector2Int startPoint in generatorPositions)
            {
                if (visit[startPoint.x, startPoint.y])
                {
                    Debug.LogError("발전소 시작 위치가 중복되었습니다: " + startPoint);
                    return false;
                }

                Stack<Vector2Int> stack = new Stack<Vector2Int>();
                stack.Push(startPoint);
                visit[startPoint.x, startPoint.y] = true;
                
                // 발전소별 DFS
                while (stack.Count > 0)
                {
                    Vector2Int curr = stack.Pop();
                    int nX, nY;

                    switch (stageMatrix[curr.x, curr.y].tile.tileType)
                    {
                        case ScriptableObjects.Stage.Tile.FACTORY:
                            numOfFactories--;
                            SetActiveTileColor(stageMatrix[curr.x, curr.y]);
                            break;
                        case ScriptableObjects.Stage.Tile.GENERATOR:
                        case ScriptableObjects.Stage.Tile.DISTRIBUTOR:
                            for (int i = 0; i < 4; i++)
                            {
                                nX = curr.x + dX[i];
                                nY = curr.y + dY[i];
                                NextTileState branchState = GetNextTileState(stageMatrix, visit, nX, nY, i);
                                if (branchState == NextTileState.AlreadyVisited)
                                {
                                    Debug.Log("전력 경로가 이미 방문한 타일과 겹칩니다: " + new Vector2Int(nX, nY));
                                    return false;
                                }
                                if (branchState is NextTileState.OutOfBounds
                                    or NextTileState.Unavailable
                                    or NextTileState.InvalidDirection)
                                {
                                    continue;
                                }

                                stack.Push(new Vector2Int(nX, nY));
                                visit[nX, nY] = true;
                            }
                            SetActiveTileColor(stageMatrix[curr.x, curr.y]);
                            break;
                        case ScriptableObjects.Stage.Tile.OBSTACLE:
                            break;
                        default:
                            SetActiveTileColor(stageMatrix[curr.x, curr.y]);
                            int dir = GetDirToInt(stageMatrix[curr.x, curr.y].direction);
                            nX = curr.x + dX[dir];
                            nY = curr.y + dY[dir];
                            NextTileState nextState = GetNextTileState(stageMatrix, visit, nX, nY, dir);
                            if (nextState == NextTileState.AlreadyVisited)
                            {
                                Debug.Log("전력 경로가 이미 방문한 타일과 겹칩니다: " + new Vector2Int(nX, nY));
                                return false;
                            }
                            if (nextState == NextTileState.Available)
                            {
                                stack.Push(new Vector2Int(nX, nY));
                                visit[nX, nY] = true;
                            }
                            break;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(trackTileDelay));
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
        
        private async Task<bool> CheckAmplifierMapAnswer()
        {
            StageTile[,] stageMatrix = MakeStageMatrix();
            
            int numOfFactories = stageData.numOfFactories;
            int width = stageArea.width;
            int height = stageArea.height;
            
            bool[,] visit = new bool[width, height];
            int[] dX = { 0, 1, 0, -1 };
            int[] dY = { 1, 0, -1, 0 };
            if (!TryGetGeneratorPositions(stageMatrix, out List<Vector2Int> generatorPositions))
            {
                return false;
            }

            foreach (Vector2Int startPoint in generatorPositions)
            {
                if (visit[startPoint.x, startPoint.y])
                {
                    Debug.LogError("발전소 시작 위치가 중복되었습니다: " + startPoint);
                    return false;
                }

                Stack<TileSearchNode> stack = new Stack<TileSearchNode>();
                stack.Push(new TileSearchNode(startPoint, maxElectric, 0));
                visit[startPoint.x, startPoint.y] = true;

                // 발전소별 DFS
                while (stack.Count > 0)
                {
                    TileSearchNode curr = stack.Pop();
                    if (curr.remainElectric < 0)
                    {
                        continue;
                    }

                    int nX, nY;
                    switch (stageMatrix[curr.x, curr.y].tile.tileType)
                    {
                        case ScriptableObjects.Stage.Tile.FACTORY:
                            numOfFactories--;
                            SetActiveTileColor(stageMatrix[curr.x, curr.y]);
                            break;
                        case ScriptableObjects.Stage.Tile.GENERATOR:
                        case ScriptableObjects.Stage.Tile.DISTRIBUTOR:
                            for (int i = 0; i < 4; i++)
                            {
                                nX = curr.x + dX[i];
                                nY = curr.y + dY[i];
                                NextTileState branchState = GetNextTileState(stageMatrix, visit, nX, nY, i);
                                if (branchState == NextTileState.AlreadyVisited)
                                {
                                    Debug.Log("전력 경로가 이미 방문한 타일과 겹칩니다: " + new Vector2Int(nX, nY));
                                    return false;
                                }
                                if (branchState is NextTileState.OutOfBounds
                                    or NextTileState.Unavailable
                                    or NextTileState.InvalidDirection)
                                {
                                    continue;
                                }

                                stack.Push(new TileSearchNode(nX, nY, curr.remainElectric - 1, curr.electricType));
                                visit[nX, nY] = true;
                            }
                            SetActiveTileColor(stageMatrix[curr.x, curr.y]);
                            break;
                        case ScriptableObjects.Stage.Tile.OBSTACLE:
                            break;
                        default:
                            SetActiveTileColor(stageMatrix[curr.x, curr.y]);
                            if (stageMatrix[curr.x, curr.y].tile.tileType == ScriptableObjects.Stage.Tile.AMPLIFIER)
                            {
                                curr.remainElectric = maxElectric;
                            }
                            int dir = GetDirToInt(stageMatrix[curr.x, curr.y].direction);
                            nX = curr.x + dX[dir];
                            nY = curr.y + dY[dir];
                            NextTileState nextState = GetNextTileState(stageMatrix, visit, nX, nY, dir);
                            if (nextState == NextTileState.AlreadyVisited)
                            {
                                Debug.Log("전력 경로가 이미 방문한 타일과 겹칩니다: " + new Vector2Int(nX, nY));
                                return false;
                            }
                            if (nextState == NextTileState.Available)
                            {
                                stack.Push(new TileSearchNode(nX, nY, curr.remainElectric - 1, curr.electricType));
                                visit[nX, nY] = true;
                            }
                            break;
                    }
                    await Task.Delay(TimeSpan.FromSeconds(trackTileDelay));
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
        
        private async Task<bool> CheckModulatorMapAnswer()
        {
            // Debug.Log("변환기 문제 풀이여부 확인 시작!");
            StageTile[,] stageMatrix = MakeStageMatrix();

            int numOfFactories = stageData.numOfFactories;
            int width = stageArea.width;
            int height = stageArea.height;
            
            bool[,] visit = new bool[width, height];
            int[] dX = { 0, 1, 0, -1 };
            int[] dY = { 1, 0, -1, 0 };
            if (!TryGetGeneratorPositions(stageMatrix, out List<Vector2Int> generatorPositions))
            {
                return false;
            }

            foreach (Vector2Int startPoint in generatorPositions)
            {
                if (visit[startPoint.x, startPoint.y])
                {
                    Debug.LogError("발전소 시작 위치가 중복되었습니다: " + startPoint);
                    return false;
                }

                Stack<TileSearchNode> stack = new Stack<TileSearchNode>();
                stack.Push(new TileSearchNode(
                    startPoint,
                    0,
                    stageMatrix[startPoint.x, startPoint.y].electricType));
                visit[startPoint.x, startPoint.y] = true;

                // 발전소별 DFS
                while (stack.Count > 0)
                {
                    TileSearchNode curr = stack.Pop();

                    int nX, nY;
                    switch (stageMatrix[curr.x, curr.y].tile.tileType)
                    {
                        case ScriptableObjects.Stage.Tile.FACTORY:
                            if (stageMatrix[curr.x, curr.y].electricType == curr.electricType)
                            {
                                SetActiveTileColor(stageMatrix[curr.x, curr.y], curr.electricType);
                                numOfFactories--;
                            }
                            break;
                        case ScriptableObjects.Stage.Tile.GENERATOR:
                        case ScriptableObjects.Stage.Tile.DISTRIBUTOR:
                            for (int i = 0; i < 4; i++)
                            {
                                nX = curr.x + dX[i];
                                nY = curr.y + dY[i];
                                NextTileState branchState = GetNextTileState(stageMatrix, visit, nX, nY, i);
                                if (branchState == NextTileState.AlreadyVisited)
                                {
                                    Debug.Log("전력 경로가 이미 방문한 타일과 겹칩니다: " + new Vector2Int(nX, nY));
                                    return false;
                                }
                                if (branchState is NextTileState.OutOfBounds
                                    or NextTileState.Unavailable
                                    or NextTileState.InvalidDirection)
                                {
                                    continue;
                                }

                                stack.Push(new TileSearchNode(nX, nY, curr.remainElectric, curr.electricType));
                                visit[nX, nY] = true;
                            }
                            SetActiveTileColor(stageMatrix[curr.x, curr.y], curr.electricType);
                            break;
                        case ScriptableObjects.Stage.Tile.OBSTACLE:
                            break;
                        default:
                            SetActiveTileColor(stageMatrix[curr.x, curr.y], curr.electricType);
                            if (stageMatrix[curr.x, curr.y].tile.tileType == ScriptableObjects.Stage.Tile.MODULATOR)
                            {
                                curr.electricType = stageMatrix[curr.x, curr.y].electricType;
                            }
                            int dir = GetDirToInt(stageMatrix[curr.x, curr.y].direction);
                            nX = curr.x + dX[dir];
                            nY = curr.y + dY[dir];
                            NextTileState nextState = GetNextTileState(stageMatrix, visit, nX, nY, dir);
                            if (nextState == NextTileState.AlreadyVisited)
                            {
                                Debug.Log("전력 경로가 이미 방문한 타일과 겹칩니다: " + new Vector2Int(nX, nY));
                                return false;
                            }
                            if (nextState == NextTileState.Available)
                            {
                                stack.Push(new TileSearchNode(nX, nY, curr.remainElectric, curr.electricType));
                                visit[nX, nY] = true;
                            }
                            break;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(trackTileDelay));
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
        
        private async Task<bool> CheckAmpAndModMapAnswer()
        {
            Debug.Log("증폭기, 변환기 문제 풀이여부 확인 시작!");
            StageTile[,] stageMatrix = MakeStageMatrix();

            int numOfFactories = stageData.numOfFactories;
            int width = stageArea.width;
            int height = stageArea.height;
            
            bool[,] visit = new bool[width, height];
            int[] dX = { 0, 1, 0, -1 };
            int[] dY = { 1, 0, -1, 0 };
            if (!TryGetGeneratorPositions(stageMatrix, out List<Vector2Int> generatorPositions))
            {
                return false;
            }

            foreach (Vector2Int startPoint in generatorPositions)
            {
                if (visit[startPoint.x, startPoint.y])
                {
                    Debug.LogError("발전소 시작 위치가 중복되었습니다: " + startPoint);
                    return false;
                }

                Stack<TileSearchNode> stack = new Stack<TileSearchNode>();
                stack.Push(new TileSearchNode(
                    startPoint,
                    maxElectric,
                    stageMatrix[startPoint.x, startPoint.y].electricType));
                visit[startPoint.x, startPoint.y] = true;

                // 발전소별 DFS
                while (stack.Count > 0)
                {
                    TileSearchNode curr = stack.Pop();
                    if (curr.remainElectric < 0)
                    {
                        continue;
                    }

                    int nX, nY;
                    switch (stageMatrix[curr.x, curr.y].tile.tileType)
                    {
                        case ScriptableObjects.Stage.Tile.FACTORY:
                            if (stageMatrix[curr.x, curr.y].electricType == curr.electricType)
                            {
                                numOfFactories--;
                                SetActiveTileColor(stageMatrix[curr.x, curr.y], curr.electricType);
                            }
                            break;
                        case ScriptableObjects.Stage.Tile.GENERATOR:
                        case ScriptableObjects.Stage.Tile.DISTRIBUTOR:
                            for (int i = 0; i < 4; i++)
                            {
                                nX = curr.x + dX[i];
                                nY = curr.y + dY[i];
                                NextTileState branchState = GetNextTileState(stageMatrix, visit, nX, nY, i);
                                if (branchState == NextTileState.AlreadyVisited)
                                {
                                    Debug.Log("전력 경로가 이미 방문한 타일과 겹칩니다: " + new Vector2Int(nX, nY));
                                    return false;
                                }
                                if (branchState is NextTileState.OutOfBounds
                                    or NextTileState.Unavailable
                                    or NextTileState.InvalidDirection)
                                {
                                    continue;
                                }

                                stack.Push(new TileSearchNode(nX, nY, curr.remainElectric - 1, curr.electricType));
                                visit[nX, nY] = true;
                            }
                            SetActiveTileColor(stageMatrix[curr.x, curr.y], curr.electricType);
                            break;
                        case ScriptableObjects.Stage.Tile.OBSTACLE:
                            break;
                        default:
                            SetActiveTileColor(stageMatrix[curr.x, curr.y], curr.electricType);
                            if (stageMatrix[curr.x, curr.y].tile.tileType == ScriptableObjects.Stage.Tile.AMPLIFIER)
                            {
                                curr.remainElectric = maxElectric;
                            }
                            if (stageMatrix[curr.x, curr.y].tile.tileType == ScriptableObjects.Stage.Tile.MODULATOR)
                            {
                                curr.electricType = stageMatrix[curr.x, curr.y].electricType;
                            }
                            int dir = GetDirToInt(stageMatrix[curr.x, curr.y].direction);
                            nX = curr.x + dX[dir];
                            nY = curr.y + dY[dir];
                            NextTileState nextState = GetNextTileState(stageMatrix, visit, nX, nY, dir);
                            if (nextState == NextTileState.AlreadyVisited)
                            {
                                Debug.Log("전력 경로가 이미 방문한 타일과 겹칩니다: " + new Vector2Int(nX, nY));
                                return false;
                            }
                            if (nextState == NextTileState.Available)
                            {
                                stack.Push(new TileSearchNode(nX, nY, curr.remainElectric - 1, curr.electricType));
                                visit[nX, nY] = true;
                            }
                            break;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(trackTileDelay));
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

        #endregion

        // 정답 확인 등 다른 함수에서 사용하는 보조 함수 모음.
        #region About Utility

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

        private bool TryGetGeneratorPositions(
            StageTile[,] stageMatrix,
            out List<Vector2Int> generatorPositions)
        {
            generatorPositions = stageData.GetGeneratorPositions();
            if (generatorPositions == null || generatorPositions.Count == 0)
            {
                Debug.LogError("발전기를 찾을 수 없습니다.");
                return false;
            }

            HashSet<Vector2Int> uniquePositions = new HashSet<Vector2Int>();
            int width = stageMatrix.GetLength(0);
            int height = stageMatrix.GetLength(1);
            foreach (Vector2Int position in generatorPositions)
            {
                if (position.x < 0 || position.x >= width || position.y < 0 || position.y >= height)
                {
                    Debug.LogError("발전기 위치가 스테이지 범위를 벗어났습니다: " + position);
                    return false;
                }

                if (!uniquePositions.Add(position))
                {
                    Debug.LogError("발전기 위치가 중복되었습니다: " + position);
                    return false;
                }

                if (stageMatrix[position.x, position.y].tile.tileType != ScriptableObjects.Stage.Tile.GENERATOR)
                {
                    Debug.LogError("발전기 위치에 발전기 타일이 없습니다: " + position);
                    return false;
                }
            }

            return true;
        }

        private NextTileState GetNextTileState(
            StageTile[,] stageMatrix,
            bool[,] visit,
            int x,
            int y,
            int direction)
        {
            if (x < 0 || x >= stageMatrix.GetLength(0) || y < 0 || y >= stageMatrix.GetLength(1))
            {
                return NextTileState.OutOfBounds;
            }

            StageTile nextTile = stageMatrix[x, y];
            if (nextTile.tile.tileType is ScriptableObjects.Stage.Tile.NONE
                or ScriptableObjects.Stage.Tile.OBSTACLE
                or ScriptableObjects.Stage.Tile.GENERATOR)
            {
                return NextTileState.Unavailable;
            }

            if (!IsAvailableDirection(nextTile, direction))
            {
                return NextTileState.InvalidDirection;
            }

            return visit[x, y]
                ? NextTileState.AlreadyVisited
                : NextTileState.Available;
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

        private bool IsAvailableDirection(StageTile tile, int dir)
        {
            switch (tile.tile.tileType)
            {
                case ScriptableObjects.Stage.Tile.FACTORY:
                case ScriptableObjects.Stage.Tile.AMPLIFIER:
                case ScriptableObjects.Stage.Tile.DISTRIBUTOR:
                case ScriptableObjects.Stage.Tile.MODULATOR:
                    return true;
                case ScriptableObjects.Stage.Tile.LINE:
                    return GetDirToInt(tile.direction) == dir;
                case ScriptableObjects.Stage.Tile.CORNER_RIGHT:
                    return (GetDirToInt(tile.direction) + 1) % 4 == dir;
                case ScriptableObjects.Stage.Tile.CORNER_LEFT:
                    return (GetDirToInt(tile.direction) + 3) % 4 == dir;
                default:
                    Debug.LogError("Unexpected tile type. tile type is " + tile.tile.tileType);
                    break;
            }

            return false;
        }

        private enum NextTileState
        {
            OutOfBounds,
            Unavailable,
            InvalidDirection,
            Available,
            AlreadyVisited
        }
#endregion
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
