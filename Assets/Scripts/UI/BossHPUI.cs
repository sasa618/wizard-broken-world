using UnityEngine;

[RequireComponent(typeof(HealthBarView))]
[RequireComponent(typeof(CanvasGroup))]
public class BossHPUI : MonoBehaviour
{
    [SerializeField] private BossController boss;
    [SerializeField] private bool keepVisibleAfterNeutralized = true;

    private HealthBarView view;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        view = GetComponent<HealthBarView>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        FindBossIfNeeded();
        UpdateVisibility();
        UpdateHealthBar();
    }

    private void Update()
    {
        FindBossIfNeeded();
        UpdateVisibility();
        UpdateHealthBar();
    }

    private void FindBossIfNeeded()
    {
        if (boss != null)
        {
            return;
        }

        boss = FindFirstObjectByType<BossController>();
    }

    private void UpdateVisibility()
    {
        bool shouldShow = boss != null && (keepVisibleAfterNeutralized || !boss.IsDead);

        canvasGroup.alpha = shouldShow ? 1f : 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private void UpdateHealthBar()
    {
        if (boss == null)
        {
            view.SetHealth(0f, 1f);
            return;
        }

        view.SetHealth(boss.CurrentHP, boss.MaxHP);
    }
}
