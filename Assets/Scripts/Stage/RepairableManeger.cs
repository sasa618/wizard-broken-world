using System.Collections.Generic;
using UnityEngine;

public class RepairableManager : MonoBehaviour
{
    public static RepairableManager Instance { get; private set; }

    [SerializeField] private int maxSimultaneousRepairedObjects = 2;

    private readonly Queue<RepairableObject> repairedQueue = new Queue<RepairableObject>();
    private readonly HashSet<RepairableObject> queuedRepairables = new HashSet<RepairableObject>();

    private int totalCount;
    private int repairedCount;

    public int TotalCount => totalCount;
    public int RepairedCount => repairedCount;
    public int CurrentRepairedCount => repairedQueue.Count;
    public int RemainingCount => totalCount - CurrentRepairedCount;

    private int repairableLayer;

    private void Awake()
    {
        Instance = this;

        repairableLayer = LayerMask.NameToLayer("Repairable");
    }

    private void Start()
    {
        CountRepairableObjects();
    }

    private void CountRepairableObjects()
    {
        RepairableObject[] objects =
            FindObjectsByType<RepairableObject>(
                FindObjectsSortMode.None
            );

        totalCount = 0;

        foreach (RepairableObject obj in objects)
        {
            if (obj.gameObject.layer == repairableLayer)
            {
                totalCount++;
            }
        }

        Debug.Log($"修復可能オブジェクト数: {totalCount}");
    }

    public void NotifyRepaired(RepairableObject repairedObject)
    {
        if (repairedObject == null || repairedObject.gameObject.layer != repairableLayer)
        {
            return;
        }

        if (queuedRepairables.Contains(repairedObject))
        {
            return;
        }

        repairedCount++;
        repairedQueue.Enqueue(repairedObject);
        queuedRepairables.Add(repairedObject);

        EnforceRepairLimit();

        Debug.Log($"Repaired total: {repairedCount} / {totalCount}");

        if (repairedCount >= totalCount)
        {
            Debug.Log("All repairable objects have been repaired at least once.");
        }
    }

    private void EnforceRepairLimit()
    {
        int limit = Mathf.Max(0, maxSimultaneousRepairedObjects);

        while (repairedQueue.Count > limit)
        {
            RepairableObject oldestObject = repairedQueue.Dequeue();
            queuedRepairables.Remove(oldestObject);

            if (oldestObject != null && oldestObject.IsRepaired)
            {
                oldestObject.Break();
            }
        }
    }

    public int GetRepairedCount()
    {
        return repairedCount;
    }

}
