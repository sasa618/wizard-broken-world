using System;
using System.Collections;
using UnityEngine;

//プレイヤーのHPを管理するスクリプト（Playerプレハブにアタッチする）
public class PlayerHP : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _playerSprite;
    [SerializeField, Tooltip("ダメージ時に赤くなる時間")] private float _redInterval = 0.75f;

    public static PlayerHP Instance { get; private set; }//スコア計算できるように

    //HPが変わったときにUIへ知らせるイベント（引数は 現在HP, 最大HP）
    //プレイヤーは実行時に生成されるのでUI側からInspectorで参照できない。そのためstaticにしている
    public static event Action<float, float> OnHPChanged;

    private float _timeCount = 0f;
    private bool _isPlayerRed; // playerが赤くなっている

    private Transform _cameraTransform;
    private Vector3 _cameraDefaultPos;
    private sbyte _shakeC;
    [SerializeField, Range(0f, 1f)] private float _shakeStrength = 0.1f;
    [SerializeField, Range(0f, 1f)] private float _shakeLength = 0.3f;

    [SerializeField, Range(0, 100)] private short _shakeInterval = 2;
    private short _frameCount;

    [Header("最大HP")]
    [SerializeField] private float maxHP = 100f;

    [Header("被弾後の無敵時間（秒）")]
    [SerializeField] private float invincibleDuration = 1f;

    [Header("無敵中の点滅間隔（秒）")]
    [SerializeField] private float blinkInterval = 0.1f;

    //現在のHP
    private float currentHP;

    //ゲームオーバーの多重呼び出し防止
    private bool isDead;

    //無敵時間の終了時刻
    private float invincibleUntil;

    private SpriteRenderer[] spriteRenderers;
    private Coroutine blinkRoutine;

    //HPバーなどのUIで使う用
    public float CurrentHP
    {
        get { return currentHP; }
    }

    public float MaxHP
    {
        get { return maxHP; }
    }

    //無敵時間中かどうか
    public bool IsInvincible => Time.time < invincibleUntil;

    private void Awake()
    {
        Instance = this;

        _cameraTransform = Camera.main.transform;
        _cameraDefaultPos = _cameraTransform.position;
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
    }

    void Start()
    {
        currentHP = maxHP;

        _timeCount = 0f;
        _isPlayerRed = false;
        _shakeC = 1;
        _frameCount = 0;

        //UIに初期値を知らせる
        OnHPChanged?.Invoke(currentHP, maxHP);
    }

    private void Update()
    {
        if (_isPlayerRed && Time.realtimeSinceStartup - _timeCount > _redInterval)
        {
            ResetPlayerColor();
        }
        else if (_isPlayerRed && Time.realtimeSinceStartup - _timeCount < _redInterval * _shakeLength && _frameCount++ == _shakeInterval)
        {
            _frameCount = 0;
            MoveCamera();
        }
    }

    /// <summary>
    /// プレイヤーをもとの色に戻す
    /// </summary>
    private void ResetPlayerColor()
    {
        _playerSprite.color = Color.white;
        _cameraTransform.position = _cameraDefaultPos;
        _isPlayerRed = false;
    }

    /// <summary>
    /// 画面を揺らす
    /// </summary>
    private void MoveCamera()
    {
        _cameraTransform.position = new Vector3(
            _cameraTransform.position.x + _shakeC * _shakeStrength,
            _cameraTransform.position.y,
            _cameraTransform.position.z
            );

        _shakeC *= -1;
    }

    //ダメージを受けるメソッド（敵の弾などから呼ばれる）
    public void TakeDamage(float damage)
    {
        if (isDead || IsInvincible)
        {
            return;
        }

        if (_playerSprite != null)
        {
            _playerSprite.color = Color.red;
            _timeCount = Time.realtimeSinceStartup;
            _isPlayerRed = true;
        }

        currentHP = Mathf.Max(currentHP - damage, 0f);
        PlayDamageSound();

        Debug.Log($"プレイヤーが{damage}ダメージを受けた 残りHP: {currentHP}");

        //UIに新しいHPを知らせる
        OnHPChanged?.Invoke(currentHP, maxHP);

        if (currentHP <= 0f)
        {
            Die();
            return;
        }

        StartInvincible();
    }

    //無敵時間を開始し、点滅を始める
    private void PlayDamageSound()
    {
        SoundManager.Instance?.PlaySE(SEType.PlayerDamage);
    }

    private void StartInvincible()
    {
        invincibleUntil = Time.time + invincibleDuration;

        if (blinkRoutine != null)
        {
            StopCoroutine(blinkRoutine);
        }

        blinkRoutine = StartCoroutine(BlinkRoutine());
    }

    //無敵時間中はスプライトを点滅させる
    private IEnumerator BlinkRoutine()
    {
        while (IsInvincible)
        {
            SetSpritesVisible(false);
            yield return new WaitForSeconds(blinkInterval);
            SetSpritesVisible(true);
            yield return new WaitForSeconds(blinkInterval);
        }

        //点滅終了後は必ず表示状態に戻す
        SetSpritesVisible(true);
        blinkRoutine = null;
    }

    private void SetSpritesVisible(bool visible)
    {
        foreach (SpriteRenderer sr in spriteRenderers)
        {
            if (sr != null)
            {
                sr.enabled = visible;
            }
        }
    }

    //HPが0になったときの処理
    private void Die()
    {
        isDead = true;

        if (GameOverPanelManager.Instance == null)
        {
            Debug.LogError(
                "シーン内にGameOverPanelManagerが見つかりません。",
                this
            );

            return;
        }

        GameOverPanelManager.Instance.GameOver();
    }
}
