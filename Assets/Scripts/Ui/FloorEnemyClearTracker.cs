using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class FloorEnemyClearTracker : MonoBehaviour
{
    [SerializeField, Min(1)] private int _totalFloorEnemies = 8;

    private readonly HashSet<string> _sceneEnemyIds = new();
    private Scene _floorScene;

    private void OnEnable()
    {
        _floorScene = gameObject.scene;
        if (!_floorScene.IsValid() || !_floorScene.name.StartsWith("Floor", StringComparison.OrdinalIgnoreCase))
            return;

        _sceneEnemyIds.Clear();
        foreach (GameObject root in _floorScene.GetRootGameObjects())
        {
            foreach (EnemyHpController enemy in root.GetComponentsInChildren<EnemyHpController>(true))
                _sceneEnemyIds.Add(BuildEnemyId(enemy.transform));
        }

        PlayerRunSession.FloorEnemyDefeated += OnFloorEnemyDefeated;
    }

    private void OnDisable()
    {
        PlayerRunSession.FloorEnemyDefeated -= OnFloorEnemyDefeated;
    }

    private void OnFloorEnemyDefeated(EnemyHpController enemy)
    {
        if (!enemy || enemy.gameObject.scene != _floorScene)
            return;

        string enemyId = BuildEnemyId(enemy.transform);
        if (_sceneEnemyIds.Contains(enemyId))
            PlayerRunSession.RecordFloorEnemyDefeated(enemyId, _totalFloorEnemies);
    }

    private static string BuildEnemyId(Transform enemy)
    {
        string id = enemy.gameObject.scene.path;
        for (Transform current = enemy; current; current = current.parent)
            id = $"{current.GetSiblingIndex()}:{current.name}/{id}";

        return id;
    }
}
