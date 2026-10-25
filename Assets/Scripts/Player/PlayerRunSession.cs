using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tracks run-scoped state shared by scenes. A new run clears both collected
/// pickups and the runtime inventory without changing the inventory asset.
/// </summary>
public static class PlayerRunSession
{
    private static readonly HashSet<string> CollectedPickupIds = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetForNewPlaySession()
    {
        CollectedPickupIds.Clear();
    }

    public static bool IsPickupCollected(string pickupId)
    {
        return !string.IsNullOrEmpty(pickupId) && CollectedPickupIds.Contains(pickupId);
    }

    public static void MarkPickupCollected(string pickupId)
    {
        if (!string.IsNullOrEmpty(pickupId))
            CollectedPickupIds.Add(pickupId);
    }

    public static void ResetRun()
    {
        CollectedPickupIds.Clear();

        foreach (PlayerInventoryData inventory in Resources.FindObjectsOfTypeAll<PlayerInventoryData>())
        {
            if (inventory)
                inventory.ClearRuntimeItems();
        }
    }
}
