using UnityEngine;
using SpaceFusion.SF_Grid_Building_System.Scripts.Core;

public class UIActionController : MonoBehaviour
{
    [SerializeField] private GameObject actionButtonsGroup; // 把上面的 ActionButtons 拖进来

    private void Start()
    {
        PlacementSystem.Instance.OnSelectionChanged += ToggleButtons;
        actionButtonsGroup.SetActive(false); // 默认隐藏
    }

    private void OnDestroy()
    {
        if (PlacementSystem.Instance != null)
            PlacementSystem.Instance.OnSelectionChanged -= ToggleButtons;
    }

    private void ToggleButtons(bool show)
    {
        actionButtonsGroup.SetActive(show);
    }
}