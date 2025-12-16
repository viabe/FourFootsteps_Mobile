using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// 씬 전환을 관리하는 매니저
/// </summary>
public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [Header("Scene Names")]
    public string mainMenuScene = "MainMenuScene";
    public string gameGuideScene = "GameGuideScene";
    public string gamePlayScene = "GamePlayScene";
    public string resultScene = "ResultScene";

    [Header("Fade Settings")]
    public CanvasGroup fadeCanvasGroup; // 페이드 효과용 (선택)
    public float fadeDuration = 0.5f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 메인 메뉴로 이동
    /// </summary>
    public void LoadMainMenu()
    {
        SceneManager.LoadScene(mainMenuScene);
    }

    /// <summary>
    /// 게임 가이드로 이동
    /// </summary>
    public void LoadGameGuide()
    {
        SceneManager.LoadScene(gameGuideScene);
    }

    /// <summary>
    /// 게임 플레이로 이동
    /// </summary>
    public void LoadGamePlay()
    {
        GameManager.Instance.ResetGameData();
        SceneManager.LoadScene(gamePlayScene);
    }

    /// <summary>
    /// 결과 화면으로 이동
    /// </summary>
    public void LoadResult()
    {
        SceneManager.LoadScene(resultScene);
    }

    /// <summary>
    /// 페이드 효과와 함께 씬 전환 (선택)
    /// </summary>
    public void LoadSceneWithFade(string sceneName)
    {
        StartCoroutine(FadeAndLoad(sceneName));
    }

    IEnumerator FadeAndLoad(string sceneName)
    {
        // 페이드 아웃
        if (fadeCanvasGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                fadeCanvasGroup.alpha = Mathf.Clamp01(elapsed / fadeDuration);
                yield return null;
            }
        }

        // 씬 로드
        SceneManager.LoadScene(sceneName);

        // 페이드 인
        if (fadeCanvasGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                fadeCanvasGroup.alpha = Mathf.Clamp01(1f - (elapsed / fadeDuration));
                yield return null;
            }
        }
    }
}