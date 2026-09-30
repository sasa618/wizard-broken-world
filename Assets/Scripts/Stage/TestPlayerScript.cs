using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerAnimator))]
public class TestPlayerController : MonoBehaviour
{
    [Header("移動")]
    [SerializeField] private float moveSpeed = 6f;

    [Header("ジャンプ")]
    [SerializeField] private float jumpPower = 10f;

    [Header("接地判定")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;

    [Header("はしご")]
    [SerializeField] private float climbSpeed = 5f;

    [SerializeField] private float moveSoundInputThreshold = 0.1f;
    [Header("段差乗り越え")]
    [SerializeField] private float stepHeight = 0.35f;
    [SerializeField] private float stepCheckDistance = 0.1f;

    private Rigidbody2D rb;
    private Collider2D bodyCollider;
    private readonly RaycastHit2D[] stepHits = new RaycastHit2D[1];
    private float moveInput;
    private bool isGrounded;

    private bool isOnLadder;
    private float verticalInput;
    private float normalGravityScale;

    private Animator animator;
    private PlayerAnimator _pa;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();
        animator = GetComponentInChildren<Animator>();

        normalGravityScale = rb.gravityScale;

        _pa = GetComponent<PlayerAnimator>();

    }

    private void Update()
    {
        if (IsGamePaused())
        {
            StopPlayerInput();
            return;
        }

        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
        {
            moveInput = 0f;
            verticalInput = 0f;
            StopMoveSound();
            return;
        }

        moveInput = 0f;

        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
        {
            moveInput -= 1f;
        }

        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
        {
            moveInput += 1f;
        }

        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );

        // はしご上でなければ通常ジャンプ
        if (!isOnLadder)
        {
            if (keyboard.spaceKey.wasPressedThisFrame && isGrounded)
            {
                rb.linearVelocity = new Vector2(
                    rb.linearVelocity.x,
                    jumpPower
                );

                PlayJumpSound();
            }
        }

        // はしご上での上下入力
        verticalInput = 0f;

        if (isOnLadder)
        {
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
            {
                verticalInput += 1f;
            }

            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
            {
                verticalInput -= 1f;
            }
        }

        UpdateMoveSound();
    }

    private void FixedUpdate()
    {
        if (IsGamePaused())
        {
            StopPlayerInput();
            return;
        }

        if (isOnLadder)
        {
            rb.gravityScale = 0f;

            rb.linearVelocity = new Vector2(
                moveInput * moveSpeed,
                verticalInput * climbSpeed
            );
        }
        else
        {
            rb.gravityScale = normalGravityScale;

            rb.linearVelocity = new Vector2(
                moveInput * moveSpeed,
                rb.linearVelocity.y
            );

            TryStepUp();
        }

        // 現在の状態をアニメーション側へ渡す
        _pa.SetPlayerAnimStatus(
            rb.linearVelocityX,
            rb.linearVelocityY,
            isGrounded
        );
    }

    // 低い段差はジャンプ無しで乗り越える
    private void TryStepUp()
    {
        if (moveInput == 0f || !isGrounded)
        {
            return;
        }

        Bounds bounds = bodyCollider.bounds;
        float direction = Mathf.Sign(moveInput);

        Vector2 lowOrigin = new Vector2(
            direction > 0f ? bounds.max.x : bounds.min.x,
            bounds.min.y + 0.05f
        );

        // 足元の前方に障害物があるか（はしご等のトリガーは無視）
        RaycastHit2D lowHit = CastSolidRay(
            lowOrigin,
            new Vector2(direction, 0f),
            stepCheckDistance
        );

        if (lowHit.collider == null)
        {
            return;
        }

        // 段差の上端より上が空いていなければ乗り越えられない壁
        Vector2 highOrigin = lowOrigin + Vector2.up * stepHeight;

        RaycastHit2D highHit = CastSolidRay(
            highOrigin,
            new Vector2(direction, 0f),
            stepCheckDistance
        );

        if (highHit.collider != null)
        {
            return;
        }

        // 乗り越え先の床の高さまで持ち上げる
        Vector2 downOrigin = new Vector2(
            lowOrigin.x + direction * stepCheckDistance,
            bounds.min.y + stepHeight
        );

        RaycastHit2D floorHit = CastSolidRay(
            downOrigin,
            Vector2.down,
            stepHeight
        );

        float targetFootY = floorHit.collider != null
            ? floorHit.point.y
            : bounds.min.y + stepHeight;

        float lift = targetFootY - bounds.min.y + 0.02f;

        if (lift <= 0f)
        {
            return;
        }

        rb.position = new Vector2(
            rb.position.x,
            rb.position.y + lift
        );
    }

    private RaycastHit2D CastSolidRay(
        Vector2 origin,
        Vector2 rayDirection,
        float distance
    )
    {
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(groundLayer);
        filter.useTriggers = false;

        int count = Physics2D.Raycast(
            origin,
            rayDirection,
            filter,
            stepHits,
            distance
        );

        return count > 0 ? stepHits[0] : default;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<LadderArea>() != null)
        {
            isOnLadder = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<LadderArea>() != null)
        {
            isOnLadder = false;
            verticalInput = 0f;
            rb.gravityScale = normalGravityScale;
        }
    }

    private void OnDisable()
    {
        StopMoveSound();
    }

    private static bool IsGamePaused()
    {
        return Time.timeScale == 0f || PauseManager.IsAnyPaused;
    }

    private void StopPlayerInput()
    {
        moveInput = 0f;
        verticalInput = 0f;
        StopMoveSound();
    }

    private void UpdateMoveSound()
    {
        bool shouldPlay =
            isGrounded &&
            !isOnLadder &&
            Mathf.Abs(moveInput) > moveSoundInputThreshold;

        if (shouldPlay)
        {
            PlayMoveSound();
        }
        else
        {
            StopMoveSound();
        }
    }

    private void PlayMoveSound()
    {
        SoundManager.Instance?.StartPlayerMove();
    }

    private void StopMoveSound()
    {
        SoundManager.Instance?.StopPlayerMove();
    }

    private void PlayJumpSound()
    {
        SoundManager.Instance?.PlaySE(SEType.Jump);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
        {
            return;
        }

        Gizmos.DrawWireSphere(
            groundCheck.position,
            groundCheckRadius
        );
    }
}
