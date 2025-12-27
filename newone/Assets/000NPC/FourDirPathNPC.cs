using UnityEngine;
using System.Collections;

public class FourDirPathNPC : MonoBehaviour
{
    [Header("路线设置")]
    public Transform[] waypoints;   // 路径点
    public float speed = 3.0f;      // 移动速度
    public float waitTime = 2.0f;   // 到达后停留时间
    public bool loop = true;        // 是否循环走

    [Header("绑定")]
    public Animator animator;       // 子物体 Animator

    // 内部变量
    private int targetIndex = 0;
    private bool isWaiting = false;

    void Start()
    {
        if (waypoints.Length > 0)
        {
            // 开局瞬移到起点
            transform.position = waypoints[0].position;
        }
    }

    void Update()
    {
        // 0. 安全检查
        if (waypoints.Length == 0 || isWaiting) return;

        // 1. 获取目标点
        Transform target = waypoints[targetIndex];

        // 2. 计算世界方向向量 (目标位置 - 当前位置)
        // 【关键】这里定义了 direction，后面才能用
        Vector3 direction = target.position - transform.position;

        // 3. 计算距离，判断是否到达
        if (direction.magnitude < 0.1f)
        {
            // 到达了，开始休息
            StartCoroutine(WaitNextPoint());

            // 停止动画
            animator.SetBool("IsMoving", false);
        }
        else
        {
            // 正在路上
            animator.SetBool("IsMoving", true);

            // A. 物理移动 (使用 direction 计算目标)
            transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

            // ==========================================================
            // B. 动画控制 (摄像机修正版)
            // ==========================================================

            // 这里使用了 direction 变量，必须确保第2步里定义的名字也叫 direction
            Vector3 screenDir = Camera.main.transform.InverseTransformDirection(direction);

            // 归一化 (只取方向)
            Vector3 normDir = screenDir.normalized;

            // 传给 Animator
            animator.SetFloat("InputX", -normDir.x);
            animator.SetFloat("InputZ", -normDir.z);
        }
    }

    IEnumerator WaitNextPoint()
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitTime);

        // 索引+1
        targetIndex++;

        // 循环判断
        if (targetIndex >= waypoints.Length)
        {
            if (loop) targetIndex = 0;
            else targetIndex = waypoints.Length - 1; // 停在终点
        }

        isWaiting = false;
    }
}