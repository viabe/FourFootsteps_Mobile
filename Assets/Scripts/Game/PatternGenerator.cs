using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 패턴(정답 + 페이크) 생성 담당
/// </summary>
public class PatternGenerator : MonoBehaviour
{
    public const int GRID_SIZE = 9; // 3x3 그리드

    [Header("Round Settings")]
    [Tooltip("각 라운드별 정답 개수 [1라운드, 2라운드, 3라운드]")]
    public int[] answerCountsPerRound = { 4, 5, 6 }; // 라운드별 정답 개수

    [Header("Fake Settings")]
    [Tooltip("각 라운드별 페이크 개수 [1라운드, 2라운드, 3라운드]")]
    public int[] fakeCountsPerRound = { 1, 2, 2 }; // 라운드별 페이크 개수

    /// <summary>
    /// 현재 라운드 (0~2)
    /// </summary>
    private int currentRound = 0;

    /// <summary>
    /// 패턴 데이터 클래스
    /// </summary>
    public class PatternData
    {
        public List<int> answerPositions; // 정답 위치들 (0~8)
        public List<FakeData> fakePositions; // 페이크 위치들
        public int answerCount; // 이번 패턴의 정답 개수
    }

    /// <summary>
    /// 페이크 데이터 클래스
    /// </summary>
    public class FakeData
    {
        public int position; // 표시 위치 (0~8)
        public FakeType type; // 페이크 타입
        public int insertAfterIndex; // 몇 번째 정답 뒤에 표시할지
    }

    public enum FakeType
    {
        RedCat,      // 빨간 고양이
        GreenDog     // 초록 강아지
    }

    /// <summary>
    /// 라운드 설정
    /// </summary>
    public void SetRound(int round)
    {
        currentRound = Mathf.Clamp(round, 0, 2);
    }

    /// <summary>
    /// 새로운 패턴 생성
    /// </summary>
    public PatternData GeneratePattern()
    {
        PatternData pattern = new PatternData();
        pattern.answerPositions = new List<int>();
        pattern.fakePositions = new List<FakeData>();

        // 현재 라운드에 맞는 정답 개수 가져오기
        int answerCount = answerCountsPerRound[currentRound];
        pattern.answerCount = answerCount;

        HashSet<int> usedPositions = new HashSet<int>();

        // 1. 정답 위치 생성 (라운드별 개수)
        for (int i = 0; i < answerCount; i++)
        {
            int randomPos = GetRandomUnusedPosition(usedPositions);
            pattern.answerPositions.Add(randomPos);
            usedPositions.Add(randomPos);
        }

        // 2. 페이크 개수 결정 (라운드별)
        int fakeCount = fakeCountsPerRound[currentRound];

        // 3. 페이크 생성
        HashSet<FakeType> usedFakeTypes = new HashSet<FakeType>(); // 사용된 페이크 타입 추적

        for (int i = 0; i < fakeCount; i++)
        {
            FakeData fake = new FakeData();
            fake.position = GetRandomUnusedPosition(usedPositions);

            // 페이크 타입 결정 (라운드별 규칙 적용)
            if (currentRound < 2) // 1, 2 라운드: 빨간 고양이와 초록 강아지 동시에 나오지 않게
            {
                // 첫 번째 페이크면 랜덤 선택, 이후에는 같은 타입만 사용
                if (usedFakeTypes.Count == 0)
                {
                    fake.type = (FakeType)Random.Range(0, 2);
                    usedFakeTypes.Add(fake.type);
                }
                else
                {
                    // 이미 사용된 타입만 사용 (동시에 나오지 않게)
                    // HashSet에서 첫 번째 요소 가져오기
                    foreach (FakeType type in usedFakeTypes)
                    {
                        fake.type = type;
                        break;
                    }
                }
            }
            else // 3라운드 이상: 동시에 나와도 되지만 같은 타입 중복은 안됨
            {
                // 사용 가능한 타입 중에서 선택
                List<FakeType> availableTypes = new List<FakeType>();
                if (!usedFakeTypes.Contains(FakeType.RedCat))
                    availableTypes.Add(FakeType.RedCat);
                if (!usedFakeTypes.Contains(FakeType.GreenDog))
                    availableTypes.Add(FakeType.GreenDog);

                // 사용 가능한 타입이 있으면 랜덤 선택, 없으면 (이미 둘 다 사용됨) 랜덤 선택
                if (availableTypes.Count > 0)
                {
                    fake.type = availableTypes[Random.Range(0, availableTypes.Count)];
                    usedFakeTypes.Add(fake.type);
                }
                else
                {
                    // 둘 다 이미 사용된 경우 (이론적으로는 발생하지 않아야 함)
                    fake.type = (FakeType)Random.Range(0, 2);
                }
            }

            fake.insertAfterIndex = Random.Range(0, answerCount); // 정답 개수에 맞춰 조정

            pattern.fakePositions.Add(fake);
            usedPositions.Add(fake.position);
        }

        Debug.Log($"패턴 생성 - 라운드: {currentRound + 1}, 정답 개수: {answerCount}, 페이크 개수: {fakeCount}");

        return pattern;
    }

    /// <summary>
    /// 사용되지 않은 랜덤 위치 반환
    /// </summary>
    private int GetRandomUnusedPosition(HashSet<int> usedPositions)
    {
        int randomPos;
        do
        {
            randomPos = Random.Range(0, GRID_SIZE);
        }
        while (usedPositions.Contains(randomPos));

        return randomPos;
    }
}