using UnityEngine;

[DisallowMultipleComponent]
public class CameraFollowX : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private PlayerSpawner playerSpawner;

    [Header("追従範囲（ステージ端で床が見切れないようにする）")]
    [SerializeField] private bool clampX;
    [SerializeField] private float minX;
    [SerializeField] private float maxX;

    private float fixedY;
    private float fixedZ;

    private void Start()
    {
        Vector3 startPosition = transform.position;
        fixedY = startPosition.y;
        fixedZ = startPosition.z;

        ResolveTargetFromSpawner();
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            ResolveTargetFromSpawner();
        }

        if (target == null)
        {
            return;
        }

        float newX = target.position.x;
        if (clampX)
        {
            newX = Mathf.Clamp(newX, minX, maxX);
        }

        transform.position = new Vector3(
            newX,
            fixedY,
            fixedZ
        );
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    private void ResolveTargetFromSpawner()
    {
        if (playerSpawner == null)
        {
            return;
        }

        target = playerSpawner.SpawnedPlayerTransform;
    }
}
