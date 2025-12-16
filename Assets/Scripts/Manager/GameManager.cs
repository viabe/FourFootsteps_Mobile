using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 게임 전체를 관리하는 싱글톤 매니저
/// 씬 전환 시에도 유지되며 배경음악을 계속 재생
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Data")]
    public int[] roundClearStatus = new int[3]; // 0: 미완료, 1: 완료
    public int currentRound = 0; // 현재 라운드 (0~2)
    public int totalPerfectRounds = 0; // 완벽 클리어한 라운드 수

    [Header("Audio")]
    public AudioClip backgroundMusic; // 배경음악
    [Range(0f, 1f)] public float musicVolume = 0.5f; // 배경음악 볼륨

    private AudioSource musicAudioSource;

    void Awake()
    {
        // 싱글톤 패턴
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시에도 유지
            InitializeAudio();
        }
        else
        {
            Destroy(gameObject); // 중복 방지
            return;
        }
    }

    void Start()
    {
        PlayBackgroundMusic();
    }

    /// <summary>
    /// 오디오 초기화
    /// </summary>
    void InitializeAudio()
    {
        musicAudioSource = gameObject.AddComponent<AudioSource>();
        musicAudioSource.playOnAwake = false;
        musicAudioSource.loop = true; // 반복 재생
        musicAudioSource.volume = musicVolume;
    }

    /// <summary>
    /// 배경음악 재생
    /// </summary>
    void PlayBackgroundMusic()
    {
        if (backgroundMusic != null && musicAudioSource != null)
        {
            if (!musicAudioSource.isPlaying) // 이미 재생 중이면 다시 시작 안 함
            {
                musicAudioSource.clip = backgroundMusic;
                musicAudioSource.Play();
                Debug.Log("배경음악 재생 시작");
            }
        }
    }

    /// <summary>
    /// 게임 데이터 초기화
    /// </summary>
    public void ResetGameData()
    {
        currentRound = 0;
        totalPerfectRounds = 0;
        
        for (int i = 0; i < roundClearStatus.Length; i++)
        {
            roundClearStatus[i] = 0;
        }
        
        Debug.Log("게임 데이터 초기화 완료!");
    }

    /// <summary>
    /// 라운드 완료 기록
    /// </summary>
    public void RecordRoundComplete(int roundIndex, bool isPerfect)
    {
        if (roundIndex >= 0 && roundIndex < roundClearStatus.Length)
        {
            roundClearStatus[roundIndex] = isPerfect ? 1 : 0;
            
            // ⭐ 삭제: totalPerfectRounds는 ResetGameData에서만 관리
            // if (isPerfect)
            // {
            //     totalPerfectRounds++;
            // }
            
            Debug.Log($"라운드 {roundIndex + 1} 기록: {(isPerfect ? "완벽" : "실패")}");
        }
    }

    /// <summary>
    /// 게임 결과 계산 (완벽 클리어한 라운드 수)
    /// </summary>
    public int GetPerfectRoundCount()
    {
        int count = 0;
        foreach (int status in roundClearStatus)
        {
            if (status == 1) count++;
        }
        return count;
    }

    /// <summary>
    /// 배경음악 볼륨 조절
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicAudioSource != null)
        {
            musicAudioSource.volume = musicVolume;
        }
    }

    /// <summary>
    /// 배경음악 일시정지
    /// </summary>
    public void PauseMusic()
    {
        if (musicAudioSource != null && musicAudioSource.isPlaying)
        {
            musicAudioSource.Pause();
        }
    }

    /// <summary>
    /// 배경음악 재개
    /// </summary>
    public void ResumeMusic()
    {
        if (musicAudioSource != null && !musicAudioSource.isPlaying)
        {
            musicAudioSource.Play();
        }
    }

    /// <summary>
    /// 배경음악 정지
    /// </summary>
    public void StopMusic()
    {
        if (musicAudioSource != null)
        {
            musicAudioSource.Stop();
        }
    }

    /// <summary>
    /// 배경음악 토글 (재생/일시정지)
    /// </summary>
    public void ToggleMusic()
    {
        if (musicAudioSource != null)
        {
            if (musicAudioSource.isPlaying)
            {
                PauseMusic();
            }
            else
            {
                ResumeMusic();
            }
        }
    }
}