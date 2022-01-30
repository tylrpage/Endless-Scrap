using System;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class HordeManager : MonoBehaviour
{
    [SerializeField] private BattleObject robotPrefab;
    [SerializeField] private Transform battleObjectContainer;
    [SerializeField] private float baseRobotSpawnNumber;
    [SerializeField] private float robotSpawnNumberIncreasePerStep;
    
    private List<(int, int)> _spawnPositions;

    private void Awake()
    {
        int width = GameManager.Instance.GridManager.EnemyGrid.GetWidth();
        int height = GameManager.Instance.GridManager.EnemyGrid.GetHeight();
        
        _spawnPositions = new List<(int, int)>();
        for (int i = 0; i < width; i++)
        {
            _spawnPositions.Add((width - 1, i));
            _spawnPositions.Add((0, i));
        }
        for (int i = 0; i < height; i++)
        {
            _spawnPositions.Add((i, height - 1));
            _spawnPositions.Add((i, 0));
        }
    }

    public void OnStep(int step)
    {
        float robotsToSpawnAvg = baseRobotSpawnNumber + (step * robotSpawnNumberIncreasePerStep);
        float remainder = Util.LosePrecision(robotsToSpawnAvg - Mathf.Floor(robotsToSpawnAvg));
        bool addExtra = remainder * 100 > GameManager.Instance.RandomManager.NextInt(100);
        if (addExtra)
        {
            robotsToSpawnAvg += 1;
        }

        int robotsToSpawn = (int) robotsToSpawnAvg;
        for (int i = 0; i < robotsToSpawn; i++)
        {
            var randomSpawn = Util.GetRandomElementFromList(_spawnPositions, GameManager.Instance.RandomManager.NextInt());
            var worldPosition = GameManager.Instance.GridManager.EnemyGrid.GetWorldPosition(randomSpawn.Item1, randomSpawn.Item2);
            BattleObject newBattleObject = Instantiate(robotPrefab, worldPosition, Quaternion.identity, battleObjectContainer);
        }
    }
}
