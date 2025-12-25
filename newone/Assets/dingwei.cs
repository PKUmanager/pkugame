using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Android;
using System.Collections;

public class GPSUnlocker : MonoBehaviour
{
    // ============ 地块 A 配置 ============
    [Header("地块A - 范围与题目")]
    public double plotA_MinLat; // 下边界
    public double plotA_MaxLat; // 上边界
    public double plotA_MinLon; // 左边界
    public double plotA_MaxLon; // 右边界
    [TextArea] public string questionA_Text; // A的题目文字
    public Sprite questionA_Image;           // A的题目图片 【新增】
    public string answerA_Correct;           // A的答案

    // ============ 地块 B 配置 ============
    [Header("地块B - 范围与题目")]
    public double plotB_MinLat;
    public double plotB_MaxLat;
    public double plotB_MinLon;
    public double plotB_MaxLon;
    [TextArea] public string questionB_Text;
    public Sprite questionB_Image;           // B的题目图片 【新增】
    public string answerB_Correct;

    // ============ 全局设置 ============
    [Header("绑定场景对象")]
    public GameObject plotObject_A;
    public GameObject plotObject_B;

    [Header("UI绑定")]
    public Text statusText;         // 调试信息
    public Text successMessageText; // 解锁成功的大字

    [Header("问答界面绑定")]
    public GameObject quizPanel;    // 面板
    public Text quizQuestionText;   // 面板里的文字题
    public Image quizQuestionImage; // 面板里的图片位 【新增】
    public InputField quizInput;    // 输入框
    public Button quizSubmitBtn;    // 提交按钮
    public Text quizFeedbackText; // 【新增】专门显示答错了的红字

    // 内部变量
    private bool isRunning = false;
    private GameObject currentPlot;    // 当前正在处理哪个地块
    private string currentAnswer;      // 当前题目的正确答案

    // 缓冲距离：0.0001度 约等于 10-11米
    private const double BufferDist = 0.0001;

    void Start()
    {
        // 初始化UI
        if (quizPanel) quizPanel.SetActive(false);
        if (successMessageText) successMessageText.text = "";
        if (quizSubmitBtn) quizSubmitBtn.onClick.AddListener(OnSubmitAnswer);

        StartCoroutine(StartLocationService());
    }

    void Update()
    {
        if (!isRunning) return;

        // 1. 获取实时位置
        double curLat = Input.location.lastData.latitude;
        double curLon = Input.location.lastData.longitude;
        float accuracy = Input.location.lastData.horizontalAccuracy;

        // 2. 【新增】计算地块 A 和 B 的“中心点”坐标
        // 原理：(最小+最大)/2 就是中间的位置
        double centerLat_A = (plotA_MinLat + plotA_MaxLat) / 2;
        double centerLon_A = (plotA_MinLon + plotA_MaxLon) / 2;

        double centerLat_B = (plotB_MinLat + plotB_MaxLat) / 2;
        double centerLon_B = (plotB_MinLon + plotB_MaxLon) / 2;

        // 3. 【新增】计算你距离这两个中心点有多远
        float distToA = CalculateDistance(curLat, curLon, centerLat_A, centerLon_A);
        float distToB = CalculateDistance(curLat, curLon, centerLat_B, centerLon_B);

        // 4. 【修改】更新调试文字，显示所有信息
        if (statusText)
        {
            statusText.text = $"精度: {accuracy}米\n" +
                              $"当前坐标: {curLat:F6}, {curLon:F6}\n" +
                              $"--------------------\n" +
                              $"距 A 中心: {distToA:F1} 米\n" +
                              $"距 B 中心: {distToB:F1} 米";
        }

        // --- 核心逻辑检测 (保持不变) ---
        CheckPlotStatus(curLat, curLon,
            plotA_MinLat, plotA_MaxLat, plotA_MinLon, plotA_MaxLon,
            plotObject_A, "地块A",
            questionA_Text, questionA_Image, answerA_Correct);

        CheckPlotStatus(curLat, curLon,
            plotB_MinLat, plotB_MaxLat, plotB_MinLon, plotB_MaxLon,
            plotObject_B, "地块B",
            questionB_Text, questionB_Image, answerB_Correct);
    }

    // 通用的检测逻辑函数
    void CheckPlotStatus(double lat, double lon,
                         double minLat, double maxLat, double minLon, double maxLon,
                         GameObject plotObj, string plotName,
                         string qText, Sprite qImg, string correctAns)
    {
        var renderer = plotObj.GetComponent<Renderer>();

        // 1. 如果已经绿了(解锁了)，就完全不管它了，跳过
        if (renderer.material.color == Color.green) return;

        // 2. 判定是否在“内圈”（地块内部）
        bool insideInner = IsInsideRect(lat, lon, minLat, maxLat, minLon, maxLon, 0);

        // 3. 判定是否在“外圈”（地块 + 10米缓冲）
        bool insideOuter = IsInsideRect(lat, lon, minLat, maxLat, minLon, maxLon, BufferDist);

        if (insideInner)
        {
            // === 状态：进入地块内部 ===
            // 只有当面板没打开时，才触发（防止每一帧都重置题目）
            if (!quizPanel.activeSelf)
            {
                OpenQuiz(plotObj, qText, qImg, correctAns);
            }
        }
        else if (insideOuter)
        {
            // === 状态：在附近10米内 ===
            // 变红，提示周边有地块
            if (renderer.material.color != Color.red)
            {
                renderer.material.color = Color.red;
            }
        }
        else
        {
            // === 状态：离得很远 ===
            // 恢复成白色/灰色 (或者你设定的默认色)
            if (renderer.material.color != Color.white && renderer.material.color != Color.red)
            {
                // 注意：这里为了不覆盖掉从红色变回来的逻辑，只有非红非白才变
                // 简单处理：只要不在范围内且没解锁，就变白
                renderer.material.color = Color.white;
            }
            // 修正：更简单的逻辑是，只要没解锁且不在红区，就是白
            if (!insideInner && !insideOuter) renderer.material.color = Color.white;
        }
    }

    // 打开问答面板
    void OpenQuiz(GameObject plotObj, string text, Sprite img, string ans)
    {
        currentPlot = plotObj;
        currentAnswer = ans;

        quizPanel.SetActive(true);
        quizQuestionText.text = text;
        quizQuestionImage.sprite = img; // 【设置图片】
        // 如果没有图片，就隐藏图片框，防止显示白方块
        quizQuestionImage.gameObject.SetActive(img != null);
        if (quizFeedbackText) quizFeedbackText.text = "";
        quizInput.text = ""; // 清空输入框
        Handheld.Vibrate();  // 震动提示
    }

    // 提交答案
    void OnSubmitAnswer()
    {
        // 建议加上 Trim() 去除玩家不小心输进去的空格
        if (quizInput.text.Trim() == currentAnswer)
        {
            UnlockSuccess();
        }
        else
        {
            // 答错了：写在专用的反馈文本上
            if (quizFeedbackText)
            {
                quizFeedbackText.text = "答案错误，请仔细观察！";
            }

            // 为了体验好，可以不完全清空输入框，或者让输入框抖动一下（进阶）
            // 这里我们只清空输入框
            quizInput.text = "";
        }
    }

    // 解锁成功
    void UnlockSuccess()
    {
        // 变绿
        currentPlot.GetComponent<Renderer>().material.color = Color.green;

        // 关闭问答
        quizPanel.SetActive(false);

        // 显示大字
        if (successMessageText)
        {
            successMessageText.text = "地块解锁成功，\n开始建造吧！";
            StartCoroutine(ClearTextAfterDelay(4f));
        }
    }

    // 矩形判定算法 (padding是扩充范围，单位度)
    bool IsInsideRect(double curLat, double curLon, double minLat, double maxLat, double minLon, double maxLon, double padding)
    {
        return (curLat >= minLat - padding) && (curLat <= maxLat + padding) &&
               (curLon >= minLon - padding) && (curLon <= maxLon + padding);
    }

    IEnumerator ClearTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (successMessageText) successMessageText.text = "";
    }

    // GPS启动协程 (保持不变，省略以节省篇幅，请保留之前的StartLocationService代码)
    IEnumerator StartLocationService()
    {
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);
            yield return new WaitForSeconds(1);
        }
        Input.location.Start(5f, 5f);
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0) { yield return new WaitForSeconds(1); maxWait--; }
        if (maxWait < 1 || Input.location.status == LocationServiceStatus.Failed) yield break;
        isRunning = true;
    }

    // 计算两点之间距离的数学公式 (结果单位：米)
    float CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        var R = 6371e3; // 地球半径
        var rad = Mathf.Deg2Rad;
        var dLat = (lat2 - lat1) * rad;
        var dLon = (lon2 - lon1) * rad;
        var a = Mathf.Sin((float)dLat / 2) * Mathf.Sin((float)dLat / 2) +
                Mathf.Cos((float)(lat1 * rad)) * Mathf.Cos((float)(lat2 * rad)) *
                Mathf.Sin((float)dLon / 2) * Mathf.Sin((float)dLon / 2);
        var c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1 - a));
        return (float)(R * c);
    }
}