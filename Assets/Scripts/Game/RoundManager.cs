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

            foreach (var cell in cellsToShow)
            {
                displayCells[cell.position].sprite = cell.sprite;
                displayCells[cell.position].enabled = true;
                displayCells[cell.position].color = Color.white;
            }

            yield return new WaitForSeconds(showDuration);

            foreach (var cell in cellsToShow)
            {
                displayCells[cell.position].enabled = false;
                displayCells[cell.position].color = new Color(1, 1, 1, 0);
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
        StartCoroutine(FlashInputButton(index, correctColor));
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
        StartCoroutine(FlashInputButton(index, wrongColor));
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

        if (isPerfect)
        {
            if (litCircleSprite != null)
            {
                roundIndicators[roundIndex].sprite = litCircleSprite;
            }
            roundIndicators[roundIndex].color = Color.green;
        }
        else
        {
            if (unlitCircleSprite != null)
            {
                roundIndicators[roundIndex].sprite = unlitCircleSprite;
            }
            roundIndicators[roundIndex].color = Color.red;
        }
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

            toastCanvasGroup.alpha = Mathf.Lerp(0f, 1f, progress);

            if (toastRectTransform != null)
            {
                toastRectTransform.localScale = Vector3.Lerp(startScale, endScale, progress);
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

            toastCanvasGroup.alpha = Mathf.Lerp(1f, 0f, progress);

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
        foreach (var cell in displayCells)
        {
            cell.enabled = false;
            cell.sprite = null;
            cell.color = new Color(1, 1, 1, 0);
        }
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
}