using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TurretLevel1 : Buildable
{
    [SerializeField] private int damage;
    [SerializeField] private int range;

    private List<(int, int)> _tilesInRange;
    private BattleObject _target;

    protected override void Awake()
    {
        base.Awake();

        _tilesInRange = GetTilesInRange(range);
        GameManager.Instance.GridManager.DrawTiles(GameManager.Instance.BuildManager.BuildablesGrid, _tilesInRange);
    }

    public override void StepAction()
    {
        if (_target != null)
        {
            // Shoot the target
            _target.TakeDamage(damage);
        }
        else
        {
            _target = FindNearestEnemyInRange();
        }
    }

    private BattleObject FindNearestEnemyInRange()
    {
        foreach (var tileInRange in _tilesInRange)
        {
            float cellSize = GameManager.Instance.BuildManager.BuildablesGrid.GetCellSize();
            var tiles = GameManager.Instance.GridManager.SmallEnemyPathfindingGrid.GetTilesFromAnotherTileSize(tileInRange, cellSize);
            foreach (var tile in tiles)
            {
                var enemiesNode = GameManager.Instance.GridManager.EnemyGrid.GetGridObject(tile.Item1, tile.Item2);
                if (enemiesNode.Data.Count > 0)
                {
                    return enemiesNode.Data.First();
                }
            }
        }
        
        return null;
    }

    private List<(int, int)> GetTilesInRange(int range)
    {
        GameManager.Instance.BuildManager.BuildablesGrid.GetXY(transform.position, out int x, out int y);
        var positionOnGrid = GameManager.Instance.BuildManager.BuildablesGrid.GetWorldPosition(x, y);
        HashSet<(int, int)> checkedPositions = new HashSet<(int, int)>();
        List<(int, int)> tilePositionsInRange = new List<(int, int)>();
        Queue<(int, int)> tilePositionsToCheck = new Queue<(int, int)>();
        tilePositionsToCheck.Enqueue((x, y));
        while (tilePositionsToCheck.Count > 0)
        {
            (int, int) gridPosition = tilePositionsToCheck.Dequeue();
            Vector3 worldPosition =
                GameManager.Instance.BuildManager.BuildablesGrid.GetWorldPosition(gridPosition.Item1,
                    gridPosition.Item2);
            // Check the position
            float distance = Mathf.Abs((positionOnGrid - worldPosition).magnitude);
            checkedPositions.Add(gridPosition);
            if (distance <= range)
            {
                tilePositionsInRange.Add(gridPosition);
                // Add its neighbors to be checked
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        (int, int) newPosition = (gridPosition.Item1 + i, gridPosition.Item2 + j);
                        bool isValidPositionOnGrid =
                            GameManager.Instance.BuildManager.BuildablesGrid.IsValidPosition(newPosition.Item1,
                                newPosition.Item2);
                        // Only add positions that are valid in the grid and we havent checked before
                        if (isValidPositionOnGrid && !checkedPositions.Contains(newPosition))
                        {
                            tilePositionsToCheck.Enqueue(newPosition);
                        }
                    }
                }
            }
        }

        return tilePositionsInRange;
    }
}
