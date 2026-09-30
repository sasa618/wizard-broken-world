using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BossMovement))]
[RequireComponent(typeof(BossJump))]
[RequireComponent(typeof(BossAttack))]
public class BossController : MonoBehaviour
{
    [SerializeField] private float maxHP = 300f;
    [SerializeField] private BossPhase currentPhase = BossPhase.Phase1;
    [SerializeField] private float currentHP;
    [SerializeField] private bool isDead;
    [SerializeField] private GameObject goalFlag;

    [Header("Action State")]
    [SerializeField] private BossActionState currentActionState = BossActionState.Move;
    [SerializeField] private float moveDurationMin = 1.2f;
    [SerializeField] private float moveDurationMax = 2.4f;
    [Range(0f, 1f)]
    [SerializeField] private float jumpChance = 0.35f;

    [Header("Respawn")]
    [SerializeField] private Transform bossRespawnPoint;
    [SerializeField] private float fallRespawnY = -8f;
    [SerializeField] private float respawnGraceTime = 0.5f;

    [Header("Neutralized Message")]
    [SerializeField] private GameObject neutralizedMessageObject;
    [SerializeField] private TMP_Text neutralizedMessageText;
    [SerializeField] private string neutralizedMessage = "\u306A\u304A\u3063\u305F\uFF01";

    [Header("Collision")]
    [SerializeField] private Collider2D bossCollider;
    [SerializeField] private float contactDamage = 20f;
    [SerializeField] private float repairableContactDamage = 20f;
    [SerializeField] private float neutralizedBounceSpeed = 10f;
    [SerializeField] private float contactKnockbackHorizontal = 8f;
    [SerializeField] private float contactKnockbackVertical = 4f;
    [SerializeField] private float contactKnockbackDirectionThreshold = 0.1f;
    [SerializeField] private float contactKnockbackDuration = 0.12f;
    [SerializeField] private float repairedEnemyKnockbackHorizontal = 10f;
    [SerializeField] private float repairedEnemyKnockbackVertical = 3f;
    [SerializeField] private float repairedEnemyPushCooldown = 0.2f;
    [SerializeField] private float neutralizedGravityScale = 1f;

    private BossMovement movement;
    private BossJump jump;
    private BossAttack attack;
    private RepairableVisualSwitcher visualSwitcher;
    private Rigidbody2D rb;
    private float moveTimer;
    private Coroutine contactKnockbackRoutine;
    private readonly Dictionary<int, int> repairableContactCounts = new Dictionary<int, int>();
    private readonly Dictionary<int, float> repairedEnemyNextPushTimes = new Dictionary<int, float>();
    private int movementBlockedRepairableId;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private float contactDisabledUntil;

    public float CurrentHP => currentHP;
    public float MaxHP => maxHP;
    public float HPRatio => maxHP > 0f ? currentHP / maxHP : 0f;
    public BossPhase CurrentPhase => currentPhase;
    public BossActionState CurrentActionState => currentActionState;
    public bool IsDead => isDead;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        initialPosition = transform.position;
        initialRotation = transform.rotation;

        if (bossCollider == null)
        {
            bossCollider = GetComponent<Collider2D>();
        }

        movement = GetComponent<BossMovement>();
        jump = GetComponent<BossJump>();
        attack = GetComponent<BossAttack>();
        visualSwitcher = GetComponent<RepairableVisualSwitcher>();
        currentHP = maxHP;
        UpdatePhase();
        EnterMoveState();

        if (visualSwitcher != null)
        {
            visualSwitcher.SetBroken();
        }

        if (goalFlag != null)
        {
            goalFlag.SetActive(false);
        }

        SetupNeutralizedMessage(false);
    }

    private void Update()
    {
        if (isDead)
        {
            return;
        }

        if (TryRespawnAfterFall())
        {
            return;
        }

        UpdatePhase();
        TickActionState();
    }

    private void FixedUpdate()
    {
        if (currentActionState != BossActionState.Move)
        {
            return;
        }

        movement.Tick();
        DealMovementBlockedRepairableDamage(movement.LastWallAheadCollider);
    }

    private void LateUpdate()
    {
        KeepNeutralizedMessageReadable();
    }

    public void TakeDamage(float amount)
    {
        if (isDead || amount <= 0f)
        {
            return;
        }

        currentHP = Mathf.Max(currentHP - amount, 0f);
        UpdatePhase();

        Debug.Log($"Boss took {amount} damage. HP: {currentHP}/{maxHP}, Phase: {currentPhase}", this);

        if (currentHP <= 0f)
        {
            Die();
        }
    }

    public void NotifyAttackFinished()
    {
        if (currentActionState != BossActionState.Attack || isDead)
        {
            return;
        }

        EnterMoveState();
    }

    private bool TryRespawnAfterFall()
    {
        if(currentActionState == BossActionState.Neutralized) return false;
        if(transform.position.y >= fallRespawnY) return false;

        RespawnFromFall();
        return true;
    }

    private void RespawnFromFall()
    {
        attack.CancelAttack();
        movement.Stop();
        jump.ResetJumpState();

        if(contactKnockbackRoutine != null)
        {
            StopCoroutine(contactKnockbackRoutine);
            contactKnockbackRoutine = null;
        }

        repairableContactCounts.Clear();
        repairedEnemyNextPushTimes.Clear();
        movementBlockedRepairableId = 0;
        contactDisabledUntil = Time.time + Mathf.Max(0f, respawnGraceTime);

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        transform.SetPositionAndRotation(GetBossRespawnPosition(), initialRotation);

        EnterMoveState();
    }

    private Vector3 GetBossRespawnPosition()
    {
        return bossRespawnPoint != null
            ? bossRespawnPoint.position
            : initialPosition;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        DealContactDamage(collision.collider);
        DealRepairableContactDamage(collision);
        TryPushEnemy(collision.collider);
        TryBounceNeutralizedPlayer(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        DealContactDamage(collision.collider);
        TryPushEnemy(collision.collider);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        DealContactDamage(collision);
        DealRepairableContactDamage(collision);
        TryPushEnemy(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        DealContactDamage(collision);
        TryPushEnemy(collision);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        ForgetRepairableContact(collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        ForgetRepairableContact(collision);
    }

    private void DealContactDamage(Collider2D collision)
    {
        if(Time.time < contactDisabledUntil) return;
        if(isDead || currentActionState == BossActionState.Neutralized || contactDamage <= 0f) return;

        if(!collision.CompareTag("Player")) return;

        PlayerHP playerHP = collision.GetComponentInParent<PlayerHP>();
        if(playerHP == null) return;

        float hpBefore = playerHP.CurrentHP;
        playerHP.TakeDamage(contactDamage);

        if(playerHP.CurrentHP < hpBefore)
        {
            ApplyContactKnockback(collision);
        }
    }

    private void DealRepairableContactDamage(Collider2D collision)
    {
        if(Time.time < contactDisabledUntil) return;
        if(isDead || currentActionState == BossActionState.Neutralized || repairableContactDamage <= 0f) return;

        RepairableObject repairableObject = GetRepairableObject(collision);
        if(repairableObject == null) return;

        RegisterRepairableContact(repairableObject);
    }

    private void DealRepairableContactDamage(Collision2D collision)
    {
        if(Time.time < contactDisabledUntil) return;
        if(isDead || currentActionState == BossActionState.Neutralized || repairableContactDamage <= 0f) return;

        RepairableObject repairableObject = GetRepairableObject(collision.collider, collision.otherCollider);
        if(repairableObject == null) return;

        RegisterRepairableContact(repairableObject);
    }

    private void RegisterRepairableContact(RepairableObject repairableObject)
    {
        int repairableId = repairableObject.GetInstanceID();
        if(repairableContactCounts.ContainsKey(repairableId))
        {
            repairableContactCounts[repairableId]++;
            return;
        }

        repairableContactCounts.Add(repairableId, 1);
        repairableObject.ReduceRepairValue(repairableContactDamage);
    }

    private void DealMovementBlockedRepairableDamage(Collider2D wallCollider)
    {
        if(Time.time < contactDisabledUntil) return;
        if(isDead || currentActionState == BossActionState.Neutralized || repairableContactDamage <= 0f) return;

        RepairableObject repairableObject = GetRepairableObject(wallCollider);
        if(repairableObject == null)
        {
            movementBlockedRepairableId = 0;
            return;
        }

        int repairableId = repairableObject.GetInstanceID();
        if(movementBlockedRepairableId == repairableId || repairableContactCounts.ContainsKey(repairableId))
        {
            movementBlockedRepairableId = repairableId;
            return;
        }

        movementBlockedRepairableId = repairableId;
        repairableObject.ReduceRepairValue(repairableContactDamage);
    }

    private void ForgetRepairableContact(Collider2D collision)
    {
        RepairableObject repairableObject = GetRepairableObject(collision);
        if(repairableObject == null) return;

        ForgetRepairableContact(repairableObject);
    }

    private void ForgetRepairableContact(Collision2D collision)
    {
        RepairableObject repairableObject = GetRepairableObject(collision.collider, collision.otherCollider);
        if(repairableObject == null) return;

        ForgetRepairableContact(repairableObject);
    }

    private void ForgetRepairableContact(RepairableObject repairableObject)
    {
        int repairableId = repairableObject.GetInstanceID();
        if(!repairableContactCounts.TryGetValue(repairableId, out int contactCount)) return;

        contactCount--;
        if(contactCount <= 0)
        {
            repairableContactCounts.Remove(repairableId);
        }
        else
        {
            repairableContactCounts[repairableId] = contactCount;
        }
    }

    private void TryPushEnemy(Collider2D collision)
    {
        if(isDead || currentActionState == BossActionState.Neutralized) return;

        EnemyBase enemy = collision.GetComponentInParent<EnemyBase>();
        if(enemy == null) return;

        int enemyId = enemy.GetInstanceID();
        if(repairedEnemyNextPushTimes.TryGetValue(enemyId, out float nextPushTime) && Time.time < nextPushTime)
        {
            RestoreBossMovementIfMoving();
            return;
        }

        float direction = movement != null && movement.Direction != 0
            ? Mathf.Sign(movement.Direction)
            : Mathf.Sign(transform.localScale.x);

        if(Mathf.Approximately(direction, 0f))
        {
            direction = 1f;
        }

        Vector2 knockbackVelocity = new Vector2(
            direction * repairedEnemyKnockbackHorizontal,
            repairedEnemyKnockbackVertical
        );

        if(enemy.TryKnockbackFromBoss(knockbackVelocity))
        {
            repairedEnemyNextPushTimes[enemyId] = Time.time + repairedEnemyPushCooldown;
            RestoreBossMovementIfMoving();
        }
    }

    private void RestoreBossMovementIfMoving()
    {
        if(currentActionState != BossActionState.Move || movement == null) return;

        movement.RestoreHorizontalMovement();
    }

    private RepairableObject GetRepairableObject(params Collider2D[] colliders)
    {
        foreach(Collider2D sourceCollider in colliders)
        {
            if(sourceCollider == null) continue;

            RepairableObject repairableObject = sourceCollider.GetComponentInParent<RepairableObject>();
            if(repairableObject != null)
            {
                return repairableObject;
            }

            Rigidbody2D attachedRigidbody = sourceCollider.attachedRigidbody;
            if(attachedRigidbody == null) continue;

            repairableObject = attachedRigidbody.GetComponentInParent<RepairableObject>();
            if(repairableObject != null)
            {
                return repairableObject;
            }
        }

        return null;
    }

    private void TryBounceNeutralizedPlayer(Collision2D collision)
    {
        if(currentActionState != BossActionState.Neutralized) return;

        SpringBounce.TryBouncePlayer(collision, neutralizedBounceSpeed);
    }

    private void ApplyContactKnockback(Collider2D playerCollider)
    {
        Rigidbody2D playerRb = playerCollider.attachedRigidbody;
        if(playerRb == null)
        {
            playerRb = playerCollider.GetComponentInParent<Rigidbody2D>();
        }

        if(playerRb == null) return;

        float direction = GetKnockbackDirection(playerRb.transform);
        float horizontalVelocity = direction * contactKnockbackHorizontal;

        playerRb.linearVelocity = new Vector2(
            horizontalVelocity,
            contactKnockbackVertical
        );

        if(contactKnockbackRoutine != null)
        {
            StopCoroutine(contactKnockbackRoutine);
        }

        contactKnockbackRoutine = StartCoroutine(
            MaintainContactKnockback(playerRb, horizontalVelocity)
        );
    }

    private float GetKnockbackDirection(Transform playerTransform)
    {
        float horizontalDifference = playerTransform.position.x - transform.position.x;
        if(Mathf.Abs(horizontalDifference) >= contactKnockbackDirectionThreshold)
        {
            return Mathf.Sign(horizontalDifference);
        }

        if(Mathf.Abs(rb.linearVelocity.x) >= contactKnockbackDirectionThreshold)
        {
            return -Mathf.Sign(rb.linearVelocity.x);
        }

        if(movement != null && movement.Direction != 0)
        {
            return -Mathf.Sign(movement.Direction);
        }

        return Random.value < 0.5f ? -1f : 1f;
    }

    private IEnumerator MaintainContactKnockback(Rigidbody2D playerRb, float horizontalVelocity)
    {
        float endTime = Time.time + contactKnockbackDuration;

        while(playerRb != null && Time.time < endTime)
        {
            yield return new WaitForFixedUpdate();

            if(playerRb == null) yield break;

            playerRb.linearVelocity = new Vector2(
                horizontalVelocity,
                playerRb.linearVelocity.y
            );
        }

        contactKnockbackRoutine = null;
    }

    private void UpdatePhase()
    {
        if (isDead)
        {
            currentPhase = BossPhase.Dead;
            return;
        }

        float hpRatio = HPRatio;

        if (hpRatio >= 0.7f)
        {
            currentPhase = BossPhase.Phase1;
        }
        else if (hpRatio >= 0.4f)
        {
            currentPhase = BossPhase.Phase2;
        }
        else
        {
            currentPhase = BossPhase.Phase3;
        }
    }

    private void TickActionState()
    {
        switch (currentActionState)
        {
            case BossActionState.Move:
                TickMoveState();
                break;
            case BossActionState.Jump:
                TickJumpState();
                break;
            case BossActionState.Attack:
            case BossActionState.Neutralized:
                break;
        }
    }

    private void TickMoveState()
    {
        moveTimer -= Time.deltaTime;

        if (moveTimer > 0f || !jump.IsGrounded())
        {
            return;
        }

        if (Random.value < jumpChance)
        {
            EnterJumpState();
        }
        else
        {
            EnterAttackState();
        }
    }

    private void TickJumpState()
    {
        jump.TickJumpState();

        if (jump.IsJumpFinished())
        {
            EnterMoveState();
        }
    }

    private void EnterMoveState()
    {
        currentActionState = BossActionState.Move;
        ResetMoveTimer();
    }

    private void EnterJumpState()
    {
        currentActionState = BossActionState.Jump;
        movement.Stop();
        jump.BeginJump();
    }

    private void EnterAttackState()
    {
        currentActionState = BossActionState.Attack;
        movement.Stop();

        if (!attack.BeginAttack(currentPhase))
        {
            EnterMoveState();
        }
    }

    private void ResetMoveTimer()
    {
        float min = Mathf.Max(0.1f, moveDurationMin);
        float max = Mathf.Max(min, moveDurationMax);
        moveTimer = Random.Range(min, max);
    }

    private void Die()
    {
        isDead = true;
        currentPhase = BossPhase.Dead;
        currentActionState = BossActionState.Neutralized;
        SoundManager.Instance?.SwitchBGM(BGM.Clear);

        movement.Stop();
        attack.CancelAttack();
        attack.enabled = false;
        jump.enabled = false;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = Mathf.Max(rb.gravityScale, neutralizedGravityScale);
        rb.constraints &= ~(RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY);
        rb.constraints |= RigidbodyConstraints2D.FreezeRotation;
        rb.WakeUp();

        if (bossCollider != null)
        {
            bossCollider.isTrigger = false;
        }

        if (visualSwitcher != null)
        {
            visualSwitcher.SetRepaired();
        }

        SetupNeutralizedMessage(true);

        if (goalFlag != null)
        {
            goalFlag.SetActive(true);
        }

        Debug.Log("Boss defeated.", this);
    }

    private void SetupNeutralizedMessage(bool visible)
    {
        if (neutralizedMessageText != null)
        {
            neutralizedMessageText.text = neutralizedMessage;
        }

        if (neutralizedMessageObject != null)
        {
            neutralizedMessageObject.SetActive(visible);
            KeepNeutralizedMessageReadable();
        }
    }

    private void KeepNeutralizedMessageReadable()
    {
        if (neutralizedMessageObject == null)
        {
            return;
        }

        Vector3 localScale = neutralizedMessageObject.transform.localScale;
        float parentSign = transform.lossyScale.x < 0f ? -1f : 1f;
        localScale.x = Mathf.Abs(localScale.x) * parentSign;
        neutralizedMessageObject.transform.localScale = localScale;
    }
}
