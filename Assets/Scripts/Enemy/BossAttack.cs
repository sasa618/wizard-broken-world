using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class BossAttack : MonoBehaviour
{
    private enum BossAttackType
    {
        AimShot,
        ThreeWay,
        GravityShot,
        GroundWave,
        HomingShot,
        FiveWay,
        Combo
    }

    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private PlayerSpawner playerSpawner;
    [SerializeField] private Transform muzzle;
    [SerializeField] private Transform groundWaveOrigin;

    [Header("Prefabs")]
    [SerializeField] private StraightBossBullet straightBulletPrefab;
    [SerializeField] private GravityBossBullet gravityBulletPrefab;
    [SerializeField] private HomingBossBullet homingBulletPrefab;
    [SerializeField] private GroundWaveBullet groundWaveBulletPrefab;

    [Header("Common")]
    [SerializeField] private float attackInterval = 2f;
    [SerializeField] private float bulletSpeed = 8f;
    [SerializeField] private float damage = 10f;

    [Header("Telegraph")]
    [SerializeField] private float attackTelegraphDuration = 0.6f;
    [SerializeField] private LineRenderer telegraphLine;
    [SerializeField] private Material telegraphMaterial;
    [SerializeField] private Color telegraphColor = Color.red;
    [SerializeField] private float telegraphLineLength = 3f;
    [SerializeField] private float telegraphLineWidth = 0.08f;
    [SerializeField] private float groundWaveTelegraphHalfWidth = 2.5f;
    [SerializeField] private float chargePulseScale = 1.15f;

    [Header("Spread")]
    [SerializeField] private float threeWaySpreadAngle = 18f;
    [SerializeField] private float fiveWaySpreadAngle = 30f;

    [Header("Gravity Shot")]
    [SerializeField] private float gravityShotHorizontalSpeed = 4f;
    [SerializeField] private float gravityShotUpSpeed = 7f;
    [SerializeField] private float gravityShotGravityScale = 1.5f;

    [Header("Ground Wave")]
    [SerializeField] private float groundWaveSpeed = 6f;

    [Header("Homing")]
    [SerializeField] private float homingDuration = 1f;
    [SerializeField] private float homingTurnSpeed = 120f;

    [Header("Phase 3")]
    [Range(0f, 1f)]
    [SerializeField] private float comboChance = 0.2f;

    private float attackTimer;
    private BossAttackType? lastAttackType;
    private bool missingTargetLogged;
    private BossController controller;
    private Coroutine attackCoroutine;
    private Vector3 scaleBeforeTelegraph;
    private readonly List<LineRenderer> telegraphLines = new List<LineRenderer>();
    private Vector2[] pendingAttackDirections = new Vector2[0];
    private Vector2 pendingGravityVelocity;

    private void Awake()
    {
        controller = GetComponent<BossController>();

        if (muzzle == null)
        {
            muzzle = transform;
        }

        if (groundWaveOrigin == null)
        {
            groundWaveOrigin = transform;
        }

        EnsureTelegraphLineCount(1);
    }

    private void Start()
    {
        ResolveTargetFromSpawner();
    }

    public void Tick(BossPhase phase)
    {
        if (phase == BossPhase.Dead)
        {
            return;
        }

        attackTimer += Time.deltaTime;

        if (attackTimer < attackInterval)
        {
            return;
        }

        attackTimer = 0f;

        if (target == null)
        {
            ResolveTargetFromSpawner();
        }

        if (target == null)
        {
            if (!missingTargetLogged)
            {
                Debug.LogWarning("BossAttack target is not assigned.", this);
                missingTargetLogged = true;
            }

            return;
        }

        BeginAttack(phase);
    }

    public bool BeginAttack(BossPhase phase)
    {
        if (phase == BossPhase.Dead || attackCoroutine != null)
        {
            return false;
        }

        if (target == null)
        {
            ResolveTargetFromSpawner();
        }

        if (target == null)
        {
            if (!missingTargetLogged)
            {
                Debug.LogWarning("BossAttack target is not assigned.", this);
                missingTargetLogged = true;
            }

            return false;
        }

        attackCoroutine = StartCoroutine(AttackRoutine(ChooseAttack(phase)));
        return true;
    }

    public void CancelAttack()
    {
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }

        HideTelegraph();
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        missingTargetLogged = false;
    }

    private void ResolveTargetFromSpawner()
    {
        if (target != null)
        {
            return;
        }

        if (playerSpawner == null)
        {
            playerSpawner = FindFirstObjectByType<PlayerSpawner>();
        }

        if (playerSpawner != null)
        {
            target = playerSpawner.SpawnedPlayerTransform;
        }

        if (target == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            target = playerObject != null ? playerObject.transform : null;
        }
    }

    private BossAttackType ChooseAttack(BossPhase phase)
    {
        BossAttackType[] candidates = phase switch
        {
            BossPhase.Phase1 => new[] { BossAttackType.AimShot, BossAttackType.ThreeWay },
            BossPhase.Phase2 => new[] { BossAttackType.ThreeWay, BossAttackType.GravityShot, BossAttackType.GroundWave },
            BossPhase.Phase3 => Random.value < comboChance
                ? new[] { BossAttackType.Combo }
                : new[] { BossAttackType.HomingShot, BossAttackType.GroundWave, BossAttackType.FiveWay },
            _ => new[] { BossAttackType.AimShot }
        };

        BossAttackType selected = candidates[Random.Range(0, candidates.Length)];

        if (lastAttackType.HasValue && candidates.Length > 1 && selected == lastAttackType.Value)
        {
            selected = candidates[Random.Range(0, candidates.Length)];
        }

        lastAttackType = selected;
        return selected;
    }

    private void ExecuteAttack(BossAttackType attackType)
    {
        SoundManager.Instance?.PlaySE(SEType.EnemyAttack);

        switch (attackType)
        {
            case BossAttackType.AimShot:
                FireStraight(GetPendingDirectionOrTarget());
                break;
            case BossAttackType.ThreeWay:
                FireDirections(pendingAttackDirections);
                break;
            case BossAttackType.GravityShot:
                FireGravityShot(pendingGravityVelocity);
                break;
            case BossAttackType.GroundWave:
                FireGroundWave();
                break;
            case BossAttackType.HomingShot:
                FireHomingShot(GetPendingDirectionOrTarget());
                break;
            case BossAttackType.FiveWay:
                FireDirections(pendingAttackDirections);
                break;
            case BossAttackType.Combo:
                FireGroundWave();
                FireDirections(pendingAttackDirections);
                break;
        }
    }

    private IEnumerator AttackRoutine(BossAttackType attackType)
    {
        CapturePendingAttack(attackType);
        ShowTelegraph(attackType);

        float timer = 0f;
        while (timer < attackTelegraphDuration)
        {
            if (controller != null && controller.IsDead)
            {
                HideTelegraph();
                attackCoroutine = null;
                yield break;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        HideTelegraph();
        ExecuteAttack(attackType);
        attackCoroutine = null;

        if (controller != null)
        {
            controller.NotifyAttackFinished();
        }
    }

    private Vector2 GetDirectionToTarget()
    {
        return ((Vector2)target.position - (Vector2)muzzle.position).normalized;
    }

    private void FireStraight(Vector2 direction)
    {
        if (straightBulletPrefab == null)
        {
            return;
        }

        StraightBossBullet bullet = Instantiate(straightBulletPrefab, muzzle.position, Quaternion.identity);
        bullet.Shoot(direction, bulletSpeed, damage);
    }

    private void FireSpread(int count, float spreadAngle)
    {
        FireDirections(GetSpreadDirections(count, spreadAngle));
    }

    private void FireDirections(Vector2[] directions)
    {
        foreach (Vector2 direction in directions)
        {
            FireStraight(direction);
        }
    }

    private void FireGravityShot()
    {
        FireGravityShot(GetGravityShotVelocity());
    }

    private void FireGravityShot(Vector2 velocity)
    {
        if (gravityBulletPrefab == null)
        {
            return;
        }

        GravityBossBullet bullet = Instantiate(gravityBulletPrefab, muzzle.position, Quaternion.identity);
        bullet.Shoot(velocity, gravityShotGravityScale, damage);
    }

    private void FireGroundWave()
    {
        if (groundWaveBulletPrefab == null)
        {
            return;
        }

        FireGroundWaveBullet(Vector2.left);
        FireGroundWaveBullet(Vector2.right);
    }

    private void FireGroundWaveBullet(Vector2 direction)
    {
        GroundWaveBullet bullet = Instantiate(
            groundWaveBulletPrefab,
            groundWaveOrigin.position,
            Quaternion.identity
        );

        bullet.Shoot(direction, groundWaveSpeed, damage);
    }

    private void FireHomingShot()
    {
        FireHomingShot(GetDirectionToTarget());
    }

    private void FireHomingShot(Vector2 direction)
    {
        if (homingBulletPrefab == null)
        {
            return;
        }

        HomingBossBullet bullet = Instantiate(homingBulletPrefab, muzzle.position, Quaternion.identity);
        bullet.Shoot(direction, bulletSpeed, damage, target, homingDuration, homingTurnSpeed);
    }

    private void EnsureTelegraphLineCount(int count)
    {
        if (telegraphLine != null && !telegraphLines.Contains(telegraphLine))
        {
            telegraphLines.Add(telegraphLine);
        }

        while (telegraphLines.Count < count)
        {
            GameObject lineObject = new GameObject("AttackTelegraphLine");
            lineObject.transform.SetParent(transform, false);
            LineRenderer line = lineObject.AddComponent<LineRenderer>();
            ConfigureTelegraphLine(line);
            line.enabled = false;
            telegraphLines.Add(line);

            if (telegraphLine == null)
            {
                telegraphLine = line;
            }
        }

        if (telegraphMaterial == null)
        {
            Shader spriteShader = Shader.Find("Sprites/Default");
            if (spriteShader != null)
            {
                telegraphMaterial = new Material(spriteShader);
            }
        }

        foreach (LineRenderer line in telegraphLines)
        {
            ConfigureTelegraphLine(line);
        }
    }

    private void ConfigureTelegraphLine(LineRenderer line)
    {
        line.positionCount = 2;
        line.useWorldSpace = true;
        line.startWidth = telegraphLineWidth;
        line.endWidth = telegraphLineWidth;
        line.startColor = telegraphColor;
        line.endColor = telegraphColor;
        if (telegraphMaterial != null)
        {
            line.material = telegraphMaterial;
        }
    }

    private void ShowTelegraph(BossAttackType attackType)
    {
        scaleBeforeTelegraph = transform.localScale;
        transform.localScale = GetPulsedScale();
        UpdateTelegraph(attackType);
    }

    private void UpdateTelegraph(BossAttackType attackType)
    {
        HideTelegraphLines();

        switch (attackType)
        {
            case BossAttackType.GroundWave:
                SetTelegraphLine(
                    0,
                    groundWaveOrigin.position + Vector3.left * groundWaveTelegraphHalfWidth,
                    groundWaveOrigin.position + Vector3.right * groundWaveTelegraphHalfWidth
                );
                break;
            case BossAttackType.GravityShot:
                SetTelegraphLine(
                    0,
                    muzzle.position,
                    (Vector2)muzzle.position + pendingGravityVelocity.normalized * telegraphLineLength
                );
                break;
            case BossAttackType.Combo:
                SetTelegraphLine(
                    0,
                    groundWaveOrigin.position + Vector3.left * groundWaveTelegraphHalfWidth,
                    groundWaveOrigin.position + Vector3.right * groundWaveTelegraphHalfWidth
                );
                SetSpreadTelegraphLines(pendingAttackDirections, 1);
                break;
            case BossAttackType.HomingShot:
            case BossAttackType.AimShot:
                SetTelegraphLine(
                    0,
                    muzzle.position,
                    (Vector2)muzzle.position + GetPendingDirectionOrTarget() * telegraphLineLength
                );
                break;
            case BossAttackType.ThreeWay:
                SetSpreadTelegraphLines(pendingAttackDirections, 0);
                break;
            case BossAttackType.FiveWay:
                SetSpreadTelegraphLines(pendingAttackDirections, 0);
                break;
        }
    }

    private void SetSpreadTelegraphLines(Vector2[] directions, int startLineIndex)
    {
        EnsureTelegraphLineCount(startLineIndex + directions.Length);

        for (int i = 0; i < directions.Length; i++)
        {
            SetTelegraphLine(
                startLineIndex + i,
                muzzle.position,
                (Vector2)muzzle.position + directions[i] * telegraphLineLength
            );
        }
    }

    private void SetTelegraphLine(int index, Vector3 start, Vector3 end)
    {
        EnsureTelegraphLineCount(index + 1);

        LineRenderer line = telegraphLines[index];
        line.SetPosition(0, start);
        line.SetPosition(1, end);
        line.enabled = true;
    }

    private void HideTelegraph()
    {
        HideTelegraphLines();

        if (scaleBeforeTelegraph != Vector3.zero)
        {
            transform.localScale = scaleBeforeTelegraph;
        }
    }

    private void HideTelegraphLines()
    {
        foreach (LineRenderer line in telegraphLines)
        {
            if (line != null)
            {
                line.enabled = false;
            }
        }
    }

    private Vector3 GetPulsedScale()
    {
        return new Vector3(
            scaleBeforeTelegraph.x * chargePulseScale,
            scaleBeforeTelegraph.y * chargePulseScale,
            scaleBeforeTelegraph.z
        );
    }

    private Vector2 GetGravityShotVelocity()
    {
        float horizontalDirection = target.position.x >= transform.position.x ? 1f : -1f;
        return new Vector2(
            horizontalDirection * gravityShotHorizontalSpeed,
            gravityShotUpSpeed
        );
    }

    private void CapturePendingAttack(BossAttackType attackType)
    {
        pendingAttackDirections = new Vector2[0];
        pendingGravityVelocity = Vector2.zero;

        switch (attackType)
        {
            case BossAttackType.AimShot:
            case BossAttackType.HomingShot:
                pendingAttackDirections = new[] { GetDirectionToTarget() };
                break;
            case BossAttackType.ThreeWay:
                pendingAttackDirections = GetSpreadDirections(3, threeWaySpreadAngle);
                break;
            case BossAttackType.FiveWay:
            case BossAttackType.Combo:
                pendingAttackDirections = GetSpreadDirections(5, fiveWaySpreadAngle);
                break;
            case BossAttackType.GravityShot:
                pendingGravityVelocity = GetGravityShotVelocity();
                break;
        }
    }

    private Vector2 GetPendingDirectionOrTarget()
    {
        return pendingAttackDirections.Length > 0
            ? pendingAttackDirections[0]
            : GetDirectionToTarget();
    }

    private Vector2[] GetSpreadDirections(int count, float spreadAngle)
    {
        Vector2 centerDirection = GetDirectionToTarget();
        Vector2[] directions = new Vector2[count];
        float startAngle = -spreadAngle * 0.5f;
        float step = count > 1 ? spreadAngle / (count - 1) : 0f;

        for (int i = 0; i < count; i++)
        {
            directions[i] = Rotate(centerDirection, startAngle + step * i);
        }

        return directions;
    }

    private static Vector2 Rotate(Vector2 direction, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        float sin = Mathf.Sin(radians);
        float cos = Mathf.Cos(radians);

        return new Vector2(
            direction.x * cos - direction.y * sin,
            direction.x * sin + direction.y * cos
        ).normalized;
    }
}
