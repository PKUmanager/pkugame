using SpaceFusion.SF_Grid_Building_System.Scripts.Core;
using SpaceFusion.SF_Grid_Building_System.Scripts.Interfaces;
using UnityEngine;

namespace SpaceFusion.SF_Grid_Building_System.Scripts.PlacementStates
{
    public class RemoveState : IPlacementState
    {
        private string _guid;
        private readonly IPlacementGrid _grid;
        private readonly PreviewSystem _previewSystem;
        private readonly GridData _gridData;
        private readonly PlacementHandler _placementHandler;
        private Vector3Int _targetPos;

        public RemoveState(IPlacementGrid grid, PreviewSystem previewSystem, GridData gridData, PlacementHandler placementHandler)
        {
            _grid = grid;
            _previewSystem = previewSystem;
            _gridData = gridData;
            _placementHandler = placementHandler;
            previewSystem.StartShowingRemovePreview(_grid.CellSize);
        }

        public void EndState()
        {
            _previewSystem.StopShowingPreview();
        }

        public void OnAction(Vector3Int gridPosition)
        {
            // 只是选中这个格子，显示红框
            _targetPos = gridPosition;
            UpdateState(gridPosition);
        }

        public void OnConfirm()
        {
            // 真正的删除逻辑在这里执行
            _guid = _gridData.GetGuid(_targetPos);
            if (_guid != null)
            {
                _gridData.RemoveObjectPositions(_targetPos);
                _placementHandler.RemoveObjectPositions(_guid);
            }
        }

        public void OnCancel() { }
        public void OnRotation() { }

        public void UpdateState(Vector3Int gridPosition)
        {
            var validity = !(_gridData.IsPlaceable(gridPosition, Vector2Int.one));
            _previewSystem.UpdateRemovalPosition(_grid.CellToWorld(gridPosition), validity);
        }
    }
}