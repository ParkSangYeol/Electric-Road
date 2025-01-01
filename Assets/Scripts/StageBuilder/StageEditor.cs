using System;
using System.Collections;
using System.Collections.Generic;
using ScriptableObjects.Stage;
using Sirenix.OdinInspector;
using Stage;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Tile = ScriptableObjects.Stage.Tile;

namespace StageBuilder
{
    public class StageEditor : MonoBehaviour
    {
        [SerializeField]
        private TMP_InputField widthInputField;
        [SerializeField]
        private TMP_InputField heightInputField;

        [SerializeField, SceneObjectsOnly] 
        private GridLayoutGroup stageArea;
        private List<GameObject> tilePlates;
        
        [SerializeField, AssetsOnly] 
        private List<GameObject> tilePrefabs;
        
        [SerializeField, AssetsOnly] 
        private TileScriptableObject defaultTile;

        [SerializeField] 
        private Transform selectGuide;
        [SerializeField] 
        private Transform modulatorGuide;
        
        private List<GameObject> tiles; 

        private int width;
        private int height;

        private void Awake()
        {
            tiles = new List<GameObject>();
            tilePlates = new List<GameObject>();
        }

        public void CreateBlankStageInScene()
        {
            if (widthInputField.text == "" || heightInputField.text == "")
            {
                Debug.LogError("가로나 세로 길이가 없습니다.");
                return;
            }
            ClearStage();
            
            width = int.Parse(widthInputField.text);
            height = int.Parse(heightInputField.text);
            TileStruct[,] tileMat = new TileStruct[width, height];
            
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    tileMat[i, j] = new TileStruct(Direction.UP, defaultTile);
                }
            }

            CreateStageInScene(tileMat);
        }

        public void CreateStageInScene(TileStruct[,] tileMatrix, bool editableAll = false)
        {
            ClearStage();
            StageArea areaComponent = stageArea.GetComponent<StageArea>();
            areaComponent.width = stageArea.constraintCount = tileMatrix.GetLength(0);
            areaComponent.height = tileMatrix.GetLength(1);
            GameObject tilePrefab = null;
            switch (areaComponent.height)
            {
                case 6:
                    tilePrefab = tilePrefabs[0];
                    selectGuide.localScale = modulatorGuide.localScale = new Vector3(1.5f, 1.5f, 1.5f);
                    break;
                case 7:
                    tilePrefab = tilePrefabs[1];
                    selectGuide.localScale = modulatorGuide.localScale = new Vector3(1.25f, 1.25f, 1.25f);
                    break;
                case 8:
                    tilePrefab = tilePrefabs[2];
                    break;
                default:
                    tilePrefab = tilePrefabs[3];
                    selectGuide.localScale = modulatorGuide.localScale = new Vector3(1.1f, 1.1f, 1.1f);
                    break;
            }
            
            stageArea.cellSize = tilePrefab.GetComponent<RectTransform>().sizeDelta;
            
            for (int y = 0; y < tileMatrix.GetLength(1); y++)
            {
                for (int x = 0; x < tileMatrix.GetLength(0); x++)
                {
                    GameObject instantiateObject = Instantiate(tilePrefab, stageArea.transform);
                    StageTile tileComponent = instantiateObject.AddComponent<StageTile>();
                    tileComponent.tile = tileMatrix[x, y].tile;
                    tileComponent.direction = tileMatrix[x, y].dir;
                    tileComponent.defaultTile = editableAll? defaultTile : tileComponent.tile;
                    tileComponent.electricType = tileMatrix[x, y].electricType;
                    if (tileComponent.tile.tileType == Tile.NONE)
                    {
                        tileComponent.isEditAble = true;
                    }
                    else
                    {
                        tileComponent.isEditAble = editableAll;
                    }
                    
                    tiles.Add(instantiateObject);
                }
            }
        }
        
        public void ClearStage()
        {
            int tileNum = tiles.Count;
            for (int i = 0; i < tileNum; i++)
            {
                GameObject removingObject = tiles[0];
                tiles.Remove(removingObject);
                Destroy(removingObject);
            }
        }
    }
}