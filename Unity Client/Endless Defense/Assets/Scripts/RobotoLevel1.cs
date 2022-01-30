using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Pathfinding;
using UnityEngine;

public class RobotoLevel1 : BattleObject
{
    [SerializeField] private float damage;
    [SerializeField] private AnimationCurve walkCurve;
    // 1 means it takes the full time to get to next step, 2 means it takes half the time
    [SerializeField] private float walkTime;
    
    private Path _path;
    private Grid<PathfindingNode> _grid;
    // The grid we have moved into, this can vary from the path
    private (int, int) _gridPosition;
    private Vector3? _targetPosition;
    private Vector3? _previousTargetPosition;
    private float _timeWalking;

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
        _previousTargetPosition = _grid.GetWorldPositionOfCenter(_path.Start.X, _path.Start.Y);
        
        // debugging
        GameManager.Instance.GridManager.DrawPath(_path);
    }

    private void Update()
    {
        // Interpolate to our target position
        _timeWalking += Time.deltaTime;
        if (_targetPosition != null && _previousTargetPosition != null)
        {
            float t = walkCurve.Evaluate(_timeWalking * walkTime);
            transform.position = Vector3.Lerp((Vector3)_previousTargetPosition, (Vector3)_targetPosition, t);
        }
    }

    public override void Step()
    {
        // To step we need a path, we need to know where we currently are, and somewhere we are going
        if (_path != null && _path.Current != null && _path.Current.PathNextPathfindingNode != null)
        {
            var obstacleGrid = GameManager.Instance.BuildManager.BuildablesGrid;
            
            // The grid we are in according according to our grid position, this can vary from the path
            // for example when we are attaking something diagonal from us and need to move up or to the side
            Point gridPositionPoint = new Point(_gridPosition.Item1, _gridPosition.Item2);
            
            Point destinationGridPosition = new Point(_path.Current.PathNextPathfindingNode.X, _path.Current.PathNextPathfindingNode.Y);
            if (Pathfinding.Pathfinding.IsPathBlocked(_grid, obstacleGrid, gridPositionPoint, destinationGridPosition, out Node<Buildable> obstacleNode, out bool directlyDiagonal))
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
        _gridPosition = (x, y);
        Vector2 nextWorldPosition = _grid.GetWorldPositionOfCenter(x, y);
        _previousTargetPosition = transform.position;
        _targetPosition = nextWorldPosition;
        _timeWalking = 0;
    }
}
