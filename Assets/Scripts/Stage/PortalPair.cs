using System.Collections;
using UnityEngine;

public class PortalPair : MonoBehaviour
{
    [SerializeField] private RepairableObject portalA;
    [SerializeField] private RepairableObject portalB;

    [SerializeField] private Transform portalAExit;
    [SerializeField] private Transform portalBExit;

    [SerializeField] private float teleportCooldown = 0.5f;

    private bool canTeleport = true;

    public void TeleportFromA(GameObject player)
    {
        if (!CanTeleport())
        {
            return;
        }

        StartCoroutine(
            TeleportRoutine(player, portalBExit.position)
        );
    }

    public void TeleportFromB(GameObject player)
    {
        if (!CanTeleport())
        {
            return;
        }

        StartCoroutine(
            TeleportRoutine(player, portalAExit.position)
        );
    }

    private bool CanTeleport()
    {
        return canTeleport
            && portalA.IsRepaired
            && portalB.IsRepaired;
    }

    private IEnumerator TeleportRoutine(
        GameObject player,
        Vector3 destination
    )
    {
        canTeleport = false;

        player.transform.position = destination;
        PlayPortalSound();

        yield return new WaitForSeconds(teleportCooldown);

        canTeleport = true;
    }

    private void PlayPortalSound()
    {
        SoundManager.Instance?.PlaySE(SEType.Portal);
    }
}
