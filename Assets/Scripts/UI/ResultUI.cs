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

        // ⭐ 성공한 개수만큼 순서대로 불빛 표시
        for (int i = 0; i < roundIndicators.Length; i++)
        {
            if (i < perfectRounds)
            {
                // 성공 - 왼쪽부터 순서대로 불빛
                roundIndicators[i].sprite = litCircleSprite;
                roundIndicators[i].color = Color.green;
            }
            else
            {
                // 실패 - 회색
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