using System;
using System.Collections.Generic;
using SpaceFusion.SF_Grid_Building_System.Scripts.Enums;
using SpaceFusion.SF_Grid_Building_System.Scripts.Interfaces;
using SpaceFusion.SF_Grid_Building_System.Scripts.Managers;
using SpaceFusion.SF_Grid_Building_System.Scripts.PlacementStates;
using SpaceFusion.SF_Grid_Building_System.Scripts.SaveSystem;
using SpaceFusion.SF_Grid_Building_System.Scripts.Scriptables;
using SpaceFusion.SF_Grid_Building_System.Scripts.Utils;
using UnityEngine;

namespace SpaceFusion.SF_Grid_Building_System.Scripts.Core
{
    public class PlacementSystem : MonoBehaviour
    {
        public static PlacementSystem Instance;

        [SerializeField] private PreviewSystem previewSystem;
        [SerializeField] private PlacementHandler placementHandler;

        // [新 UI 事件] true=显示确认/取消/旋转按钮，false=隐藏
        public event Action<bool> OnSelectionChanged;

        // [修复报错关键点] 恢复这两个被删除的旧事件，供 CameraController 使用
        public event Action OnPlacementStateStart;
        public event Action OnPlacementStateEnd;

        // [状态标记] 是否处于查看/编辑模式
        public bool IsInspectionMode { get; private set; } = false;

        private readonly Dictionary<GridDataType, GridData> _gridDataMap = new();
        private IPlacementState _stateHandler;
        private InputManager _inputManager;
        private GameConfig _gameConfig;
        private PlaceableObjectDatabase _database;
        private PlacementGrid _grid;

        // 辅助变量
        private Vector3Int _lastDetectedPosition = Vector3Int.zero;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            _inputManager = InputManager.Instance;
            _gameConfig = GameConfig.Instance;
            _database = _gameConfig.PlaceableObjectDatabase;

            _inputManager.OnClicked += HandleClickAction;
            _inputManager.OnExit += StopState;
        }

        public void Initialize(PlacementGrid grid)
        {
            _grid = grid;
            InitializeGridData();
        }

        private void InitializeGridData()
        {
            foreach (GridDataType gridType in Enum.GetValues(typeof(GridDataType)))
            {
                _gridDataMap.Add(gridType, new GridData());
            }
        }

        // --- 1. 交互模式控制 ---

        public void SetInspectionMode(bool isActive)
        {
            IsInspectionMode = isActive;
            // 切换模式时，取消当前的任何操作
            CancelAction();
        }

        public void StartPlacement(string assetID)
        {
            SetInspectionMode(false); // 关闭查看模式
            StopState();

            _stateHandler = new PlacementState(assetID, _grid, previewSystem, _database, _gridDataMap, placementHandler);

            // 同时触发新旧两个事件
            OnSelectionChanged?.Invoke(true);
            OnPlacementStateStart?.Invoke(); // [修复] 通知相机：开始放置了
        }

        public void StartMoving(PlacedObject placedObject)
        {
            SetInspectionMode(false);
            StopState();

            _stateHandler = new MovingState(placedObject, _grid, previewSystem, _gridDataMap, placementHandler);

            OnSelectionChanged?.Invoke(true);
            OnPlacementStateStart?.Invoke(); // [修复] 通知相机：开始移动了
        }

        // --- 2. UI按钮绑定的方法 ---

        public void ConfirmAction()
        {
            if (_stateHandler != null)
            {
                _stateHandler.OnConfirm();
                StopState();
            }
        }

        public void CancelAction()
        {
            if (_stateHandler != null)
            {
                _stateHandler.OnCancel();
                StopState();
            }
        }

        public void RotateStructure()
        {
            _stateHandler?.OnRotation();
        }

        // --- 3. 内部逻辑 ---

        private void HandleClickAction()
        {
            if (_stateHandler == null || InputManager.IsPointerOverUIObject()) return;

            var mousePosition = _inputManager.GetSelectedMapPosition();
            var gridPosition = _grid.WorldToCell(mousePosition);

            _stateHandler.OnAction(gridPosition);
        }

        private void StopState()
        {
            if (_stateHandler == null) return;
            _stateHandler.EndState();
            _stateHandler = null;

            // 同时触发新旧两个事件
            OnSelectionChanged?.Invoke(false);
            OnPlacementStateEnd?.Invoke(); // [修复] 通知相机：放置/移动结束了
        }

        // --- 4. 兼容其他功能的方法 ---
        public void StartRemoving(GridDataType gridType)
        {
            StopState();
            _stateHandler = new RemoveState(_grid, previewSystem, _gridDataMap[gridType], placementHandler);

            OnSelectionChanged?.Invoke(true);
            OnPlacementStateStart?.Invoke(); // [修复]
        }

        public void StartRemovingAll()
        {
            StopState();
            _stateHandler = new RemoveAllState(_grid, previewSystem, _gridDataMap, placementHandler);

            OnSelectionChanged?.Invoke(true);
            OnPlacementStateStart?.Invoke(); // [修复]
        }

        public void InitializeLoadedObject(PlaceableObjectData podata)
        {
            var state = new LoadedObjectPlacementState(podata, _grid, _database, _gridDataMap, placementHandler);
            state.OnConfirm();
        }

        public void Remove(PlacedObject placedObject)
        {
            var gridPosition = placedObject.data.gridPosition;
            var gridData = _gridDataMap[placedObject.placeable.GridType];
            gridData.RemoveObjectPositions(gridPosition);
            placementHandler.RemoveObjectPositions(placedObject.data.guid);
        }

        private void Update()
        {
            if (_stateHandler != null)
            {
                var mousePosition = _inputManager.GetSelectedMapPosition();
                var gridPosition = _grid.WorldToCell(mousePosition);
                if (_lastDetectedPosition != gridPosition)
                {
                    _stateHandler.UpdateState(gridPosition);
                    _lastDetectedPosition = gridPosition;
                }
            }
        }
    }
}