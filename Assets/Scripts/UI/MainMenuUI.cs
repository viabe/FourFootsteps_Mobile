using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 메인 메뉴 UI 관리
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [Header("Buttons")]
    public Button startButton;

    void Start()
    {
        startButton.onClick.AddListener(OnStartButtonClick);
    }

    void OnStartButtonClick()
    {
        SceneTransitionManager.Instance.LoadGameGuide();
    }
}