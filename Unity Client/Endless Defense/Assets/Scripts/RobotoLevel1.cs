using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        // To step we need a path, we need to know where we currently are, and somewhere we are going
        if (_path != null && _path.Current != null && _path.Current.PathNextPathfindingNode != null)
        {
            var obstacleGrid = GameManager.Instance.BuildManager.BuildablesGrid;
            
            // The grid we are in according to our world position, this can vary from the path
            // for example when we are attaking something diagonal from us and need to move up or to the side
            PathfindingNode worldPositionNode = _grid.GetGridObject(transform.position);
            Point actualGridPosition = new Point(worldPositionNode.X, worldPositionNode.Y);
            
            Point destinationGridPosition = new Point(_path.Current.PathNextPathfindingNode.X, _path.Current.PathNextPathfindingNode.Y);
            if (Pathfinding.Pathfinding.IsPathBlocked(_grid, obstacleGrid, actualGridPosition, destinationGridPosition, out Node<Buildable> obstacleNode, out bool directlyDiagonal))
            {
                if (directlyDiagonal)
                {
                    var moveHorizontal = (_path.Current.X + 1, _path.Current.Y);
                    var moveVertical = (_path.Current.X, _path.Current.Y + 1);
                    List<(int, int)> newGridPositionOptions = new List<(int, int)>()
                    {
                        moveHorizontal, moveVertical
                    };
                    // Remove invalid positions (out of bounds)
                    newGridPositionOptions.Where(x => _grid.IsValidPosition(x.Item1, x.Item2));
                    int randomInt = GameManager.Instance.RandomManager.NextInt();
                    var randomChoice = Util.GetRandomElementFromList(newGridPositionOptions, randomInt);
                    MoveToGridPosition(randomChoice.Item1, randomChoice.Item2);
                }
                else
                {
                    // Path is blocked by non diagonal thing, attack what is blocking you
                    obstacleNode.Data.TakeDamage(damage);
                }
            }
            else
            {
                // Path isnt blocked, move on through
                _path.Current = _path.Current.PathNextPathfindingNode;
                if (_path.Current != null)
                {
                    MoveToGridPosition(_path.Current.X, _path.Current.Y);
                }
            }
        }
    }

    private void MoveToGridPosition(int x, int y)
    {
        Vector2 nextWorldPosition = _grid.GetWorldPositionOfCenter(x, y);
        transform.position = nextWorldPosition;
    }
}
