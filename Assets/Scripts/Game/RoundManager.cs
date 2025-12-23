using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// 게임 플레이 씬 관리
/// 위: 패턴 표시 (DisplayCells)
/// 아래: 입력 버튼 (InputButtons)
/// </summary>
public class RoundManager : MonoBehaviour
{
    public static RoundManager Instance { get; private set; }

    [Header("Display Area (위쪽 검은 패널)")]
    public Image[] displayCells;

    [Header("Input Area (아래쪽 버튼)")]
    public Button[] inputButtons;

    [Header("Round Indicators")]
    public Image[] roundIndicators;

    [Header("Toast Message")]
    public GameObject toastMessage;
    public Text toastText;
    public Image toastBackground;

    [Header("Sprites")]
    public Sprite greenCatSprite;
    public Sprite redCatSprite;
    public Sprite greenDogSprite;
    public Sprite litCircleSprite;
    public Sprite unlitCircleSprite;

    [Header("Timing")]
    public float startDelay = 3f;
    public float showDuration = 0.6f;
    public float delayBetween = 0.2f;
    public float feedbackDuration = 0.3f;
    public float roundCompleteDuration = 1.0f;
    public float toastDuration = 1.5f;
    
    [Header("Animation")]
    public float cellAppearDuration = 0.3f;
    public float cellDisappearDuration = 0.2f;
    public float cellAppearScale = 1.2f;
    public float buttonClickScale = 0.9f;
    public float buttonClickDuration = 0.2f;
    public float buttonRotateDuration = 0.5f;
    public float buttonShakeIntensity = 10f;
    public float buttonShakeDuration = 0.3f;
    public float indicatorUpdateDuration = 0.4f;
    public float indicatorUpdateScale = 1.3f;

    [Header("Colors")]
    public Color defaultColor = Color.white;
    public Color correctColor = Color.green;
    public Color wrongColor = Color.red;

    [Header("Audio - 효과음만")]
    public AudioClip toastSound;
    public AudioClip correctSound;
    public AudioClip wrongSound;
    public AudioClip roundCompleteSound;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    private PatternGenerator patternGenerator;
    private int currentRound = 0;
    private PatternGenerator.PatternData currentPattern;
    private int playerAnswerIndex = 0;
    private bool hasWrongAnswer = false;
    private bool isProcessingInput = false;
    private bool[] roundPerfectStatus = new bool[3];

    private AudioSource sfxAudioSource;

    private CanvasGroup toastCanvasGroup;
    private RectTransform toastRectTransform;
    private int originalFontSize;

    private Coroutine currentToastCoroutine;
    private RectTransform[] displayCellRectTransforms;
    private RectTransform[] inputButtonRectTransforms;
    private RectTransform[] roundIndicatorRectTransforms;

    public enum ToastType
    {
        Info,
        Success,
        Warning,
        Error
    }

    void Awake()
    {
        Instance = this;
        patternGenerator = gameObject.AddComponent<PatternGenerator>();

        sfxAudioSource = gameObject.AddComponent<AudioSource>();
        sfxAudioSource.playOnAwake = false;
        sfxAudioSource.volume = sfxVolume;

        if (toastMessage != null)
        {
            toastCanvasGroup = toastMessage.GetComponent<CanvasGroup>();
            if (toastCanvasGroup == null)
            {
                toastCanvasGroup = toastMessage.AddComponent<CanvasGroup>();
            }

            toastRectTransform = toastMessage.GetComponent<RectTransform>();
        }

        if (toastText != null)
        {
            originalFontSize = toastText.fontSize;
        }

        // DisplayCell RectTransform 캐싱
        displayCellRectTransforms = new RectTransform[displayCells.Length];
        for (int i = 0; i < displayCells.Length; i++)
        {
            if (displayCells[i] != null)
            {
                displayCellRectTransforms[i] = displayCells[i].GetComponent<RectTransform>();
                if (displayCellRectTransforms[i] == null)
                {
                    displayCellRectTransforms[i] = displayCells[i].gameObject.AddComponent<RectTransform>();
                }
            }
        }

        // InputButton RectTransform 캐싱
        inputButtonRectTransforms = new RectTransform[inputButtons.Length];
        for (int i = 0; i < inputButtons.Length; i++)
        {
            if (inputButtons[i] != null)
            {
                inputButtonRectTransforms[i] = inputButtons[i].GetComponent<RectTransform>();
                if (inputButtonRectTransforms[i] == null)
                {
                    inputButtonRectTransforms[i] = inputButtons[i].gameObject.AddComponent<RectTransform>();
                }
            }
        }

        // RoundIndicator RectTransform 캐싱
        roundIndicatorRectTransforms = new RectTransform[roundIndicators.Length];
        for (int i = 0; i < roundIndicators.Length; i++)
        {
            if (roundIndicators[i] != null)
            {
                roundIndicatorRectTransforms[i] = roundIndicators[i].GetComponent<RectTransform>();
                if (roundIndicatorRectTransforms[i] == null)
                {
                    roundIndicatorRectTransforms[i] = roundIndicators[i].gameObject.AddComponent<RectTransform>();
                }
            }
        }
    }

    void Start()
    {
        for (int i = 0; i < inputButtons.Length; i++)
        {
            int index = i;
            inputButtons[i].onClick.AddListener(() => OnCellClick(index));
        }

        InitializeRoundIndicators();
        HideAllDisplayCells();
        StartCoroutine(StartGameWithDelay());
    }

    void HideAllDisplayCells()
    {
        foreach (var cell in displayCells)
        {
            cell.enabled = false;
            cell.color = new Color(1, 1, 1, 0);
        }
    }

    IEnumerator StartGameWithDelay()
    {
        // ⭐ 토스트 배경 미리 활성화
        if (toastMessage != null)
        {
            toastMessage.SetActive(true);
            
            if (toastBackground != null)
            {
                toastBackground.color = new Color(1f, 1f, 1f, 0.9f);
            }
            
            if (toastCanvasGroup != null)
            {
                toastCanvasGroup.alpha = 1f;
            }
        }

        // 카운트다운 (3, 2, 1) - 배경은 유지, 텍스트만 변경
        for (int i = 3; i >= 1; i--)
        {
            if (toastMessage != null && toastText != null)
            {
                ShowToastTextOnly($"{i}");
                yield return new WaitForSeconds(1.2f);
            }
        }

        // "게임 시작!" - 배경은 유지, 텍스트만 변경
        if (toastMessage != null && toastText != null)
        {
            ShowToastTextOnly("게임 시작!");
            yield return new WaitForSeconds(1.2f);
        }

        // ⭐ 모든 카운트다운 끝난 후 토스트 숨김
        if (toastMessage != null)
        {
            toastMessage.SetActive(false);
        }

        StartGame();
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null && sfxAudioSource != null)
        {
            sfxAudioSource.PlayOneShot(clip, sfxVolume);
        }
    }

    void InitializeRoundIndicators()
    {
        for (int i = 0; i < roundIndicators.Length; i++)
        {
            if (unlitCircleSprite != null)
            {
                roundIndicators[i].sprite = unlitCircleSprite;
            }
            roundIndicators[i].color = Color.gray;
        }
    }

    void StartGame()
    {
        currentRound = 0;
        for (int i = 0; i < 3; i++)
        {
            roundPerfectStatus[i] = false;
        }
        StartRound();
    }

    void StartRound()
    {
        playerAnswerIndex = 0;
        hasWrongAnswer = false;
        isProcessingInput = false;

        patternGenerator.SetRound(currentRound);
        currentPattern = patternGenerator.GeneratePattern();

        ClearDisplayCells();
        ClearInputButtons();
        SetInputButtonsInteractable(false);

        StartCoroutine(ShowPatternSequence());
    }

    IEnumerator ShowPatternSequence()
    {
        for (int answerIdx = 0; answerIdx < currentPattern.answerPositions.Count; answerIdx++)
        {
            int answerPos = currentPattern.answerPositions[answerIdx];
            
            List<CellToShow> cellsToShow = new List<CellToShow>();
            
            cellsToShow.Add(new CellToShow
            {
                position = answerPos,
                sprite = greenCatSprite
            });

            foreach (var fake in currentPattern.fakePositions)
            {
                if (fake.insertAfterIndex == answerIdx)
                {
                    Sprite fakeSprite = fake.type == PatternGenerator.FakeType.RedCat 
                        ? redCatSprite : greenDogSprite;

                    cellsToShow.Add(new CellToShow
                    {
                        position = fake.position,
                        sprite = fakeSprite
                    });
                }
            }

            // 셀 등장 애니메이션
            foreach (var cell in cellsToShow)
            {
                displayCells[cell.position].sprite = cell.sprite;
                displayCells[cell.position].enabled = true;
                StartCoroutine(AnimateCellAppear(cell.position));
            }

            yield return new WaitForSeconds(showDuration);

            // 셀 사라짐 애니메이션
            foreach (var cell in cellsToShow)
            {
                StartCoroutine(AnimateCellDisappear(cell.position));
            }

            yield return new WaitForSeconds(delayBetween);
        }

        SetInputButtonsInteractable(true);
    }

    void OnCellClick(int clickedIndex)
    {
        if (isProcessingInput) return;

        int correctIndex = currentPattern.answerPositions[playerAnswerIndex];

        if (clickedIndex == correctIndex)
        {
            isProcessingInput = true;
            StartCoroutine(HandleCorrectAnswer(clickedIndex));
        }
        else
        {
            isProcessingInput = true;
            hasWrongAnswer = true;
            StartCoroutine(HandleWrongAnswer(clickedIndex));
        }
    }

    IEnumerator HandleCorrectAnswer(int index)
    {
        PlaySound(correctSound);
        StartCoroutine(AnimateCorrectButton(index));
        playerAnswerIndex++;

        yield return new WaitForSeconds(feedbackDuration);

        if (playerAnswerIndex >= currentPattern.answerCount)
        {
            yield return OnRoundComplete();
        }
        else
        {
            isProcessingInput = false;
        }
    }

    IEnumerator HandleWrongAnswer(int index)
    {
        PlaySound(wrongSound);
        StartCoroutine(AnimateWrongButton(index));
        playerAnswerIndex++;

        yield return new WaitForSeconds(feedbackDuration);

        if (playerAnswerIndex >= currentPattern.answerCount)
        {
            yield return OnRoundComplete();
        }
        else
        {
            isProcessingInput = false;
        }
    }

    IEnumerator OnRoundComplete()
    {
        SetInputButtonsInteractable(false);

        roundPerfectStatus[currentRound] = !hasWrongAnswer;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.RecordRoundComplete(currentRound, !hasWrongAnswer);
        }

        UpdateRoundIndicator(currentRound, !hasWrongAnswer);

        PlaySound(roundCompleteSound);

        yield return new WaitForSeconds(roundCompleteDuration);

        currentRound++;

        if (currentRound < 3)
        {
            if (toastMessage != null)
            {
                ShowToast($"{currentRound + 1}라운드 시작!", ToastType.Info);
                yield return new WaitForSeconds(toastDuration);
            }
            StartRound();
        }
        else
        {
            yield return new WaitForSeconds(2f);
            SceneManager.LoadScene("ResultScene");
        }
    }

    void UpdateRoundIndicator(int roundIndex, bool isPerfect)
    {
        if (roundIndex < 0 || roundIndex >= roundIndicators.Length)
            return;

        StartCoroutine(AnimateRoundIndicatorUpdate(roundIndex, isPerfect));
    }

    IEnumerator AnimateRoundIndicatorUpdate(int roundIndex, bool isPerfect)
    {
        if (roundIndex < 0 || roundIndex >= roundIndicators.Length ||
            roundIndex >= roundIndicatorRectTransforms.Length) yield break;

        Image indicator = roundIndicators[roundIndex];
        RectTransform rectTransform = roundIndicatorRectTransforms[roundIndex];
        if (indicator == null || rectTransform == null) yield break;

        Color targetColor = isPerfect ? Color.green : Color.red;
        Sprite targetSprite = isPerfect ? litCircleSprite : unlitCircleSprite;
        Color startColor = indicator.color;
        Sprite originalSprite = indicator.sprite;

        // 스프라이트 변경
        if (targetSprite != null)
        {
            indicator.sprite = targetSprite;
        }

        float elapsed = 0f;
        Vector3 originalScale = Vector3.one;
        Vector3 midScale = new Vector3(indicatorUpdateScale, indicatorUpdateScale, 1f);

        // 스케일 팝업
        while (elapsed < indicatorUpdateDuration * 0.5f)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / (indicatorUpdateDuration * 0.5f);
            rectTransform.localScale = Vector3.Lerp(originalScale, midScale, progress);
            yield return null;
        }

        // 색상 전환
        elapsed = 0f;
        while (elapsed < indicatorUpdateDuration * 0.5f)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / (indicatorUpdateDuration * 0.5f);
            indicator.color = Color.Lerp(startColor, targetColor, progress);
            yield return null;
        }

        // 스케일 복귀
        elapsed = 0f;
        while (elapsed < indicatorUpdateDuration * 0.5f)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / (indicatorUpdateDuration * 0.5f);
            float easedProgress = EaseOutBack(progress);
            rectTransform.localScale = Vector3.Lerp(midScale, originalScale, easedProgress);
            yield return null;
        }

        rectTransform.localScale = originalScale;
        indicator.color = targetColor;
    }

    void ShowToastWithTextScale(string message)
    {
        if (toastMessage == null || toastText == null) return;

        // 기존 애니메이션 중단
        if (currentToastCoroutine != null)
        {
            StopCoroutine(currentToastCoroutine);
        }

        toastText.text = message;

        if (toastBackground != null)
        {
            toastBackground.color = new Color(1f, 1f, 1f, 0.9f);
        }

        if (toastCanvasGroup != null)
        {
            toastCanvasGroup.alpha = 1f;
        }

        if (toastRectTransform != null)
        {
            toastRectTransform.localScale = Vector3.one;
        }

        toastMessage.SetActive(true);
        PlaySound(toastSound);
        
        currentToastCoroutine = StartCoroutine(TextSizeAnimation());
    }

    IEnumerator TextSizeAnimation()
    {
        float duration = 0.3f;
        float elapsed = 0f;

        int startSize = Mathf.RoundToInt(originalFontSize * 1.3f);
        int endSize = originalFontSize;

        if (toastCanvasGroup != null)
        {
            toastCanvasGroup.alpha = 1f;
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            if (toastText != null)
            {
                toastText.fontSize = Mathf.RoundToInt(Mathf.Lerp(startSize, endSize, progress));
            }

            yield return null;
        }

        if (toastText != null)
        {
            toastText.fontSize = endSize;
        }

        yield return new WaitForSeconds(0.7f);

        if (toastMessage != null)
        {
            toastMessage.SetActive(false);
        }
        
        currentToastCoroutine = null;
    }

    void ShowToast(string message, ToastType type = ToastType.Info)
    {
        if (toastMessage == null || toastText == null) return;

        toastText.text = message;

        if (toastBackground != null)
        {
            toastBackground.color = new Color(1f, 1f, 1f, 0.9f);
        }

        toastMessage.SetActive(true);
        PlaySound(toastSound);
        StartCoroutine(ToastAnimationSequence());
    }

    IEnumerator ToastAnimationSequence()
    {
        float duration = 0.3f;
        float elapsed = 0f;

        Vector3 startScale = new Vector3(0.8f, 0.8f, 1f);
        Vector3 endScale = Vector3.one;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            float easedProgress = EaseOutBack(progress);

            toastCanvasGroup.alpha = Mathf.Lerp(0f, 1f, progress);

            if (toastRectTransform != null)
            {
                toastRectTransform.localScale = Vector3.Lerp(startScale, endScale, easedProgress);
            }

            yield return null;
        }

        toastCanvasGroup.alpha = 1f;
        if (toastRectTransform != null)
        {
            toastRectTransform.localScale = endScale;
        }

        yield return new WaitForSeconds(toastDuration - 0.6f);

        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            float easedProgress = EaseInBack(progress);

            toastCanvasGroup.alpha = Mathf.Lerp(1f, 0f, easedProgress);

            yield return null;
        }

        toastCanvasGroup.alpha = 0f;

        if (toastMessage != null)
        {
            toastMessage.SetActive(false);
        }
    }

    void ClearDisplayCells()
    {
        for (int i = 0; i < displayCells.Length; i++)
        {
            if (displayCells[i] != null)
            {
                displayCells[i].enabled = false;
                displayCells[i].sprite = null;
                displayCells[i].color = new Color(1, 1, 1, 0);
                
                // 스케일 초기화
                if (displayCellRectTransforms[i] != null)
                {
                    displayCellRectTransforms[i].localScale = Vector3.one;
                }
            }
        }
    }

    IEnumerator AnimateCellAppear(int cellIndex)
    {
        if (cellIndex < 0 || cellIndex >= displayCells.Length || 
            cellIndex >= displayCellRectTransforms.Length) yield break;

        Image cell = displayCells[cellIndex];
        RectTransform rectTransform = displayCellRectTransforms[cellIndex];
        if (cell == null || rectTransform == null) yield break;

        float elapsed = 0f;
        Vector3 startScale = Vector3.zero;
        Vector3 midScale = new Vector3(cellAppearScale, cellAppearScale, 1f);
        Vector3 endScale = Vector3.one;
        Color startColor = Color.white;
        startColor.a = 0f;
        Color endColor = Color.white;

        rectTransform.localScale = startScale;
        cell.color = startColor;

        while (elapsed < cellAppearDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / cellAppearDuration;
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
            cell.color = Color.Lerp(startColor, endColor, progress);

            yield return null;
        }

        rectTransform.localScale = endScale;
        cell.color = endColor;
    }

    IEnumerator AnimateCellDisappear(int cellIndex)
    {
        if (cellIndex < 0 || cellIndex >= displayCells.Length || 
            cellIndex >= displayCellRectTransforms.Length) yield break;

        Image cell = displayCells[cellIndex];
        RectTransform rectTransform = displayCellRectTransforms[cellIndex];
        if (cell == null || rectTransform == null) yield break;

        float elapsed = 0f;
        Vector3 startScale = rectTransform.localScale;
        Vector3 endScale = Vector3.zero;
        Color startColor = cell.color;
        Color endColor = startColor;
        endColor.a = 0f;

        while (elapsed < cellDisappearDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / cellDisappearDuration;
            float easedProgress = EaseInBack(progress);

            rectTransform.localScale = Vector3.Lerp(startScale, endScale, easedProgress);
            cell.color = Color.Lerp(startColor, endColor, progress);

            yield return null;
        }

        rectTransform.localScale = endScale;
        cell.color = endColor;
        cell.enabled = false;
    }

    void ClearInputButtons()
    {
        foreach (var btn in inputButtons)
        {
            btn.GetComponent<Image>().color = defaultColor;
        }
    }

    void SetInputButtonsInteractable(bool interactable)
    {
        foreach (var btn in inputButtons)
        {
            btn.interactable = interactable;
        }
    }

    IEnumerator FlashInputButton(int index, Color flashColor)
    {
        Image btnImage = inputButtons[index].GetComponent<Image>();
        Color original = btnImage.color;
        
        btnImage.color = flashColor;
        yield return new WaitForSeconds(feedbackDuration);
        btnImage.color = original;
    }

    IEnumerator AnimateCorrectButton(int index)
    {
        if (index < 0 || index >= inputButtons.Length || 
            index >= inputButtonRectTransforms.Length) yield break;

        Image btnImage = inputButtons[index].GetComponent<Image>();
        RectTransform rectTransform = inputButtonRectTransforms[index];
        if (btnImage == null || rectTransform == null) yield break;

        Color originalColor = btnImage.color;
        Vector3 originalScale = Vector3.one;
        Vector3 originalRotation = rectTransform.localEulerAngles;

        // 색상 변경
        btnImage.color = correctColor;

        // 스케일 다운
        float elapsed = 0f;
        while (elapsed < buttonClickDuration * 0.5f)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / (buttonClickDuration * 0.5f);
            rectTransform.localScale = Vector3.Lerp(originalScale, originalScale * buttonClickScale, progress);
            yield return null;
        }

        // 회전 + 스케일 복귀
        elapsed = 0f;
        while (elapsed < buttonRotateDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / buttonRotateDuration;
            float easedProgress = EaseOutBack(progress);
            
            // 회전 (360도)
            rectTransform.localEulerAngles = new Vector3(0, 0, 360f * (1f - progress));
            
            // 스케일 복귀
            rectTransform.localScale = Vector3.Lerp(originalScale * buttonClickScale, originalScale, easedProgress);
            
            yield return null;
        }

        // 색상 복귀
        elapsed = 0f;
        while (elapsed < feedbackDuration - buttonRotateDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / (feedbackDuration - buttonRotateDuration);
            btnImage.color = Color.Lerp(correctColor, originalColor, progress);
            yield return null;
        }

        rectTransform.localScale = originalScale;
        rectTransform.localEulerAngles = originalRotation;
        btnImage.color = originalColor;
    }

    IEnumerator AnimateWrongButton(int index)
    {
        if (index < 0 || index >= inputButtons.Length || 
            index >= inputButtonRectTransforms.Length) yield break;

        Image btnImage = inputButtons[index].GetComponent<Image>();
        RectTransform rectTransform = inputButtonRectTransforms[index];
        if (btnImage == null || rectTransform == null) yield break;

        Color originalColor = btnImage.color;
        Vector3 originalScale = Vector3.one;
        Vector3 originalPosition = rectTransform.localPosition;

        // 색상 변경
        btnImage.color = wrongColor;

        // 스케일 다운
        float elapsed = 0f;
        while (elapsed < buttonClickDuration * 0.5f)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / (buttonClickDuration * 0.5f);
            rectTransform.localScale = Vector3.Lerp(originalScale, originalScale * buttonClickScale, progress);
            yield return null;
        }

        // 흔들림 효과
        elapsed = 0f;
        Vector3 startPos = originalPosition;
        while (elapsed < buttonShakeDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / buttonShakeDuration;
            float shakeAmount = buttonShakeIntensity * (1f - progress); // 점점 약해짐
            
            // 랜덤 방향으로 흔들림
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            Vector3 shakeOffset = new Vector3(
                Mathf.Cos(angle) * shakeAmount,
                Mathf.Sin(angle) * shakeAmount,
                0f
            );
            rectTransform.localPosition = startPos + shakeOffset;
            
            yield return null;
        }

        // 스케일 및 위치 복귀
        elapsed = 0f;
        while (elapsed < buttonClickDuration * 0.5f)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / (buttonClickDuration * 0.5f);
            float easedProgress = EaseOutBack(progress);
            
            rectTransform.localScale = Vector3.Lerp(originalScale * buttonClickScale, originalScale, easedProgress);
            rectTransform.localPosition = Vector3.Lerp(rectTransform.localPosition, originalPosition, progress);
            
            yield return null;
        }

        // 색상 복귀
        elapsed = 0f;
        float remainingTime = feedbackDuration - buttonShakeDuration - buttonClickDuration;
        while (elapsed < remainingTime && remainingTime > 0f)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / remainingTime;
            btnImage.color = Color.Lerp(wrongColor, originalColor, progress);
            yield return null;
        }

        rectTransform.localScale = originalScale;
        rectTransform.localPosition = originalPosition;
        rectTransform.localEulerAngles = Vector3.zero;
        btnImage.color = originalColor;
    }

    private class CellToShow
    {
        public int position;
        public Sprite sprite;
    }

    /// <summary>
    /// 텍스트만 변경 (배경은 유지)
    /// </summary>
    void ShowToastTextOnly(string message)
    {
        if (toastText == null) return;

        // 기존 애니메이션 중단
        if (currentToastCoroutine != null)
        {
            StopCoroutine(currentToastCoroutine);
        }

        toastText.text = message;
        PlaySound(toastSound);
        
        currentToastCoroutine = StartCoroutine(TextOnlyAnimation());
    }

    /// <summary>
    /// 텍스트 크기만 애니메이션 (배경/투명도 변경 없음)
    /// </summary>
    IEnumerator TextOnlyAnimation()
    {
        float duration = 0.3f;
        float elapsed = 0f;

        int startSize = Mathf.RoundToInt(originalFontSize * 1.3f);
        int endSize = originalFontSize;

        // 폰트 크기 애니메이션
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            if (toastText != null)
            {
                toastText.fontSize = Mathf.RoundToInt(Mathf.Lerp(startSize, endSize, progress));
            }

            yield return null;
        }

        if (toastText != null)
        {
            toastText.fontSize = endSize;
        }

        currentToastCoroutine = null;
    }

    // Easing 함수들
    float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    float EaseInBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return c3 * t * t * t - c1 * t * t;
    }

    float EaseOutBounce(float t)
    {
        float n1 = 7.5625f;
        float d1 = 2.75f;

        if (t < 1f / d1)
        {
            return n1 * t * t;
        }
        else if (t < 2f / d1)
        {
            return n1 * (t -= 1.5f / d1) * t + 0.75f;
        }
        else if (t < 2.5f / d1)
        {
            return n1 * (t -= 2.25f / d1) * t + 0.9375f;
        }
        else
        {
            return n1 * (t -= 2.625f / d1) * t + 0.984375f;
        }
    }
}