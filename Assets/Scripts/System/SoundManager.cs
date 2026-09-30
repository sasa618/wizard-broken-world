using Unity.VisualScripting;
using UnityEngine;

public enum SEType
{
    Jump,
    RepairComplete,
    EnemyRepair,
    Coin,
    Portal,
    Goal,
    PlayerDamage,
    RepairableBreak,
    UIButton,
    EnemyAttack,
    FinalResult,
    SpringBounce
}

public enum BGM
{
    None = 0,
    Title = 1,
    Stage1 = 2,
    Boss1 = 3,
    Stage2 = 4,
    Clear = 5,
}

public class SoundManager : MonoBehaviour
{
    private const string BGMVolumePrefsKey = "BGMVolume";
    private const string SEVolumePrefsKey = "SEVolume";

    public static SoundManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource seSource;
    [SerializeField] private AudioSource loopSource;
    [SerializeField] private AudioSource playerMoveLoopSource;
    [SerializeField] private AudioSource repairProgressLoopSource;
    [SerializeField] private AudioSource bgmSource;

    [Header("SE Clips")]
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip repairCompleteSound;
    [SerializeField] private AudioClip enemyRepairSound;
    [SerializeField] private AudioClip coinSound;
    [SerializeField] private AudioClip portalSound;
    [SerializeField] private AudioClip goalSound;
    [SerializeField] private AudioClip playerDamageSound;
    [SerializeField] private AudioClip repairableBreakSound;
    [SerializeField] private AudioClip UIButtonSound;
    [SerializeField] private AudioClip pauseOpenSound;
    [SerializeField] private AudioClip enemyAttackSound;
    [SerializeField] private AudioClip finalResultSound;
    [SerializeField] private AudioClip springBounceSound;

    [Header("Loop Clips")]
    [SerializeField] private AudioClip repairBeamLoopSound;
    [SerializeField] private AudioClip playerMoveLoopSound;
    [SerializeField] private AudioClip repairProgressLoopSound;

    [Header("BGM Clips")]
    [SerializeField] private AudioClip titleBGM;
    [SerializeField] private AudioClip stageBGM;
    [SerializeField] private AudioClip stage2BGM;
    [SerializeField] private AudioClip bossBGM;
    [SerializeField] private AudioClip clearBGM;

    [Header("Volumes")]
    [SerializeField] private float seVolume = 1f;
    [SerializeField] private float repairBeamLoopVolume = 1f;
    [SerializeField] private float playerMoveLoopVolume = 1f;
    [SerializeField] private float repairProgressLoopVolume = 1f;
    [SerializeField] private float bgmVolume = 1f;

    private BGM _currentBGM = BGM.None;

    public float BGMVolume => bgmVolume;
    public float SEVolume => seVolume;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        seSource = EnsureAudioSource(seSource, "SESource", false);
        loopSource = EnsureAudioSource(loopSource, "LoopSource", true);
        playerMoveLoopSource = EnsureAudioSource(playerMoveLoopSource, "PlayerMoveLoopSource", true);
        repairProgressLoopSource = EnsureAudioSource(repairProgressLoopSource, "RepairProgressLoopSource", true);
        bgmSource = EnsureAudioSource(bgmSource, "BGMSource", true);

        seSource.playOnAwake = false;
        loopSource.playOnAwake = false;
        playerMoveLoopSource.playOnAwake = false;
        repairProgressLoopSource.playOnAwake = false;
        bgmSource.playOnAwake = false;

        LoadVolumeSettings();
    }

    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);

        if (bgmSource != null)
        {
            bgmSource.volume = bgmVolume;
        }

        PlayerPrefs.SetFloat(BGMVolumePrefsKey, bgmVolume);
        PlayerPrefs.Save();
    }

    public void SetSEVolume(float volume)
    {
        seVolume = Mathf.Clamp01(volume);
        ApplyLoopVolumes();

        PlayerPrefs.SetFloat(SEVolumePrefsKey, seVolume);
        PlayerPrefs.Save();
    }

    public void PlaySE(SEType type)
    {
        AudioClip clip = GetSEClip(type);

        if (clip == null || seSource == null)
        {
            return;
        }

        seSource.PlayOneShot(clip, seVolume);
    }

    public void PlayUIButtonSE()
    {
        PlaySE(SEType.UIButton);
    }

    public void PlayPauseOpenSE()
    {
        PlayOneShot(pauseOpenSound);
    }

    public void StartRepairBeam()
    {
        PlayLoop(loopSource, repairBeamLoopSound, GetSELoopVolume(repairBeamLoopVolume));
    }

    public void StopRepairBeam()
    {
        StopLoop(loopSource, repairBeamLoopSound);
    }

    public void StartPlayerMove()
    {
        PlayLoop(playerMoveLoopSource, playerMoveLoopSound, GetSELoopVolume(playerMoveLoopVolume));
    }

    public void StopPlayerMove()
    {
        StopLoop(playerMoveLoopSource, playerMoveLoopSound);
    }

    public void StartRepairProgress()
    {
        PlayLoop(repairProgressLoopSource, repairProgressLoopSound, GetSELoopVolume(repairProgressLoopVolume));
    }

    public void StopRepairProgress()
    {
        StopLoop(repairProgressLoopSource, repairProgressLoopSound);
    }

    public void PlayBGM()
    {
        if (stageBGM == null || bgmSource == null)
        {
            return;
        }

        if (bgmSource.clip == stageBGM && bgmSource.isPlaying)
        {
            return;
        }

        bgmSource.clip = stageBGM;
        bgmSource.loop = true;
        bgmSource.volume = bgmVolume;
        bgmSource.Play();
    }

    public void PlayBGM(AudioClip clip, BGM bgm)
    {
        if (clip == null || bgmSource == null)
        {
            return;
        }

        if (bgmSource.clip == clip && bgmSource.isPlaying)
        {
            return;
        }

        bgmSource.Stop();

        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.volume = bgmVolume;
        bgmSource.Play();

        _currentBGM = bgm;
        Debug.Log($"<color=green>BGM Changed: {bgm}</color>");
    }

    public void SwitchBGM(BGM bgm)
    {
        if (bgmSource == null)
        {
            return;
        }

        switch (bgm)
        {
            case BGM.None:
                bgmSource.Stop();
                _currentBGM = BGM.None;
                break;
            case BGM.Title:
                PlayBGM(titleBGM, bgm);
                break;
            case BGM.Stage1:
                PlayBGM(stageBGM, bgm);
                break;
            case BGM.Stage2:
                PlayBGM(stage2BGM, bgm);
                break;
            case BGM.Boss1:
                PlayBGM(bossBGM, bgm);
                break;
            case BGM.Clear:
                PlayBGM(clearBGM, bgm);
                break;
            default:
                break;
        }
    }

    private AudioClip GetSEClip(SEType type)
    {
        switch (type)
        {
            case SEType.Jump:
                return jumpSound;
            case SEType.RepairComplete:
                return repairCompleteSound;
            case SEType.EnemyRepair:
                return enemyRepairSound;
            case SEType.Coin:
                return coinSound;
            case SEType.Portal:
                return portalSound;
            case SEType.Goal:
                return goalSound;
            case SEType.PlayerDamage:
                return playerDamageSound;
            case SEType.RepairableBreak:
                return repairableBreakSound;
            case SEType.UIButton:
                return UIButtonSound;
            case SEType.EnemyAttack:
                return enemyAttackSound;
            case SEType.FinalResult:
                return finalResultSound;
            case SEType.SpringBounce:
                return springBounceSound;
            default:
                return null;
        }
    }

    private void LoadVolumeSettings()
    {
        bgmVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(BGMVolumePrefsKey, bgmVolume));
        seVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(SEVolumePrefsKey, seVolume));

        if (bgmSource != null)
        {
            bgmSource.volume = bgmVolume;
        }

        ApplyLoopVolumes();
    }

    private void ApplyLoopVolumes()
    {
        SetLoopSourceVolume(loopSource, GetSELoopVolume(repairBeamLoopVolume));
        SetLoopSourceVolume(playerMoveLoopSource, GetSELoopVolume(playerMoveLoopVolume));
        SetLoopSourceVolume(repairProgressLoopSource, GetSELoopVolume(repairProgressLoopVolume));
    }

    private float GetSELoopVolume(float baseVolume)
    {
        return Mathf.Clamp01(baseVolume) * seVolume;
    }

    private void SetLoopSourceVolume(AudioSource source, float volume)
    {
        if (source != null)
        {
            source.volume = volume;
        }
    }

    private void PlayOneShot(AudioClip clip)
    {
        if (clip == null || seSource == null)
        {
            return;
        }

        seSource.PlayOneShot(clip, seVolume);
    }

    private void PlayLoop(AudioSource source, AudioClip clip, float volume)
    {
        if (source == null || clip == null)
        {
            return;
        }

        if (source.clip != clip)
        {
            source.clip = clip;
        }

        source.loop = true;
        source.volume = volume;

        if (!source.isPlaying)
        {
            source.Play();
        }
    }

    private void StopLoop(AudioSource source, AudioClip clip)
    {
        if (source == null)
        {
            return;
        }

        if (source.clip == clip && source.isPlaying)
        {
            source.Stop();
        }
    }

    private AudioSource EnsureAudioSource(
        AudioSource source,
        string childName,
        bool loop
    )
    {
        if (source != null)
        {
            source.loop = loop;
            return source;
        }

        GameObject child = new GameObject(childName);
        child.transform.SetParent(transform, false);

        AudioSource createdSource = child.AddComponent<AudioSource>();
        createdSource.loop = loop;
        createdSource.playOnAwake = false;

        return createdSource;
    }
}
