using UnityEngine;

/// <summary>
/// 노치/홈바를 고려한 Safe Area 적용 스크립트
/// Canvas 바로 아래 Panel에 부착
/// </summary>
public class SafeArea : MonoBehaviour
{
    private RectTransform rectTransform;
    private Rect safeArea;
    private Vector2 minAnchor;
    private Vector2 maxAnchor;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        ApplySafeArea();
    }

    void ApplySafeArea()
    {
        safeArea = Screen.safeArea;

        // Safe Area를 앵커로 변환
        minAnchor = safeArea.position;
        maxAnchor = minAnchor + safeArea.size;

        minAnchor.x /= Screen.width;
        minAnchor.y /= Screen.height;
        maxAnchor.x /= Screen.width;
        maxAnchor.y /= Screen.height;

        rectTransform.anchorMin = minAnchor;
        rectTransform.anchorMax = maxAnchor;

        Debug.Log($"Safe Area Applied: {safeArea}");
    }

    // 에디터에서 테스트용
    void Update()
    {
        #if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.S))
        {
            ApplySafeArea();
        }
        #endif
    }
}