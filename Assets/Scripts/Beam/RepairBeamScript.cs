using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(LineRenderer))]
public class RepairBeam : MonoBehaviour
{
    [SerializeField] private Transform beamOrigin;
    [SerializeField] private float maxDistance = 8f;
    [SerializeField] private float repairPerSecond = 40f;
    [SerializeField] private LayerMask hitLayers;

    [SerializeField] private Color startBeamColor;
    [SerializeField] private Color endBeamColor;
    [SerializeField] private float beamWidth;

    [SerializeField] private Material beamMaterial;

    [SerializeField] private ParticleSystem _glareParticle;
    [SerializeField] public ParticleSystem _dustParticle;
    [SerializeField] public ParticleSystem _repairParticle;

    [SerializeField] private Light2D _magicLight;

    private LineRenderer lineRenderer;
    private Camera mainCamera;

    private Transform _dustOriginTransform; // ビームが当たった場所から出るやつの位置
    private Transform _repairOriginTransform;

    [SerializeField, Tooltip("パーティクルの親オブジェクトを入れる")]
    private Transform _hitParticles;

    [SerializeField] private Transform _playerTransform;

    [SerializeField, Tooltip("未設定なら_playerTransformから自動取得する")]
    private PlayerAnimator _playerAnimator;

    [SerializeField, Range(0f, 2f), Tooltip("プレイヤーからこの距離(ワールド単位)以内にカーソルがあるときは向きを変えない")]
    private float _facingDeadZone = 0.5f;

    private Vector3 _tempHitPos;
    //private Vector3 _tempPos;

    private bool _isBeamEmitted;
    private bool _isDustEmitted;
    private RepairableProgressBar _currentProgressBar;

    private ContactFilter2D _beamContactFilter;
    private readonly List<RaycastHit2D> _beamHits = new List<RaycastHit2D>();

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        mainCamera = Camera.main;

        _isBeamEmitted = false;
        _isDustEmitted = false;

        //_tempPos = Vector3.zero;
        _tempHitPos = Vector3.zero;

        BeamHitEffect(false);

        _dustOriginTransform = _dustParticle != null ? _dustParticle.transform : null;
        _repairOriginTransform = _repairParticle != null ? _repairParticle.transform : null;

        lineRenderer.material = beamMaterial;

        lineRenderer.positionCount = 2;
        BeamSwitch(false);
        lineRenderer.useWorldSpace = true;

        if (_magicLight != null)
        {
            _magicLight.enabled = false;
        }

        // 色設定
        lineRenderer.startColor = startBeamColor;
        lineRenderer.endColor = endBeamColor;

        lineRenderer.startWidth = beamWidth;
        lineRenderer.endWidth = beamWidth;

        hitLayers =
        LayerMask.GetMask("Repairable", "Enemy", "Ground");

        // 当たり判定はトリガーコライダーなのでuseTriggersは必須
        _beamContactFilter = new ContactFilter2D
        {
            useTriggers = true
        };
        _beamContactFilter.SetLayerMask(hitLayers);

        if (_playerAnimator == null && _playerTransform != null)
        {
            _playerAnimator = _playerTransform.GetComponent<PlayerAnimator>();
        }
    }

    private void Update()
    {// Pause・Result・GameOver中はビームを出さない
        if (Time.timeScale == 0f || PauseManager.IsAnyPaused)
        {
            StopBeamOutput();
            return;
        }

        Quaternion tempQ;
        if (_playerTransform.rotation.eulerAngles.y != 0)
        {
            tempQ = Quaternion.Euler(0f, -180f, 0f);
            SetHitParticleRotation(tempQ);
        }
        else
        {
            tempQ = Quaternion.Euler(0f, 0f, 0f);
            SetHitParticleRotation(tempQ);
        }

        if (Mouse.current == null || beamOrigin == null || mainCamera == null)
        {
            StopBeamOutput();
            SetRepairSound(false);
            return;
        }

        if (Mouse.current.leftButton.isPressed)
        {
            DrawBeam();
        }
        else
        {
            SetHitParticlePosition(_tempHitPos);
            /*
            _dustOriginTransform.position = _tempHitPos;
            _repairOriginTransform.position = _tempHitPos;
            */

            //lineRenderer.enabled = false;
            BeamSwitch(false);
            BeamHitEffect(false);
            SetRepairSound(false);
            SetCurrentProgressBar(null);

            // ビームを撃っていない間は移動方向による向き制御に戻す
            if (_playerAnimator != null)
            {
                _playerAnimator.ClearDirectionOverride();
            }
        }
    }

    private void OnDisable()
    {
        StopBeamSound();
        SetRepairSound(false);
    }

    private void StopBeamOutput()
    {
        SetHitParticlePosition(_tempHitPos);
        /*
        _dustOriginTransform.position = _tempHitPos;
        _repairOriginTransform.position = _tempHitPos;
        */

        //lineRenderer.enabled = false;
        BeamSwitch(false);
        BeamHitEffect(false);
        SetRepairSound(false);
        SetCurrentProgressBar(null);

        if (_playerAnimator != null)
        {
            _playerAnimator.ClearDirectionOverride();
        }
    }

    private void DrawBeam()
    {
        Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(
           new Vector3(
               mouseScreenPosition.x,
               mouseScreenPosition.y,
               -mainCamera.transform.position.z
           )
       );

        // ビームを撃っている方向にプレイヤーを向ける
        // ビーム原点はプレイヤーの子なので、向きを更新してから原点を取り直す
        UpdatePlayerFacing(mouseWorldPosition);

        // 見た目の発射位置 (手元)。左右反転で位置が入れ替わる
        Vector2 muzzle = beamOrigin.position;

        // 狙いの基準点。左右反転しても動かないので、反転してもビームの角度が変わらない
        Vector2 aimOrigin = GetAimOrigin(muzzle);

        Vector2 direction =
            ((Vector2)mouseWorldPosition - aimOrigin).normalized;
        RaycastHit2D hit = RaycastBeam(aimOrigin, direction);
        Vector2 endPosition =
            aimOrigin + direction * maxDistance;

        bool hitFlag = hit.collider != null;
        bool hitRepairable = false;
        bool isRepairingObject = false;
        if (hitFlag)
        {
            endPosition = hit.point;

            RepairableObject repairableObject =
                hit.collider.GetComponentInParent<RepairableObject>();
            float repairProgressBefore =
                repairableObject != null
                    ? repairableObject.RepairProgress
                    : 1f;

            IRepairable repairable =
                hit.collider.GetComponentInParent<IRepairable>();

            if (repairable != null)
            {
                repairable.Repair(
                    repairPerSecond * Time.deltaTime
                );

                isRepairingObject =
                    repairableObject != null &&
                    repairProgressBefore < 1f &&
                    repairableObject.RepairProgress < 1f;
            }
            else
            {
                BossController boss =
                    hit.collider.GetComponentInParent<BossController>();

                if (boss != null)
                {
                    boss.TakeDamage(
                        repairPerSecond * Time.deltaTime
                    );
                    hitRepairable = true;
                }
            }

            RepairableProgressBar progressBar =
                repairableObject != null
                    ? repairableObject.GetComponentInChildren<RepairableProgressBar>(true)
                    : null;

            //未修理だけでなく、攻撃で削られた修理済み(ゲージ回復中)も対象にする
            if (repairableObject != null && repairableObject.RepairProgress < 1f)
            {
                SetCurrentProgressBar(progressBar);

                hitRepairable = hitRepairable || hit.collider.CompareTag("Repairable") || hit.collider.CompareTag("Enemy");
            }
            else
            {
                SetCurrentProgressBar(null);
            }

            _tempHitPos = new Vector3(endPosition.x, endPosition.y);
        }
        else
        {
            SetCurrentProgressBar(null);
        }

        SetRepairSound(isRepairingObject);

        SetHitParticlePosition(_tempHitPos);
        
        //lineRenderer.enabled = true;
        BeamSwitch(true);
        lineRenderer.SetPosition(0, muzzle);
        lineRenderer.SetPosition(1, endPosition);

        BeamHitEffect(hit, hitRepairable);

        float length = Vector2.Distance(muzzle, endPosition);

        lineRenderer.material.mainTextureScale = new Vector2(length, 1f);

        beamMaterial.mainTextureScale = new Vector2(length, 1f);
    }

    /// <summary>
    /// 狙いの基準点を返す
    /// 高さはビーム原点のまま、横位置だけプレイヤーの中心軸に合わせることで
    /// 左右反転しても基準点が動かないようにする
    /// (ビーム原点をそのまま使うと、反転した瞬間に原点が左右に飛んで角度が急変する)
    /// </summary>
    /// <param name="muzzle">見た目の発射位置</param>
    private Vector2 GetAimOrigin(Vector2 muzzle)
    {
        if (_playerTransform == null)
        {
            return muzzle;
        }

        return new Vector2(_playerTransform.position.x, muzzle.y);
    }

    /// <summary>
    /// カーソルの左右に合わせてプレイヤーの向きを上書きする
    /// </summary>
    /// <param name="mouseWorldPosition">カーソルのワールド座標</param>
    private void UpdatePlayerFacing(Vector3 mouseWorldPosition)
    {
        if (_playerAnimator == null || _playerTransform == null)
        {
            return;
        }

        // ビーム原点はプレイヤーの子で反転すると左右に動くため、
        // 反転の影響を受けないプレイヤー本体の位置を基準にする
        // (原点基準にすると、カーソルがプレイヤー付近のとき毎フレーム反転してしまう)
        float offsetX = mouseWorldPosition.x - _playerTransform.position.x;

        // カーソルがプレイヤーに重なっているときは向きを変えない
        if (Mathf.Abs(offsetX) < _facingDeadZone)
        {
            return;
        }

        _playerAnimator.SetDirectionOverride(offsetX > 0f);
    }

    /// <summary>
    /// 修理済みのはしごを無視して、最も手前に当たったものを返す
    /// </summary>
    private RaycastHit2D RaycastBeam(Vector2 origin, Vector2 direction)
    {
        int hitCount = Physics2D.Raycast(
            origin,
            direction,
            _beamContactFilter,
            _beamHits,
            maxDistance
        );

        RaycastHit2D nearest = default;

        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit2D candidate = _beamHits[i];

            if (candidate.collider == null || IsBeamThroughLadder(candidate.collider))
            {
                continue;
            }

            if (nearest.collider == null || candidate.distance < nearest.distance)
            {
                nearest = candidate;
            }
        }

        return nearest;
    }

    /// <summary>
    /// ビームを透過させるはしごかどうか
    /// 未修理のはしごは修理できるように当たり判定を残す
    /// </summary>
    private static bool IsBeamThroughLadder(Collider2D target)
    {
        // 修理用の当たり判定はLadderAreaの兄弟なので、修理対象の根元から探す
        RepairableObject repairableObject =
            target.GetComponentInParent<RepairableObject>();

        if (repairableObject != null)
        {
            //ゲージ満タンの修理済みはしごだけ透過する(削られている間は回復できるように当てる)
            return repairableObject.GetComponentInChildren<LadderArea>(true) != null
                && repairableObject.IsRepaired
                && repairableObject.RepairProgress >= 1f;
        }

        // 修理対象ではないはしごは常に透過させる
        return target.GetComponentInParent<LadderArea>(true) != null;
    }

    private void SetCurrentProgressBar(RepairableProgressBar progressBar)
    {
        if (_currentProgressBar == progressBar)
        {
            if (_currentProgressBar != null)
            {
                _currentProgressBar.Show();
            }

            return;
        }

        if (_currentProgressBar != null)
        {
            _currentProgressBar.Hide();
        }

        _currentProgressBar = progressBar;

        if (_currentProgressBar != null)
        {
            _currentProgressBar.Show();
        }
    }

    /// <summary>
    /// ビーム本体のオンオフ切り替え
    /// </summary>
    /// <param name="b"></param>
    private void BeamSwitch(bool b)
    {
        if (b && !_isBeamEmitted)
        {
            lineRenderer.enabled = true;
            if (_glareParticle != null && !_glareParticle.isPlaying)
            {
                _glareParticle.Play(true);
            }

            if (_magicLight != null)
            {
                _magicLight.enabled = true;
            }
            PlayBeamSound();
            _isBeamEmitted = true;
        }
        else if (!b && _isBeamEmitted)
        {
            lineRenderer.enabled = false;
            if (_glareParticle != null)
            {
                _glareParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            if (_magicLight != null)
            {
                _magicLight.enabled = false;
            }
            StopBeamSound();
            SetRepairSound(false);
            _isBeamEmitted = false;
        }
    }

    private void PlayBeamSound()
    {
        SoundManager.Instance?.StartRepairBeam();
    }

    private void StopBeamSound()
    {
        SoundManager.Instance?.StopRepairBeam();
    }

    private void SetRepairSound(bool shouldPlay)
    {
        if (shouldPlay)
        {
            PlayRepairSound();
        }
        else
        {
            StopRepairSound();
        }
    }

    private void PlayRepairSound()
    {
        SoundManager.Instance?.StartRepairProgress();
    }

    private void StopRepairSound()
    {
        SoundManager.Instance?.StopRepairProgress();
    }

    /// <summary>
    /// ヒット時のパーティクル表示切り替え
    /// </summary>
    /// <param name="hit"></param>
    /// <param name="hitRepairable"></param>
    private void BeamHitEffect(bool hit, bool hitRepairable=false)
    {
        if (hit && _isBeamEmitted)
        {
            if (_dustParticle != null && !_dustParticle.isPlaying)
            {
                _dustParticle.Play(true);
            }

            if (hitRepairable)
            {
                if (_repairParticle != null && !_repairParticle.isPlaying)
                {
                    _repairParticle.Play(true);
                }
            }
            else if (_repairParticle != null && _repairParticle.isPlaying)
            {
                _repairParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }

            _isDustEmitted = true;
        }
        else if (!hit && _isDustEmitted)
        {
            if (_dustParticle != null)
            {
                _dustParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }

            if (_repairParticle != null && _repairParticle.isPlaying)
            {
                _repairParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
            _isDustEmitted = false;
        }
    }

    private void SetHitParticleRotation(Quaternion rotation)
    {
        if (_dustOriginTransform != null)
        {
            _dustOriginTransform.localRotation = rotation;
        }

        if (_repairOriginTransform != null)
        {
            _repairOriginTransform.localRotation = rotation;
        }
    }

    private void SetHitParticlePosition(Vector3 position)
    {
        if (_hitParticles != null)
        {
            _hitParticles.position = position;
        }
    }
}
