using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class StartupUI : MonoBehaviour
{
    public Slider loadingSlider;
    public TextMeshProUGUI loadingText;
    public Button startButton;

    void Start()
    {
        startButton.gameObject.SetActive(false);
        StartCoroutine(LoadingProcess());
    }

    IEnumerator LoadingProcess()
    {
        yield return new WaitForSeconds(1f);

        float time = 0;
        float totalTime = 3f;

        while (time < totalTime)
        {
            time += Time.deltaTime;
            float progress = time / totalTime;

            loadingSlider.value = progress;
            loadingText.text = $"加载中 {Mathf.RoundToInt(progress * 100)}%";

            yield return null;
        }

        loadingSlider.value = 1;
        loadingText.text = "加载完成！";

        startButton.gameObject.SetActive(true);
        startButton.onClick.RemoveAllListeners();
        startButton.onClick.AddListener(OnStartGame);
    }

    void OnStartGame()
    {
        SceneManager.LoadScene("SampleScene");
    }
}