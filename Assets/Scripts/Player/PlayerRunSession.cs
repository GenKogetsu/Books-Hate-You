using System.Collections.Generic;
using System;
using UnityEngine;

/// <summary>
/// Tracks run-scoped state shared by scenes. A new run clears both collected
/// pickups and the runtime inventory without changing the inventory asset.
/// </summary>
public static class PlayerRunSession
{
    public const string WinAchievementId = "win_game";
    public const string BookKillAchievementId = "kill_librarian_with_book";
    public const string AllKeysAchievementId = "collect_all_keys";
    public const string ClearAllFloorEnemiesAchievementId = "clear_all_floor_enemies";
    public const string WinWithoutDyingAchievementId = "win_without_dying";

    private static readonly HashSet<string> CollectedPickupIds = new();
    private static readonly HashSet<string> DefeatedFloorEnemyIds = new();
    private static int _deathCount;
    private static int _collectedKeyCount;
    private static int _librariansKilledWithBooks;
    private static bool _hasWon;
    private static bool _hasCollectedAllKeys;
    private static bool _hasClearedAllFloorEnemies;
    private static bool _hasWonWithoutDying;

    public static int DeathCount => _deathCount;
    public static event Action<int> DeathCountChanged;
    public static event Action<string> AchievementUnlocked;
    public static event Action<EnemyHpController> FloorEnemyDefeated;
    public static bool HasWon => _hasWon;
    public static bool HasKilledLibrarianWithBook => _librariansKilledWithBooks > 0;
    public static bool HasCollectedAllKeys => _hasCollectedAllKeys;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetForNewPlaySession()
    {
        CollectedPickupIds.Clear();
        DefeatedFloorEnemyIds.Clear();
        _deathCount = 0;
        DeathCountChanged = null;
        _collectedKeyCount = 0;
        _librariansKilledWithBooks = 0;
        _hasWon = false;
        _hasCollectedAllKeys = false;
        _hasClearedAllFloorEnemies = false;
        _hasWonWithoutDying = false;
        AchievementUnlocked = null;
        FloorEnemyDefeated = null;
    }

    public static void RecordWin()
    {
        if (_hasWon)
            return;

        _hasWon = true;
        AchievementUnlocked?.Invoke(WinAchievementId);

        if (_deathCount == 0 && !_hasWonWithoutDying)
        {
            _hasWonWithoutDying = true;
            AchievementUnlocked?.Invoke(WinWithoutDyingAchievementId);
        }
    }

    public static void RecordLibrarianKilledWithBook()
    {
        if (_librariansKilledWithBooks > 0)
            return;

        _librariansKilledWithBooks = 1;
        AchievementUnlocked?.Invoke(BookKillAchievementId);
    }

    public static void RecordKeyCollected(int amount)
    {
        if (amount <= 0)
            return;

        _collectedKeyCount = Mathf.Min(5, _collectedKeyCount + amount);
        if (_collectedKeyCount >= 5 && !_hasCollectedAllKeys)
        {
            _hasCollectedAllKeys = true;
            AchievementUnlocked?.Invoke(AllKeysAchievementId);
        }
    }

    public static void RecordDeath()
    {
        _deathCount++;
        DeathCountChanged?.Invoke(_deathCount);
    }

    public static void NotifyFloorEnemyDefeated(EnemyHpController enemy)
    {
        if (enemy)
            FloorEnemyDefeated?.Invoke(enemy);
    }

    public static void RecordFloorEnemyDefeated(string enemyId, int totalFloorEnemies)
    {
        if (string.IsNullOrEmpty(enemyId) || !DefeatedFloorEnemyIds.Add(enemyId))
            return;

        if (!_hasClearedAllFloorEnemies && DefeatedFloorEnemyIds.Count >= Mathf.Max(1, totalFloorEnemies))
        {
            _hasClearedAllFloorEnemies = true;
            AchievementUnlocked?.Invoke(ClearAllFloorEnemiesAchievementId);
        }
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
        _collectedKeyCount = 0;

        foreach (PlayerInventoryData inventory in Resources.FindObjectsOfTypeAll<PlayerInventoryData>())
        {
            if (inventory)
                inventory.ClearRuntimeItems();
        }
    }
}
