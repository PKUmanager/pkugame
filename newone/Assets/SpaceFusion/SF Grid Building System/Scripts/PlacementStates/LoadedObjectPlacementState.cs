using System;
using System.Collections.Generic;
using SpaceFusion.SF_Grid_Building_System.Scripts.Core;
using SpaceFusion.SF_Grid_Building_System.Scripts.Enums;
using SpaceFusion.SF_Grid_Building_System.Scripts.Interfaces;
using SpaceFusion.SF_Grid_Building_System.Scripts.SaveSystem;
using SpaceFusion.SF_Grid_Building_System.Scripts.Scriptables;
using SpaceFusion.SF_Grid_Building_System.Scripts.Utils;
using UnityEngine;

namespace SpaceFusion.SF_Grid_Building_System.Scripts.PlacementStates
{
    /// <summary>
    /// State handler for placements of loading saved objects
    /// </summary>
    public class LoadedObjectPlacementState : IPlacementState
    {
        private readonly IPlacementGrid _grid;
        private readonly PlacementHandler _placementHandler;

        private readonly Placeable _selectedObject;
        private readonly GridData _selectedGridData;
        private readonly PlaceableObjectData _podata;
        private readonly Vector2Int _occupiedCells;

        public LoadedObjectPlacementState(PlaceableObjectData podata, IPlacementGrid grid,
            PlaceableObjectDatabase database,
            Dictionary<GridDataType, GridData> gridDataMap, PlacementHandler placementHandler)
        {
            _grid = grid;
            _placementHandler = placementHandler;
            _podata = podata;

            _selectedObject = database.GetPlaceable(podata.assetIdentifier);
            if (!_selectedObject)
            {
                throw new Exception($"No placeable with identifier '{podata.assetIdentifier}' found");
            }

            _selectedGridData = gridDataMap[_selectedObject.GridType];
            _occupiedCells = PlaceableUtils.GetOccupiedCells(_selectedObject, podata.direction, _grid.CellSize);
        }

        // 加载模式下，不需要点击交互
        public void OnAction(Vector3Int gridPosition) { }

        // [核心逻辑] 直接在这里执行放置
        public void OnConfirm()
        {
            // 注意：这里的位置来源于存档数据(_podata)，而不是鼠标点击
            var worldPos = _grid.CellToWorld(_podata.gridPosition);

            var guid = _placementHandler.PlaceLoadedObject(
                _selectedObject,
                worldPos,
                _podata,
                _grid.CellSize
            );

            _selectedGridData.Add(_podata.gridPosition, _occupiedCells, _selectedObject.GetAssetIdentifier(), guid);
        }

        // 空实现接口方法
        public void OnCancel() { }
        public void OnRotation() { }
        public void UpdateState(Vector3Int gridPosition) { }
        public void EndState() { }
    }
}