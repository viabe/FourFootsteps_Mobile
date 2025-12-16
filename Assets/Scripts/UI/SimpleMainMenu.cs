using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

/// <summary>
/// 메인 메뉴 화면
/// 화면 아무 곳이나 터치하면 게임 가이드로 이동
/// </summary>
public class SimpleMainMenu : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string nextSceneName = "GameGuideScene";

    private bool isTransitioning = false;

    void Start()
    {
        // ⭐ 메인 화면 진입 시 GameManager 리셋
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetGameData();
            Debug.Log("메인 화면 진입 - GameManager 리셋 완료!");
        }
    }

    void Update()
    {
        // 마우스 클릭 (PC 테스트용)
        if (!isTransitioning && Input.GetMouseButtonDown(0))
        {
            if (!IsPointerOverUIElement())
            {
                OnScreenClick();
            }
        }

        // 터치 입력 (모바일)
        if (!isTransitioning && Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            
            if (touch.phase == TouchPhase.Began)
            {
                if (!IsPointerOverUIElement())
                {
                    OnScreenClick();
                }
            }
        }
    }

    void OnScreenClick()
    {
        isTransitioning = true;
        Debug.Log("화면 터치! 게임 가이드로 이동!");
        LoadNextScene();
    }

    void LoadNextScene()
    {
        Debug.Log($"씬 로드: {nextSceneName}");
        SceneManager.LoadScene(nextSceneName);
    }

    bool IsPointerOverUIElement()
    {
        if (EventSystem.current == null)
            return false;

        if (Input.GetMouseButtonDown(0))
        {
            return EventSystem.current.IsPointerOverGameObject();
        }

        if (Input.touchCount > 0)
        {
            return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
        }

        return false;
    }
}