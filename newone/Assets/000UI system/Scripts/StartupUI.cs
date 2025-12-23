using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class StartupUI : MonoBehaviour
{
    // 声明变量（对应UI元素）
    public Slider loadingSlider;
    public TextMeshProUGUI loadingText;
    public Button startButton;

    void Start()
    {
        // 初始隐藏开始按钮
        startButton.gameObject.SetActive(false);

        // 开始加载
        StartCoroutine(LoadingProcess());
    }

    IEnumerator LoadingProcess()
    {
        // 等待1秒显示Logo
        yield return new WaitForSeconds(1f);

        // 模拟加载3秒
        float time = 0;
        float totalTime = 3f;

        while (time < totalTime)
        {
            time += Time.deltaTime;
            float progress = time / totalTime;

            // 更新进度条
            loadingSlider.value = progress;
            loadingText.text = $"加载中 {Mathf.RoundToInt(progress * 100)}%";

            yield return null; // 等待下一帧
        }

        // 加载完成
        loadingSlider.value = 1;
        loadingText.text = "加载完成！";

        // 显示开始按钮
        startButton.gameObject.SetActive(true);

        // 添加按钮点击事件
        startButton.onClick.AddListener(OnStartGame);
    }

    void OnStartGame()
    {
        // 加载主游戏场景
        SceneManager.LoadScene("MainGameScene");
    }
}