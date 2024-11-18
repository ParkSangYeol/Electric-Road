using System.Collections;
using System.Collections.Generic;
using ScriptableObjects.Stage;
using Stage;
using UnityEngine;

namespace Command
{
    public class TilePlaceCommand : ICommand
    {
        // 단일 타일을 배치하는 커맨드
        private readonly StageTile targetTile;
        private readonly TileScriptableObject targetTileData;
        private readonly Direction targetDirection;
        private readonly int targetElectricType;
        private readonly TileScriptableObject beforeTileData;
        private readonly Direction beforeDirection;
        private readonly int beforeElectricType;

        public TilePlaceCommand(StageTile targetTile, TileScriptableObject targetData, 
            Direction targetDirection, int targetElectricType)
        {
            this.targetTile = targetTile;
            if (targetTile == null)
            {
                Debug.LogError("커맨드의 타일값이 NULL 입니다.");
                throw new UnityException();
            }
            this.targetTileData = targetData;
            this.targetDirection = targetDirection;
            this.targetElectricType = targetElectricType;

            beforeTileData = targetTile.tile ;
            beforeDirection = targetTile.direction;
            beforeElectricType = targetTile.electricType;
        }
        
        public void Execute()
        {
            targetTile.tile = targetTileData;
            targetTile.direction = targetDirection;
            targetTile.electricType = targetElectricType;
        }

        public void Undo()
        {
            targetTile.tile = beforeTileData;
            targetTile.direction = beforeDirection;
            targetTile.electricType = beforeElectricType;
        }
    }
    
}