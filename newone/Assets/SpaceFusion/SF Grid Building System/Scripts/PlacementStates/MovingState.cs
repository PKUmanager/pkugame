using System.Collections.Generic;
using SpaceFusion.SF_Grid_Building_System.Scripts.Core;
using SpaceFusion.SF_Grid_Building_System.Scripts.Enums;
using SpaceFusion.SF_Grid_Building_System.Scripts.Interfaces;
using SpaceFusion.SF_Grid_Building_System.Scripts.Scriptables;
using SpaceFusion.SF_Grid_Building_System.Scripts.Utils;
using UnityEngine;

namespace SpaceFusion.SF_Grid_Building_System.Scripts.PlacementStates
{
    public class MovingState : IPlacementState
    {
        private readonly IPlacementGrid _grid;
        private readonly PreviewSystem _previewSystem;
        private readonly PlacedObject _placeable;
        private readonly Placeable _selectedPlaceable;
        private readonly Vector3Int _oldGridPosition;
        private readonly GridData _relevantGridData;
        private ObjectDirection _currentDirection;
        private Vector3Int _currentGridPosition;
        private Vector2Int _correctedObjectSize;

        public MovingState(PlacedObject placeable, IPlacementGrid grid, PreviewSystem previewSystem,
            Dictionary<GridDataType, GridData> gridDataMap, PlacementHandler placementHandler)
        {
            _placeable = placeable;
            _selectedPlaceable = placeable.placeable;
            _oldGridPosition = placeable.data.gridPosition;
            _currentDirection = placeable.data.direction;
            _relevantGridData = gridDataMap[_selectedPlaceable.GridType];
            _grid = grid;
            _previewSystem = previewSystem;

            // 隐藏真身，显示虚影
            _placeable.gameObject.SetActive(false);

            _correctedObjectSize = PlaceableUtils.GetOccupiedCells(_selectedPlaceable, _currentDirection, _grid.CellSize);
            _previewSystem.StartShowingPlacementPreview(_selectedPlaceable, _grid.CellSize);

            // 初始选中时，虚影在原位
            _currentGridPosition = _oldGridPosition;
            UpdateState(_currentGridPosition);
        }

        public void OnAction(Vector3Int gridPosition)
        {
            UpdateState(gridPosition);
        }

        public void OnConfirm()
        {
            if (!IsPlacementValid(_currentGridPosition)) return;

            // 移动数据：先删旧的，再加新的
            _relevantGridData.Move(_oldGridPosition, _currentGridPosition, _correctedObjectSize);

            // 移动真身
            _placeable.gameObject.SetActive(true);
            var offset = PlaceableUtils.CalculateOffset(_placeable.gameObject, _grid.CellSize);
            var worldPos = _grid.CellToWorld(_currentGridPosition);

            _placeable.transform.position = worldPos + PlaceableUtils.GetTotalOffset(offset, _currentDirection);
            _placeable.transform.rotation = Quaternion.Euler(0, PlaceableUtils.GetRotationAngle(_currentDirection), 0);

            // 更新物体内部数据
            _placeable.data.gridPosition = _currentGridPosition;
            _placeable.data.direction = _currentDirection;
        }

        public void OnCancel()
        {
            // 取消时，把真身重新显示出来，位置不变
            _placeable.gameObject.SetActive(true);
        }

        public void OnRotation()
        {
            _currentDirection = PlaceableUtils.GetNextDir(_currentDirection);
            _correctedObjectSize = PlaceableUtils.GetOccupiedCells(_selectedPlaceable, _currentDirection, _grid.CellSize);
            UpdateState(_currentGridPosition);
        }

        public void UpdateState(Vector3Int gridPosition)
        {
            _currentGridPosition = gridPosition;
            var isValid = IsPlacementValid(gridPosition);
            _previewSystem.UpdatePosition(_grid.CellToWorld(gridPosition), isValid, _selectedPlaceable, _currentDirection);
        }

        public void EndState()
        {
            _previewSystem.StopShowingPreview();
        }

        private bool IsPlacementValid(Vector3Int gridPosition)
        {
            return _relevantGridData.IsMoveable(_oldGridPosition, gridPosition, _correctedObjectSize) &&
                   _grid.IsWithinBounds(gridPosition, _correctedObjectSize);
        }
    }
}