using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class ResultUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Text titleText;
    public Text scoreText;
    public Image[] roundIndicators;

    [Header("Sprites")]
    public Sprite litCircleSprite;
    public Sprite unlitCircleSprite;

    [Header("Buttons")]
    public Button mainMenuButton;

    [Header("Animation")]
    public bool useFadeIn = true;
    public float fadeInDuration = 0.5f;

    private CanvasGroup canvasGroup;

    void Awake()
    {
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
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(OnMainMenuClick);
        }

        DisplayResult();

        if (useFadeIn)
        {
            StartCoroutine(FadeIn());
        }
    }

    void DisplayResult()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager가 없습니다!");
            return;
        }

        int perfectRounds = GameManager.Instance.GetPerfectRoundCount();

        if (titleText != null)
        {
            if (perfectRounds > 0)
            {
                titleText.text = "게임 클리어!";
                titleText.color = new Color(0.3f, 0.8f, 0.3f);
            }
            else
            {
                titleText.text = "게임 오버";
                titleText.color = new Color(0.8f, 0.3f, 0.3f);
            }
        }

        if (scoreText != null)
        {
            scoreText.text = $"3개 중 {perfectRounds}개 성공!";
        }

        for (int i = 0; i < roundIndicators.Length; i++)
        {
            bool isPerfect = GameManager.Instance.roundClearStatus[i] == 1;
            
            if (isPerfect)
            {
                roundIndicators[i].sprite = litCircleSprite;
                roundIndicators[i].color = Color.green;
            }
            else
            {
                roundIndicators[i].sprite = unlitCircleSprite;
                roundIndicators[i].color = Color.gray;
            }
        }
    }

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

    void OnMainMenuClick()
    {
        Debug.Log("메인 메뉴로 이동!");
        
        // ⭐ 메인으로 가기 전 리셋은 필요 없음 (SimpleMainMenu에서 Start에 리셋하므로)
        // 하지만 안전하게 추가해도 됨
        
        SceneManager.LoadScene("MainMenuScene");
    }

    void OnDestroy()
    {
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveListener(OnMainMenuClick);
        }
    }
}