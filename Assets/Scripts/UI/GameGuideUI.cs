using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// 간단한 게임 가이드 화면
/// </summary>
public class GameGuideUI : MonoBehaviour
{
    [Header("UI References")]
    public Button startButton;
    public Image guideImage; // 가이드 이미지 (선택사항 - 페이드 효과용)

    [Header("Animation")]
    public bool useFadeIn = true; // 페이드 인 효과 사용 여부
    public float fadeInDuration = 0.5f;

    private CanvasGroup canvasGroup;

    void Awake()
    {
        // 페이드 인을 위한 CanvasGroup
        if (useFadeIn)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            canvasGroup.alpha = 0f;
        }
    }

    void Start()
    {
        // 버튼 리스너 등록
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartButtonClick);
        }
        else
        {
            Debug.LogError("StartButton이 연결되지 않았습니다!");
        }

        // 페이드 인 효과
        if (useFadeIn)
        {
            StartCoroutine(FadeIn());
        }
    }

    /// <summary>
    /// 페이드 인 효과
    /// </summary>
    IEnumerator FadeIn()
    {
        float elapsed = 0f;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    /// <summary>
    /// 게임 시작 버튼 클릭
    /// </summary>
    void OnStartButtonClick()
    {
        Debug.Log("게임 시작!");
        
        // 게임 데이터 초기화
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetGameData();
        }
        
        // 게임 플레이 씬으로 이동
        SceneManager.LoadScene("GamePlayScene");
    }

    void OnDestroy()
    {
        if (startButton != null)
        {
            startButton.onClick.RemoveListener(OnStartButtonClick);
        }
    }
}