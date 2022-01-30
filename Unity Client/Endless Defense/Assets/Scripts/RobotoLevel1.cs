using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class RobotoLevel1 : BattleObject
{
    [SerializeField] private float damage;
    
    private Path _path;
    private Grid<PathfindingNode> _grid;

    protected override void Awake()
    {
        base.Awake();
        
        InitializePath(Vector2.zero);
    }

    public void InitializePath(Vector2 destinationWorldPosition)
    {
        _grid = GameManager.Instance.GridManager.SmallEnemyGrid;
        
        var gridStartPosition = _grid.GetGridObject(transform.position);
        var gridEndPosition = _grid.GetGridObject(destinationWorldPosition);
        Point gridStartPoint = new Point(gridStartPosition.X, gridStartPosition.Y);
        Point gridEndPoint = new Point(gridEndPosition.X, gridEndPosition.Y);
        
        _path = Pathfinding.Pathfinding.GetDirectPath(_grid, gridStartPoint, gridEndPoint);
        
        GameManager.Instance.GridManager.DrawPath(_path);
    }
    
    public override void Step()
    {
        if (_path != null && _path.Current != null)
        {
            var obstacleGrid = GameManager.Instance.BuildManager.BuildablesGrid;
            if (Pathfinding.Pathfinding.IsPathBlocked(_grid, obstacleGrid, _path, out Node<Buildable> obstacleNode))
            {
                // Path is blocked, attacking what is blocking you
                obstacleNode.Data.TakeDamage(damage);
            }
            else
            {
                // Path isnt blocked, move on through
                _path.Current = _path.Current.PathNextPathfindingNode;
                if (_path.Current != null)
                {
                    Vector2 nextWorldPosition = _grid.GetWorldPositionOfCenter(_path.Current.X, _path.Current.Y);
                    transform.position = nextWorldPosition;
                }
            }
        }
    }
}
