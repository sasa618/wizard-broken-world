using UnityEngine;

/// <summary>
/// Animatorが設定されているオブジェクトにアタッチしてください！！
/// </summary>
[RequireComponent(typeof(Animator))]
public class EnemyAnimator : MonoBehaviour
{
    [SerializeField] private EnemyBase _enemy;

    [SerializeField] private BossMovement _boss;

    [SerializeField] private bool _isBoss = false;

    [Header("↓実行中に変更しないで！！")]
    [SerializeField, Tooltip("修理済みの見た目にするか")] private bool _isRepaired = false;

    [SerializeField] private bool _enableMoveAnimation = true;

    private Animator _animator;

    private int _isMovingID; // "IsMoving": 動いているかどうか

    void Awake()
    {
        _animator = GetComponent<Animator>();

        if (_enableMoveAnimation)
        {
            _isMovingID = Animator.StringToHash("IsMoving");
        }

        if (_isRepaired)
        {
            _animator.SetBool("IsRepaired", true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_enableMoveAnimation)
        {
            SetIsMoving();
            //Debug.Log(_enemy.IsMoving());
        }
    }

    private void SetIsMoving()
    {
        if (_isBoss && _boss != null)
        {
            _animator.SetBool(_isMovingID, _boss.IsMoving());
        }
        else if (_enemy != null) {
            _animator.SetBool(_isMovingID, _enemy.IsMoving());
        }
    }
}
