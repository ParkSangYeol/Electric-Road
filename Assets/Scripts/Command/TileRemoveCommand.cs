using System.Collections;
using System.Collections.Generic;
using ScriptableObjects.Stage;
using Stage;
using UnityEngine;

namespace Command
{
    public class TileRemoveCommand : ICommand
    {
        // 단일 타일을 제거하는 커맨드
        private readonly StageTile targetTile;
        public readonly TileScriptableObject beforeTileData;
        private readonly Direction beforeDirection;
        private readonly int beforeElectricType;

        public TileRemoveCommand(StageTile targetTile)
        {
            this.targetTile = targetTile;
            if (targetTile == null)
            {
                Debug.LogError("커맨드의 타일값이 NULL 입니다.");
                throw new UnityException();
            }

            beforeTileData = targetTile.tile;
            beforeDirection = targetTile.direction;
            beforeElectricType = targetTile.electricType;
        }
        
        public void Execute()
        {
            targetTile.EraseTile();
        }

        public void Undo()
        {
            targetTile.tile = beforeTileData;
            targetTile.direction = beforeDirection;
            targetTile.electricType = beforeElectricType;
        }
    }
}