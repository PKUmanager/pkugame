using System.Collections.Generic;
using SpaceFusion.SF_Grid_Building_System.Scripts.Core;
using SpaceFusion.SF_Grid_Building_System.Scripts.Enums;
using SpaceFusion.SF_Grid_Building_System.Scripts.Interfaces;
using SpaceFusion.SF_Grid_Building_System.Scripts.Scriptables;
using SpaceFusion.SF_Grid_Building_System.Scripts.Utils;
using UnityEngine;

namespace SpaceFusion.SF_Grid_Building_System.Scripts.PlacementStates
{
    public class PlacementState : IPlacementState
    {
        private readonly IPlacementGrid _grid;
        private readonly PreviewSystem _previewSystem;
        private readonly PlacementHandler _placementHandler;
        private readonly GridData _selectedGridData;
        private readonly Placeable _selectedObject;
        private ObjectDirection _currentDirection = ObjectDirection.Down;
        private Vector3Int _currentGridPosition;
        private Vector2Int _occupiedCells;

        public PlacementState(string assetIdentifier, IPlacementGrid grid, PreviewSystem previewSystem,
            PlaceableObjectDatabase database, Dictionary<GridDataType, GridData> gridDataMap, PlacementHandler placementHandler)
        {
            _grid = grid;
            _previewSystem = previewSystem;
            _placementHandler = placementHandler;
            _selectedObject = database.GetPlaceable(assetIdentifier);
            _selectedGridData = gridDataMap[_selectedObject.GridType];

            _occupiedCells = PlaceableUtils.GetOccupiedCells(_selectedObject, _currentDirection, _grid.CellSize);
            _previewSystem.StartShowingPlacementPreview(_selectedObject, _grid.CellSize);
        }

        // 1. 点击屏幕：只更新位置
        public void OnAction(Vector3Int gridPosition)
        {
            UpdateState(gridPosition);
        }

        // 2. 点击确认：真正放置
        public void OnConfirm()
        {
            if (!IsPlacementValid(_currentGridPosition))
            {
                Debug.Log("位置无效，无法放置");
                return;
            }

            var offset = PlaceableUtils.CalculateOffset(_selectedObject.Prefab, _grid.CellSize);
            var worldPosition = _grid.CellToWorld(_currentGridPosition);

            var guid = _placementHandler.PlaceObject(_selectedObject, worldPosition, _currentGridPosition,
                _currentDirection, offset, _grid.CellSize);

            _selectedGridData.Add(_currentGridPosition, _occupiedCells, _selectedObject.GetAssetIdentifier(), guid);
        }

        // 3. 点击取消：不做任何事，System会调用EndState清理虚影
        public void OnCancel() { }

        public void OnRotation()
        {
            _currentDirection = PlaceableUtils.GetNextDir(_currentDirection);
            _occupiedCells = PlaceableUtils.GetOccupiedCells(_selectedObject, _currentDirection, _grid.CellSize);
            UpdateState(_currentGridPosition);
        }

        public void UpdateState(Vector3Int gridPosition)
        {
            _currentGridPosition = gridPosition;
            bool isValid = IsPlacementValid(gridPosition);
            _previewSystem.UpdatePosition(_grid.CellToWorld(gridPosition), isValid, _selectedObject, _currentDirection);
        }

        public void EndState()
        {
            _previewSystem.StopShowingPreview();
        }

        private bool IsPlacementValid(Vector3Int gridPosition)
        {
            return _selectedGridData.IsPlaceable(gridPosition, _occupiedCells) && _grid.IsWithinBounds(gridPosition, _occupiedCells);
        }
    }
}