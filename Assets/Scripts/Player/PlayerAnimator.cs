using UnityEngine;

/// Playerのアニメーションを制御する
public class PlayerAnimator : MonoBehaviour
{
    private bool _dir = true; // Player向き | true: 右, false: 左 (回転0度がデフォルトの右向き)

    // 移動方向より優先される向き指定があるか (ビーム発射中など)
    private bool _hasDirectionOverride;

    [SerializeField]
    private Animator _animator;
    
    Transform _transform;

    // 状態名定義
    private string _namePlayerJumpUp = "PlayerJumpUp"; // Player上方向ジャンプ状態名
    private string _namePlayerJumpDown = "PlayerJumpDown"; // Player下「方向ジャンプ状態名

    // アニメーター状態名のハッシュID
    private int _playerJumpIDUp;
    private int _playerJumpIDDown;

    void Awake()
    {
        // SerializeFieldで取得
        //_animator = GetComponent<Animator>();
        _transform = GetComponent<Transform>();

        // ハッシュIDを取得
        _playerJumpIDUp = Animator.StringToHash(_namePlayerJumpUp);
        _playerJumpIDDown = Animator.StringToHash(_namePlayerJumpDown);
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// Playerの状態定義に必要な変数を設定
    /// </summary>
    /// <param name="xVelocity">x方向の速度</param>
    /// <param name="yVelocity">y方向の速度</param>
    /// <param name="isGrounded">着地しているか</param>
    public void SetPlayerAnimStatus(float xVelocity, float yVelocity, bool isGrounded)
    {
        // 向きの優先指定中は移動方向で上書きしない
        if (xVelocity != 0 && !_hasDirectionOverride)
        {
            SwitchDirection((xVelocity > 0) ? true : false);
        }

        if (_animator != null)
        {
            _animator.SetFloat("Speed", Mathf.Abs(xVelocity));
            _animator.SetBool("IsGrounded", isGrounded);
            PlayerJumper(yVelocity, isGrounded);
        }
        else
        {
            Debug.LogError("Animatorが取得できていません", this);
        }
    }

    /// <summary>
    /// 移動方向より優先して向きを指定する (ビーム発射中など)
    /// </summary>
    /// <param name="faceRight">true: 右, false: 左</param>
    public void SetDirectionOverride(bool faceRight)
    {
        _hasDirectionOverride = true;
        SwitchDirection(faceRight);
    }

    /// <summary>
    /// 向きの優先指定を解除し、移動方向による制御に戻す
    /// </summary>
    public void ClearDirectionOverride()
    {
        _hasDirectionOverride = false;
    }

    /// <summary>
    /// 見た目の向きを設定
    /// </summary>
    /// <param name="newDir"></param>
    private void SwitchDirection(bool newDir)
    {
        if (newDir == _dir)
        {
            return;
        }

        _dir = newDir;

        // Debug.Log($"Called:{dir}");
        // 向きに応じて回転
        if (_dir)
        {
            _transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
        else
        {
            _transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }
    }

    /// <summary>
    /// Playerのジャンプモーションを実行する
    /// </summary>
    private void PlayerJumper(float yVel, bool isGrounded)
    {
        if (!isGrounded && yVel >= 0)
        {
            _animator.Play(_playerJumpIDUp);
        }
        else if (!isGrounded)
        {
            _animator.Play(_playerJumpIDDown);
        }
    }
}
