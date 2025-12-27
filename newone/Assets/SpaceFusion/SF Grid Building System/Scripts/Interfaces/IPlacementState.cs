using UnityEngine;

namespace SpaceFusion.SF_Grid_Building_System.Scripts.Interfaces
{
    public interface IPlacementState
    {

        /// <summary>
        /// 退出状态时的清理工作
        /// </summary>
        void EndState();

        /// <summary>
        /// 点击屏幕时触发（现在只用于移动虚影，不要在这里放置！）
        /// </summary>
        void OnAction(Vector3Int gridPosition);

        /// <summary>
        /// [新增] 点击 UI 的“确认”按钮时触发（真正保存数据/放置物体）
        /// </summary>
        void OnConfirm();

        /// <summary>
        /// [新增] 点击 UI 的“取消”按钮时触发
        /// </summary>
        void OnCancel();

        /// <summary>
        /// 更新状态（每帧调用，用于刷新虚影位置）
        /// </summary>
        void UpdateState(Vector3Int gridPosition);

        /// <summary>
        /// 点击 UI 的“旋转”按钮时触发
        /// </summary>
        void OnRotation();
    }
}