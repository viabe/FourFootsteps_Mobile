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
    public float textAnimationDuration = 0.5f;
    public float textDelayBetween = 0.2f;
    public float indicatorAnimationDuration = 0.4f;
    public float indicatorDelayBetween = 0.15f;
    public float pulseDuration = 1.0f;
    public float pulseScale = 1.15f;

    private CanvasGroup canvasGroup;
    private RectTransform canvasRectTransform;
    private RectTransform[] indicatorRectTransforms;
    private RectTransform titleRectTransform;
    private RectTransform scoreRectTransform;

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

        canvasRectTransform = GetComponent<RectTransform>();
        if (canvasRectTransform == null)
        {
            canvasRectTransform = gameObject.AddComponent<RectTransform>();
        }

        // RectTransform 캐싱
        if (titleText != null)
        {
            titleRectTransform = titleText.GetComponent<RectTransform>();
            if (titleRectTransform == null)
            {
                titleRectTransform = titleText.gameObject.AddComponent<RectTransform>();
            }
        }

        if (scoreText != null)
        {
            scoreRectTransform = scoreText.GetComponent<RectTransform>();
            if (scoreRectTransform == null)
            {
                scoreRectTransform = scoreText.gameObject.AddComponent<RectTransform>();
            }
        }

        indicatorRectTransforms = new RectTransform[roundIndicators.Length];
        for (int i = 0; i < roundIndicators.Length; i++)
        {
            if (roundIndicators[i] != null)
            {
                indicatorRectTransforms[i] = roundIndicators[i].GetComponent<RectTransform>();
                if (indicatorRectTransforms[i] == null)
                {
                    indicatorRectTransforms[i] = roundIndicators[i].gameObject.AddComponent<RectTransform>();
                }
            }
        }
    }

    void Start()
    {
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(OnMainMenuClick);
            // 버튼 인터랙션 효과 추가
            AddButtonInteractionEffects(mainMenuButton);
        }

        DisplayResult();

        if (useFadeIn)
        {
            StartCoroutine(AnimateEntrance());
        }
        else
        {
            StartCoroutine(AnimateTextsAndIndicators());
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
            // 초기 상태 설정 (애니메이션을 위해)
            if (titleRectTransform != null)
            {
                titleRectTransform.localScale = Vector3.zero;
            }
            Color titleColor = titleText.color;
            titleColor.a = 0f;
            titleText.color = titleColor;
        }

        if (scoreText != null)
        {
            scoreText.text = $"3개 중 {perfectRounds}개 성공!";
            // 초기 상태 설정
            if (scoreRectTransform != null)
            {
                scoreRectTransform.localScale = Vector3.zero;
            }
            Color scoreColor = scoreText.color;
            scoreColor.a = 0f;
            scoreText.color = scoreColor;
        }

        // ⭐ 인디케이터 초기 상태 설정 (애니메이션으로 표시됨)
        for (int i = 0; i < roundIndicators.Length; i++)
        {
            if (i < perfectRounds)
            {
                // 성공 - 스프라이트와 색상만 설정, 표시는 애니메이션으로
                roundIndicators[i].sprite = litCircleSprite;
                roundIndicators[i].color = Color.green;
            }
            else
            {
                // 실패 - 회색
                roundIndicators[i].sprite = unlitCircleSprite;
                roundIndicators[i].color = Color.gray;
            }
            
            // 초기 상태 설정
            if (indicatorRectTransforms[i] != null)
            {
                indicatorRectTransforms[i].localScale = Vector3.zero;
            }
            Color indicatorColor = roundIndicators[i].color;
            indicatorColor.a = 0f;
            roundIndicators[i].color = indicatorColor;
        }
    }

    IEnumerator AnimateEntrance()
    {
        // 전체 UI 스케일 + 페이드인
        float elapsed = 0f;
        Vector3 startScale = new Vector3(0.8f, 0.8f, 1f);
        Vector3 endScale = Vector3.one;

        if (canvasRectTransform != null)
        {
            canvasRectTransform.localScale = startScale;
        }

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / fadeInDuration;
            float easedProgress = EaseOutBack(progress);

            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, progress);
            }

            if (canvasRectTransform != null)
            {
                canvasRectTransform.localScale = Vector3.Lerp(startScale, endScale, easedProgress);
            }

            yield return null;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }
        if (canvasRectTransform != null)
        {
            canvasRectTransform.localScale = endScale;
        }

        // 텍스트와 인디케이터 애니메이션 시작
        yield return StartCoroutine(AnimateTextsAndIndicators());
    }

    IEnumerator AnimateTextsAndIndicators()
    {
        // Title 텍스트 애니메이션
        if (titleText != null && titleRectTransform != null)
        {
            yield return StartCoroutine(AnimateTextPopIn(titleText, titleRectTransform));
            yield return new WaitForSeconds(textDelayBetween);
        }

        // Score 텍스트 애니메이션
        if (scoreText != null && scoreRectTransform != null)
        {
            yield return StartCoroutine(AnimateTextPopIn(scoreText, scoreRectTransform));
            yield return new WaitForSeconds(textDelayBetween);
        }

        // Round Indicators 순차 애니메이션
        if (GameManager.Instance != null)
        {
            int perfectRounds = GameManager.Instance.GetPerfectRoundCount();
            for (int i = 0; i < perfectRounds && i < roundIndicators.Length; i++)
            {
                if (roundIndicators[i] != null && indicatorRectTransforms[i] != null)
                {
                    yield return StartCoroutine(AnimateIndicatorPopIn(i));
                    yield return new WaitForSeconds(indicatorDelayBetween);
                }
            }

            // 성공한 인디케이터에 펄스 효과 추가
            for (int i = 0; i < perfectRounds && i < roundIndicators.Length; i++)
            {
                if (roundIndicators[i] != null && indicatorRectTransforms[i] != null)
                {
                    StartCoroutine(AnimateIndicatorPulse(i));
                }
            }
        }
    }

    IEnumerator AnimateTextPopIn(Text text, RectTransform rectTransform)
    {
        float elapsed = 0f;
        Vector3 startScale = Vector3.zero;
        Vector3 midScale = new Vector3(1.2f, 1.2f, 1f);
        Vector3 endScale = Vector3.one;
        Color startColor = text.color;
        Color endColor = text.color;
        startColor.a = 0f;
        endColor.a = 1f;

        while (elapsed < textAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / textAnimationDuration;
            float easedProgress = EaseOutBack(progress);

            // 스케일 애니메이션 (0 -> 1.2 -> 1.0)
            if (progress < 0.6f)
            {
                float scaleProgress = progress / 0.6f;
                rectTransform.localScale = Vector3.Lerp(startScale, midScale, scaleProgress);
            }
            else
            {
                float scaleProgress = (progress - 0.6f) / 0.4f;
                rectTransform.localScale = Vector3.Lerp(midScale, endScale, scaleProgress);
            }

            // 알파 애니메이션
            text.color = Color.Lerp(startColor, endColor, progress);

            yield return null;
        }

        rectTransform.localScale = endScale;
        text.color = endColor;
    }

    IEnumerator AnimateIndicatorPopIn(int index)
    {
        Image indicator = roundIndicators[index];
        RectTransform rectTransform = indicatorRectTransforms[index];

        float elapsed = 0f;
        Vector3 startScale = new Vector3(0.5f, 0.5f, 1f);
        Vector3 midScale = new Vector3(1.2f, 1.2f, 1f);
        Vector3 endScale = Vector3.one;
        Color startColor = indicator.color;
        Color endColor = indicator.color;
        startColor.a = 0f;
        endColor.a = 1f;

        while (elapsed < indicatorAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / indicatorAnimationDuration;
            float easedProgress = EaseOutBack(progress);

            // 스케일 애니메이션 (0.5 -> 1.2 -> 1.0)
            if (progress < 0.6f)
            {
                float scaleProgress = progress / 0.6f;
                rectTransform.localScale = Vector3.Lerp(startScale, midScale, scaleProgress);
            }
            else
            {
                float scaleProgress = (progress - 0.6f) / 0.4f;
                rectTransform.localScale = Vector3.Lerp(midScale, endScale, scaleProgress);
            }

            // 알파 애니메이션
            indicator.color = Color.Lerp(startColor, endColor, progress);

            yield return null;
        }

        rectTransform.localScale = endScale;
        indicator.color = endColor;
    }

    IEnumerator AnimateIndicatorPulse(int index)
    {
        Image indicator = roundIndicators[index];
        RectTransform rectTransform = indicatorRectTransforms[index];
        Vector3 baseScale = Vector3.one;

        while (true)
        {
            float elapsed = 0f;
            while (elapsed < pulseDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / pulseDuration;
                float scale = 1f + (pulseScale - 1f) * Mathf.Sin(progress * Mathf.PI);
                rectTransform.localScale = baseScale * scale;
                yield return null;
            }
        }
    }

    void AddButtonInteractionEffects(Button button)
    {
        if (button == null) return;

        RectTransform buttonRect = button.GetComponent<RectTransform>();
        if (buttonRect == null) return;

        // 클릭 시 스케일 효과
        button.onClick.AddListener(() =>
        {
            StartCoroutine(ButtonClickAnimation(buttonRect));
        });
    }

    IEnumerator ButtonClickAnimation(RectTransform rectTransform)
    {
        float duration = 0.2f;
        float elapsed = 0f;
        Vector3 originalScale = Vector3.one;
        Vector3 pressedScale = new Vector3(0.9f, 0.9f, 1f);

        // 눌림
        while (elapsed < duration * 0.5f)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / (duration * 0.5f);
            rectTransform.localScale = Vector3.Lerp(originalScale, pressedScale, progress);
            yield return null;
        }

        // 복귀
        elapsed = 0f;
        while (elapsed < duration * 0.5f)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / (duration * 0.5f);
            float easedProgress = EaseOutBack(progress);
            rectTransform.localScale = Vector3.Lerp(pressedScale, originalScale, easedProgress);
            yield return null;
        }

        rectTransform.localScale = originalScale;
    }

    // Easing 함수들
    float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
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