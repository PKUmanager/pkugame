using System;
using System.Collections.Generic;
using SpaceFusion.SF_Grid_Building_System.Scripts.Core;
using SpaceFusion.SF_Grid_Building_System.Scripts.Enums;
using SpaceFusion.SF_Grid_Building_System.Scripts.Interfaces;
using UnityEngine;

namespace SpaceFusion.SF_Grid_Building_System.Scripts.PlacementStates
{
    /// <summary>
    /// RemoveAllState removes everything on the selected position, independent of the gridData
    /// </summary>
    public class RemoveAllState : IPlacementState
    {
        private readonly IPlacementGrid _grid;
        private readonly PreviewSystem _previewSystem;
        private readonly Dictionary<GridDataType, GridData> _gridDataMap;
        private readonly PlacementHandler _placementHandler;

        // [新增] 记录当前选中的位置
        private Vector3Int _targetPos;

        public RemoveAllState(IPlacementGrid grid, PreviewSystem previewSystem,
            Dictionary<GridDataType, GridData> gridDataMap,
            PlacementHandler placementHandler)
        {
            _grid = grid;
            _previewSystem = previewSystem;
            _gridDataMap = gridDataMap;
            _placementHandler = placementHandler;
            previewSystem.StartShowingRemovePreview(_grid.CellSize);
        }

        public void EndState()
        {
            _previewSystem.StopShowingPreview();
        }

        // 1. 点击屏幕：只选中位置，更新预览
        public void OnAction(Vector3Int gridPosition)
        {
            _targetPos = gridPosition;
            UpdateState(gridPosition);
        }

        // 2. 点击确认：执行“全部删除”逻辑
        public void OnConfirm()
        {
            var hasDeletedSomething = false;

            // 遍历所有网格层级 (例如：建筑层、地形层)
            foreach (GridDataType gridType in Enum.GetValues(typeof(GridDataType)))
            {
                var data = _gridDataMap[gridType];

                // 检查该层在该位置是否有东西 (IsPlaceable返回true代表空，false代表有东西)
                if (data.IsPlaceable(_targetPos, Vector2Int.one))
                {
                    continue; // 这一层是空的，跳过
                }

                // 获取并移除
                var guid = data.GetGuid(_targetPos);
                if (guid != null)
                {
                    data.RemoveObjectPositions(_targetPos);
                    _placementHandler.RemoveObjectPositions(guid);
                    hasDeletedSomething = true;
                }
            }

            if (!hasDeletedSomething)
            {
                Debug.LogWarning($"Remove All: Nothing to remove on grid position: {_targetPos}");
            }
        }

        // 3. 点击取消：什么都不做
        public void OnCancel() { }

        public void OnRotation()
        {
            // 删除模式不需要旋转
        }

        public void UpdateState(Vector3Int gridPosition)
        {
            // 只要这里有任何东西，显示就有效(红色)；如果全是空的，可能是无效(透明/灰色)
            var isValid = !IsPositionEmpty(gridPosition);
            _previewSystem.UpdatePosition(_grid.CellToWorld(gridPosition), isValid, null);
        }

        private bool IsPositionEmpty(Vector3Int gridPosition)
        {
            foreach (GridDataType gridType in Enum.GetValues(typeof(GridDataType)))
            {
                var data = _gridDataMap[gridType];
                if (!data.IsPlaceable(gridPosition, Vector2Int.one))
                {
                    return false; // 只要有一层不空，就返回 false
                }
            }
            return true;
        }
    }
}