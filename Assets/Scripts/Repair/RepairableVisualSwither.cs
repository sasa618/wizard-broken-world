using UnityEngine;

public class RepairableVisualSwitcher : MonoBehaviour
{
    [SerializeField] private GameObject brokenVisual;
    [SerializeField] private GameObject repairedVisual;
    [SerializeField] private Collider2D solidCollider;

    [SerializeField] private GameObject repairedOnlyObject;

    public void SetBroken()
    {
        if (brokenVisual != null)
            brokenVisual.SetActive(true);

        if (repairedVisual != null)
            repairedVisual.SetActive(false);

        if (solidCollider != null)
            solidCollider.enabled = false;

        if (repairedOnlyObject != null)
            repairedOnlyObject.SetActive(false);
    }

    public void SetRepaired()
    {
        if (brokenVisual != null)
            brokenVisual.SetActive(false);

        if (repairedVisual != null)
            repairedVisual.SetActive(true);

        if (solidCollider != null)
            solidCollider.enabled = true;

        if (repairedOnlyObject != null)
            repairedOnlyObject.SetActive(true);
    }
}