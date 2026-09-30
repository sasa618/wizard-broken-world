using UnityEngine;

public class RepairableObject : MonoBehaviour, IRepairable
{
    [SerializeField] private float maxRepairValue = 100f;
    [SerializeField] private RepairableVisualSwitcher visualSwitcher;
    [SerializeField] private RepairableProgressBar progressBar;

    private float currentRepairValue;
    private bool isRepaired;
    [SerializeField] private bool startRepaired = false;

    public bool IsRepaired => isRepaired;
    public float RepairProgress =>
        maxRepairValue <= 0f
            ? 1f
            : Mathf.Clamp01(currentRepairValue / maxRepairValue);

    private void Awake()
    {
        if (progressBar == null)
        {
            progressBar = GetComponentInChildren<RepairableProgressBar>(true);
        }
    }

    private void Start()
    {
        if (startRepaired)
        {
            currentRepairValue = maxRepairValue;
            isRepaired = true;

            if (visualSwitcher != null)
            {
                visualSwitcher.SetRepaired();
            }
        }
        else
        {
            currentRepairValue = 0f;
            isRepaired = false;

            if (visualSwitcher != null)
            {
                visualSwitcher.SetBroken();
            }
        }
    }

    public void Break()
    {
        bool wasRepaired = isRepaired;

        currentRepairValue = 0f;
        isRepaired = false;

        if (visualSwitcher != null)
        {
            visualSwitcher.SetBroken();
        }

        if (wasRepaired)
        {
            SoundManager.Instance?.PlaySE(SEType.RepairableBreak);
        }
    }

    public bool ReduceRepairValue(float amount)
    {
        if (!isRepaired || amount <= 0f)
        {
            return false;
        }

        currentRepairValue = Mathf.Max(currentRepairValue - amount, 0f);

        if (currentRepairValue <= 0f)
        {
            Break();

            if (progressBar != null)
            {
                progressBar.HideRepairValueChange();
            }
        }
        else if (progressBar != null)
        {
            progressBar.ShowRepairValueChange();
        }

        return true;
    }

    public void Repair(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        //修理済みでも、ボス弾などで削られたゲージはビームで回復できる
        if (isRepaired)
        {
            currentRepairValue = Mathf.Min(
                currentRepairValue + amount,
                maxRepairValue
            );

            return;
        }

        currentRepairValue += amount;

        if (currentRepairValue >= maxRepairValue)
        {
            currentRepairValue = maxRepairValue;
            isRepaired = true;

            if (visualSwitcher != null)
            {
                visualSwitcher.SetRepaired();
            }

            SoundManager.Instance?.PlaySE(SEType.RepairComplete);

            if (RepairableManager.Instance != null)
            {
                RepairableManager.Instance.NotifyRepaired(this);
            }

            Debug.Log($"{gameObject.name} repaired.");
        }
    }

}
