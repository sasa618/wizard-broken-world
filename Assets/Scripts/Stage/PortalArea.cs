using UnityEngine;

public class PortalArea : MonoBehaviour
{
    public enum PortalType
    {
        A,
        B
    }

    [SerializeField] private PortalType portalType;
    [SerializeField] private PortalPair portalPair;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (portalType == PortalType.A)
        {
            portalPair.TeleportFromA(other.gameObject);
        }
        else
        {
            portalPair.TeleportFromB(other.gameObject);
        }
    }
}