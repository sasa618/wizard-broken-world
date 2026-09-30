using UnityEngine;
using UnityEngine.Events;

public class EnemyRepairable : MonoBehaviour, IRepairable
{
    //倒された敵の数、すべてのインスタンスで共有する
    private static int defeatedCount;

    public static int GetDefeatedCount()
    {
        return defeatedCount;
    }

    //defeatedCountの初期化
    public static void ResetCount()
    {
        defeatedCount = 0;
    }

    [SerializeField] private float maxRepairValue = 100f;

    [SerializeField] private UnityEvent onRepaired;

    [SerializeField] private ParticleSystem _repairedParticle;


    private float currentRepairValue;
    private bool isRepaired;

    public void Repair(float amount)
    {
        if(isRepaired || amount <= 0f) return;

        currentRepairValue += amount;

        if(currentRepairValue >= maxRepairValue)
        {
            currentRepairValue = maxRepairValue;
            isRepaired = true;

            //倒された後の処理、VisualSwitcher()などを呼び出す
            onRepaired.Invoke();
            SoundManager.Instance?.PlaySE(SEType.EnemyRepair);

            if (_repairedParticle != null)
            {
                _repairedParticle.Play(true);
            }

            Debug.Log($"{gameObject.name} を修理して無力化しました。");
        }
    }
}
