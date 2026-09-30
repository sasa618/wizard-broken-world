using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    //移動スピード
    [SerializeField] private float moveSpeed = 10.0f;

    //ジャンプ力
    [SerializeField] private float jumpForce = 15.0f;

    //プレイヤーの足元にある子オブジェクト
    [SerializeField] private Transform groundChecker;
    //地面をチェックする円の半径
    [SerializeField] private float checkerRadius = 0.1f;
    //地面のレイヤー
    [SerializeField] private LayerMask groundLayer;

    //地面に着地しているかどうか
    private bool isGrounded;
        
    //プレイヤーのRigidbody2D
    private Rigidbody2D rb;

    //プレイヤーのSpriteRenderer
    private SpriteRenderer sr;
    
    void Start()
    {
        //プレイヤーのRigidBody2Dを取得
        rb = GetComponent<Rigidbody2D>();

        //プレイヤーのSpriteRendererを取得
        sr = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        //歩行の処理
        if (IsGamePaused())
        {
            rb.linearVelocityX = 0f;
            return;
        }

        Walk();
        //ジャンプの処理
        Jump();
    }

    //プレイヤーを左右に動かすメソッド
    private static bool IsGamePaused()
    {
        return Time.timeScale == 0f || PauseManager.IsAnyPaused;
    }

    private void Walk()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
        {
            rb.linearVelocityX = 0f;
            return;
        }

        float direction = 0f;

        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
        {
            direction -= 1f;
        }

        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
        {
            direction += 1f;
        }

        rb.linearVelocityX = direction * moveSpeed;

        //右に進んでいたら (デフォルトが左向きの場合の単純な処理)
        if (direction > 0)
        {
            //左右反転させる
            sr.flipX = true;
        }
        //左に進んでいたら
        else if(direction < 0)
        {
            //デフォルトのまま
            sr.flipX = false;
        }
    }

    //プレイヤーをスペースキーでジャンプさせるメソッド
    private void Jump()
    {
        //足元のチェッカーが地面を検知したら
        isGrounded = Physics2D.OverlapCircle(
            groundChecker.position,
            checkerRadius,
            groundLayer
        );

        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
        {
            return;
        }

        //スペースキーを押した、かつ着地している場合に
        if (keyboard.spaceKey.wasPressedThisFrame && isGrounded)
        {
            //上方向に一気に力を加える
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            SoundManager.Instance?.PlaySE(SEType.Jump);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundChecker == null)
        {
            return;
        }

        Gizmos.DrawWireSphere(
            groundChecker.position,
            checkerRadius
        );
    }
}
