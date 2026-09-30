using UnityEngine;

public class RepairableProgressBar : MonoBehaviour
{
    [SerializeField] private RepairableObject target;
    [SerializeField] private GameObject barRoot;
    [SerializeField] private Transform fillTransform;
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1f, 0f);
    [SerializeField] private float repairValueChangeDisplayTime = 1.5f;

    private Vector3 initialFillScale;
    private Vector3 initialFillLocalPosition;
    private bool isVisible;
    private bool repairBeamVisible;
    private float repairValueChangeVisibleUntil;

    private void Awake()
    {
        if (target == null)
        {
            target = GetComponentInParent<RepairableObject>();
        }

        if (barRoot == null)
        {
            barRoot = gameObject;
        }

        if (fillTransform != null)
        {
            initialFillScale = fillTransform.localScale;
            initialFillLocalPosition = fillTransform.localPosition;
        }

        SetVisible(false);
    }

    private void LateUpdate()
    {
        RefreshVisibility();

        if (!isVisible || target == null)
        {
            return;
        }

        barRoot.transform.position = target.transform.position + worldOffset;
        UpdateFill();
    }

    public void Show()
    {
        repairBeamVisible = true;
        RefreshVisibility();
    }

    public void Hide()
    {
        repairBeamVisible = false;
        RefreshVisibility();
    }

    public void ShowRepairValueChange()
    {
        if (target == null || !target.IsRepaired)
        {
            HideRepairValueChange();
            return;
        }

        repairValueChangeVisibleUntil = Time.time + repairValueChangeDisplayTime;
        RefreshVisibility();
    }

    public void HideRepairValueChange()
    {
        repairValueChangeVisibleUntil = 0f;
        RefreshVisibility();
    }

    private void UpdateFill()
    {
        if (fillTransform == null)
        {
            return;
        }

        float progress = target.RepairProgress;
        float fillScaleX = initialFillScale.x * progress;
        fillTransform.localScale = new Vector3(
            fillScaleX,
            initialFillScale.y,
            initialFillScale.z
        );

        fillTransform.localPosition = new Vector3(
            initialFillLocalPosition.x - (initialFillScale.x - fillScaleX) * 0.5f,
            initialFillLocalPosition.y,
            initialFillLocalPosition.z
        );
    }

    private void SetVisible(bool visible)
    {
        isVisible = visible;

        if (barRoot != null && (target == null || barRoot != target.gameObject))
        {
            barRoot.SetActive(visible);
        }
    }

    private void RefreshVisibility()
    {
        //未修理だけでなく、ビームでゲージ回復中(修理済みだが削られている)も表示する
        bool shouldShowForRepairBeam =
            repairBeamVisible && target != null && target.RepairProgress < 1f;
        bool shouldShowForRepairValueChange =
            target != null
            && target.IsRepaired
            && Time.time < repairValueChangeVisibleUntil;

        bool shouldShow = shouldShowForRepairBeam || shouldShowForRepairValueChange;

        if (shouldShow && target != null)
        {
            barRoot.transform.position = target.transform.position + worldOffset;
            UpdateFill();
        }

        SetVisible(shouldShow);
    }
}
