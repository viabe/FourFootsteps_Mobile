using UnityEngine;
using UnityEngine.UI; // Text용
using TMPro; // TextMeshPro용
using System.Collections;

/// <summary>
/// 텍스트를 깜박이게 만드는 스크립트
/// Text와 TextMeshPro 둘 다 지원
/// </summary>
public class BlinkingText : MonoBehaviour
{
    [Header("Blink Settings")]
    [SerializeField] private float blinkSpeed = 1f;
    [SerializeField] private float minAlpha = 0f;
    [SerializeField] private float maxAlpha = 1f;

    private TextMeshProUGUI textMeshPro;
    private Text unityText;
    private Color originalColor;
    private bool isTMP = false;

    void Awake()
    {
        // TextMeshPro 먼저 확인
        textMeshPro = GetComponent<TextMeshProUGUI>();
        
        if (textMeshPro != null)
        {
            isTMP = true;
            originalColor = textMeshPro.color;
            Debug.Log("TextMeshPro 사용 중");
        }
        else
        {
            // 일반 Text 확인
            unityText = GetComponent<Text>();
            
            if (unityText != null)
            {
                isTMP = false;
                originalColor = unityText.color;
                Debug.Log("Unity Text 사용 중");
            }
            else
            {
                Debug.LogError("Text 또는 TextMeshProUGUI 컴포넌트를 찾을 수 없습니다!");
            }
        }
    }

    void OnEnable()
    {
        if (textMeshPro != null || unityText != null)
        {
            StartCoroutine(BlinkCoroutine());
        }
    }

    void OnDisable()
    {
        StopAllCoroutines();
        
        // 텍스트를 원래 색상으로 복원
        if (isTMP && textMeshPro != null)
        {
            textMeshPro.color = originalColor;
        }
        else if (!isTMP && unityText != null)
        {
            unityText.color = originalColor;
        }
    }

    IEnumerator BlinkCoroutine()
    {
        while (true)
        {
            yield return Fade(maxAlpha, minAlpha);
            yield return Fade(minAlpha, maxAlpha);
        }
    }

    IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float elapsed = 0f;
        Color color;

        // 현재 색상 가져오기
        if (isTMP)
            color = textMeshPro.color;
        else
            color = unityText.color;

        while (elapsed < blinkSpeed)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / blinkSpeed);
            
            color.a = alpha;

            // 색상 적용
            if (isTMP)
                textMeshPro.color = color;
            else
                unityText.color = color;
            
            yield return null;
        }

        // 최종 알파값 정확히 설정
        color.a = endAlpha;
        
        if (isTMP)
            textMeshPro.color = color;
        else
            unityText.color = color;
    }
}