using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 게임 플레이 화면 UI 관리
/// </summary>
public class GamePlayUI : MonoBehaviour
{
    [Header("Toast Message")]
    public GameObject toastMessageObject;
    public Text toastText;
    public float toastDuration = 1.5f;

    /// <summary>
    /// 라운드 시작 토스트 표시
    /// </summary>
    public void ShowRoundStartToast(int roundNumber)
    {
        StartCoroutine(ShowToastCoroutine($"{roundNumber}라운드 시작!"));
    }

    IEnumerator ShowToastCoroutine(string message)
    {
        toastText.text = message;
        toastMessageObject.SetActive(true);

        yield return new WaitForSeconds(toastDuration);

        toastMessageObject.SetActive(false);
    }
}