using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Pathfinding;
using UnityEngine;
using Random = UnityEngine.Random;

public class RobotoLevel1 : BattleObject
{
    [SerializeField] private int damage;
    [SerializeField] private AnimationCurve walkCurve;
    // 1 means it takes the full time to get to next step, 2 means it takes half the time
    [SerializeField] private float walkTime;
    [SerializeField] private RobotAnimationController robotAnimationController;
    [SerializeField] private GameObject deathScrap;
    [SerializeField] private AnimationCurve deathScrapCurve;
    
    private Path _path;

    private Grid<EnemiesNode> _enemyGrid;
    // The grid we have moved into, this can vary from the path
    private (int, int) _gridPosition;
    private Vector3 _targetPosition;
    private Vector3 _previousTargetPosition;
    private Vector3? _interpStart;
    private float _timeWalking;
    private bool _isAttacking;
    private bool _died;

    protected override void Awake()
    {
        base.Awake();

        _enemyGrid = GameManager.Instance.GridManager.EnemyGrid;
        GameManager.Instance.GridManager.AddEnemyToGrid(_enemyGrid, this, transform.position);
        GameManager.Instance.GridManager.EnemyRobots.Add(this);
        
        InitializePath(Vector2.zero);
    }

    public override void Die()
    {
        if (!_died)
        {
            _died = true;
            
            robotAnimationController.ChangeAnimationState("vanish");
            GameManager.Instance.GridManager.RemoveEnemyFromGrid(_enemyGrid, this, transform.position);
            GameManager.Instance.GridManager.DeadRobots.Add(this);
            GameManager.Instance.GridManager.EnemyRobots.Remove(this);

            StartCoroutine(SpawnScrapAfterTime());
        }
    }

    public IEnumerator FlyScrap(Vector3 position, Action complete)
    {
        float deathScrapT = 0;
        float randomSpeed = Random.Range(0.8f, 1.2f);
        while (deathScrapT < 1)
        {
            deathScrapT += Time.deltaTime * randomSpeed;
            float lerpT = deathScrapCurve.Evaluate(deathScrapT);
            deathScrap.transform.position = Vector3.Lerp(transform.position, position, lerpT);
            
            yield return null;
        }
        complete?.Invoke();
    }

    IEnumerator SpawnScrapAfterTime()
    {
        yield return new WaitForSeconds(2f);
        deathScrap.SetActive(true);
        MovingObjectUI.gameObject.SetActive(false);
    }

    public void InitializePath(Vector2 destinationWorldPosition)
    {
        Point gridStartPoint = new Point();
        Point gridEndPoint = new Point();
        _enemyGrid.GetXY(transform.position, out gridStartPoint.X, out gridStartPoint.Y);
        _enemyGrid.GetXY(destinationWorldPosition, out gridEndPoint.X, out gridEndPoint.Y);
        
        _path = Pathfinding.Pathfinding.GetDirectPath(_enemyGrid, gridStartPoint, gridEndPoint);
        _interpStart = _enemyGrid.GetWorldPositionOfCenter(_path.Start.X, _path.Start.Y);
        _targetPosition = _enemyGrid.GetWorldPositionOfCenter(_path.Start.X, _path.Start.Y);
        
        // debugging
        //GameManager.Instance.GridManager.DrawPath(_path);
    }

    private void Update()
    {
        // Interpolate to our target position
        _timeWalking += Time.deltaTime;
        if (_targetPosition != null && _interpStart != null && Health > 0)
        {
            float t = walkCurve.Evaluate(_timeWalking * walkTime);
            transform.position = Vector3.Lerp((Vector3)_interpStart, (Vector3)_targetPosition, t);
            
            if (!_isAttacking)
            {
                robotAnimationController.SetAnimation(RobotAnimationController.AnimationType.walk, _targetPosition - (Vector3)_interpStart);
            }
        }
    }

    public override void StepAction()
    { 
        // To step we need a path, we need to know where we currently are, and somewhere we are going
        if (_path != null && _path.Current != null && _path.Current.PathNextPathfindingNode != null)
        {
            var obstacleGrid = GameManager.Instance.BuildManager.BuildablesGrid;
            
            // The grid we are in according according to our grid position, this can vary from the path
            // for example when we are attaking something diagonal from us and need to move up or to the side
            Point gridPositionPoint = new Point(_gridPosition.Item1, _gridPosition.Item2);
            
            Point destinationTilePosition = new Point(_path.Current.PathNextPathfindingNode.X, _path.Current.PathNextPathfindingNode.Y);
            if (Pathfinding.Pathfinding.IsPathBlocked(_enemyGrid, obstacleGrid, gridPositionPoint, destinationTilePosition, out Node<Buildable> obstacleNode, out var blockDirection))
            {
                bool directlyDiagonal = blockDirection.x != 0 && blockDirection.y != 0;
                if (directlyDiagonal)
                {
                    var moveHorizontal = (_path.Current.X + 1, _path.Current.Y);
                    var moveVertical = (_path.Current.X, _path.Current.Y + 1);
                    List<(int, int)> newGridPositionOptions = new List<(int, int)>()
                    {
                        moveHorizontal, moveVertical
                    };
                    // Remove invalid positions (out of bounds)
                    newGridPositionOptions.Where(x => _enemyGrid.IsValidPosition(x.Item1, x.Item2));
                    int randomInt = GameManager.Instance.RandomManager.NextInt();
                    var randomChoice = Util.GetRandomElementFromList(newGridPositionOptions, randomInt);
                    MoveToGridPosition(randomChoice.Item1, randomChoice.Item2);
                }
                else
                {
                    // Path is blocked by non diagonal thing, attack what is blocking you
                    obstacleNode.Data.TakeDamage(damage);
                    _isAttacking = true;
                    //Vector3 attackDirection = new Vector3(destinationTilePosition.X - gridPositionPoint.X, destinationTilePosition.Y - gridPositionPoint.Y);
                    Vector3 attackDirection = new Vector3(blockDirection.x, blockDirection.y);
                    robotAnimationController.SetAnimation(RobotAnimationController.AnimationType.hit2, attackDirection);
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
        Vector2 nextWorldPosition = _enemyGrid.GetWorldPositionOfCenter(x, y);
        _interpStart = transform.position;
        _previousTargetPosition = _targetPosition;
        _targetPosition = nextWorldPosition;
        _timeWalking = 0;
        _isAttacking = false;
        
        GameManager.Instance.GridManager.MoveEnemyOnGrid(_enemyGrid, this, _previousTargetPosition, _targetPosition);
    }
}
