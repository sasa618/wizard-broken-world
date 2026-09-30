using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    private enum EnemyState
    {
        Patrol,   //巡回中
        Attack,   //攻撃中
        Repaired  //修理済み
    }
    
    [SerializeField] protected float moveSpeed = 5.0f;
    [SerializeField] protected EnemyBullet bulletPrefab;
    [SerializeField] protected Transform muzzle;
    [SerializeField] protected float fireInterval = 2f;
    [SerializeField] protected float sightRange = 6f;
    [SerializeField] protected LayerMask sightMask;
    [SerializeField] protected float contactDamage = 10f;
    [SerializeField] private float enemyContactTurnCooldown = 0.15f;
    [SerializeField] private float bossKnockbackDuration = 0.2f;
    [SerializeField] private float neutralizedGravityScale = 1f;

    protected Rigidbody2D rb;
    private EnemyState state = EnemyState.Patrol;
    protected float fireTimer;
    private Transform playerTransform;
    private float lastEnemyContactTurnTime = -999f;
    private float bossKnockbackUntil;

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    //virtualは付けず、子からの変更を受け付けない(修理済みチェックだけ行う)
    private void Update()
    {
        if(state == EnemyState.Repaired) return;
        if(IsBossKnockbackActive) return;

        UpdateAlive();
    }

    //生きている間の毎フレームの行動。overrideすると行動パターンを差し替えられる
    protected virtual void UpdateAlive()
    {
        if(PlayerInSight())
        {
            state = EnemyState.Attack;
            rb.linearVelocityX = 0f;

            fireTimer += Time.deltaTime;
            if(fireTimer >= fireInterval)
            {
                fireTimer = 0f;
                Shoot();
            }
        }
        else
        {
            state = EnemyState.Patrol;
            Move();
        }
    }

    protected virtual void Move()
    {
        rb.linearVelocityX = FacingDirection.x * moveSpeed;
    }

    protected virtual void Shoot()
    {
        FireBullet(FacingDirection);
    }

    //この関数を呼び出せば弾をdirectionの方向へ出せる
    protected void FireBullet(Vector2 direction)
    {
        EnemyBullet bullet = Instantiate(bulletPrefab, muzzle.position, Quaternion.identity);
        bullet.Shoot(direction);
        SoundManager.Instance?.PlaySE(SEType.EnemyAttack);
    }

    //overrideすると視界の形を差し替えられる(既定は向いている方向への直線)
    protected virtual bool PlayerInSight()
    {
        RaycastHit2D hit = Physics2D.Raycast(
            muzzle.position,
            FacingDirection,
            sightRange,
            sightMask
        );

        //↓デバッグ用(直ったら消す)
    Debug.DrawRay(muzzle.position, FacingDirection * sightRange, Color.red);
    Debug.Log(hit.collider != null ? $"視線が当たった: {hit.collider.name}" : "何にも当たっていない");

        return hit.collider != null && hit.collider.CompareTag("Player");
    }

    //overrideすると修理後の追加演出を入れられる(必ずbase.SetRepaired()を呼ぶこと)
    public virtual void SetRepaired()
    {
        state = EnemyState.Repaired;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = Mathf.Max(rb.gravityScale, neutralizedGravityScale);
        rb.constraints &= ~(RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY);
        rb.constraints |= RigidbodyConstraints2D.FreezeRotation;
        rb.WakeUp();
        EnsurePushColliderAvailable();
        bossKnockbackUntil = 0f;

        // Keep colliders active so neutralized enemies can fall and be pushed.
    }

    public void TryDealContactDamage(Collider2D collision)
    {
        if(state == EnemyState.Repaired || contactDamage <= 0f) return;

        if(!collision.CompareTag("Player")) return;

        PlayerHP playerHP = collision.GetComponentInParent<PlayerHP>();
        if(playerHP == null) return;

        playerHP.TakeDamage(contactDamage);
    }

    //プレイヤーのTransformを取得する。プレイヤーが見つからなければfalse
    protected virtual bool CanTurnOnEnemyContact => false;

    public bool IsRepaired => state == EnemyState.Repaired;

    private bool IsBossKnockbackActive => Time.time < bossKnockbackUntil;

    public bool TryKnockbackFromBoss(Vector2 knockbackVelocity)
    {
        if(rb == null) return false;

        if(!IsRepaired)
        {
            rb.linearVelocity = knockbackVelocity;
            rb.WakeUp();
            bossKnockbackUntil = Time.time + bossKnockbackDuration;
            return true;
        }

        if(rb.bodyType != RigidbodyType2D.Dynamic)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
        }

        rb.linearVelocity = knockbackVelocity;
        rb.WakeUp();
        return true;
    }

    private void EnsurePushColliderAvailable()
    {
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            if (col.enabled && !col.isTrigger)
            {
                return;
            }
        }

        foreach (Collider2D col in colliders)
        {
            if (!col.enabled)
            {
                continue;
            }

            col.isTrigger = false;
            return;
        }
    }

    protected virtual void TurnAroundForEnemyContact()
    {
        Vector3 scale = transform.localScale;
        scale.x = -scale.x;
        transform.localScale = scale;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryTurnOnEnemyContact(collision.collider);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        TryTurnOnEnemyContact(collision);
    }

    private void TryTurnOnEnemyContact(Collider2D collision)
    {
        if(IsRepaired || !CanTurnOnEnemyContact) return;
        if(IsBossKnockbackActive) return;
        if(Time.time - lastEnemyContactTurnTime < enemyContactTurnCooldown) return;
        if(!collision.CompareTag("Enemy")) return;

        EnemyBase otherEnemy = collision.GetComponentInParent<EnemyBase>();
        if(otherEnemy == null || otherEnemy == this) return;

        TurnAroundForEnemyContact();
        lastEnemyContactTurnTime = Time.time;
    }

    protected bool TryGetPlayerTransform(out Transform player)
    {
        if(playerTransform == null)
        {
            //プレイヤーは実行時にスポーンされるため、初回に探して覚えておく
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if(playerObject == null)
            {
                player = null;
                return false;
            }

            playerTransform = playerObject.transform;
        }

        player = playerTransform;
        return true;
    }

    public Vector2 FacingDirection
    {
        get
        {
            return transform.localScale.x > 0 ? Vector2.left : Vector2.right;
        }
    }

    public bool IsMoving()
    {
        return rb.linearVelocityX != 0 || rb.linearVelocityY != 0;
    }
}
