using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleObject : MonoBehaviour
{
    [SerializeField] private int startingHealth;
    [SerializeField] private MovingObjectUI movingObjectUIPrefabOverride;
    [SerializeField] private Vector2 size;
    [SerializeField] private bool isEnemy;
    [SerializeField] private int speed;
    [SerializeField] private float timeFromDeathToDestroy;

    public int Health => _health;

    public int StartingHealth => startingHealth;

    // todo: have robot pathfinding use this size
    public Vector2 Size => size;

    public MovingObjectUI MovingObjectUI => _movingObjectUI;

    private int _health;
    private MovingObjectUI _movingObjectUI;
    private int _stepsSinceAction;

    protected virtual void Awake()
    {
        _health = startingHealth;
        
        Vector3 offset = Vector3.up * size.y;
        _movingObjectUI = GameManager.Instance.MovingObjectUIManager.CreateMovingObject(this, movingObjectUIPrefabOverride, offset, size.x);
        _movingObjectUI.SetHealth(_health, startingHealth);
        
        GameManager.Instance.BattleManager.AddBattleObject(this);
    }

    public virtual void StepAction()
    {
    }

    public void OnStep()
    {
        // Check if it is time for this object to step
        _stepsSinceAction++;
        if (_stepsSinceAction >= speed && _health > 0)
        {
            _stepsSinceAction = 0;
            StepAction();
        }
    }

    public virtual void Die()
    {
        StartCoroutine(WaitToCleanup());
    }

    private IEnumerator WaitToCleanup()
    {
        yield return new WaitForSeconds(timeFromDeathToDestroy);
        CleanUp();
    }

    public void CleanUp()
    {
        GameManager.Instance.MovingObjectUIManager.RemoveMovingObject(this);
        Destroy(gameObject);
    }

    public Vector2 GetGridPosition()
    {
        return transform.position;
    }

    public void SetPosition(Vector2 gridPosition)
    {
        transform.position = gridPosition;
    }

    public void MoveInDirection(Vector2 direction)
    {
        Vector2 newPosition = GetGridPosition() + direction;
        SetPosition(newPosition);
    }

    public void TakeDamage(int damage)
    {
        _health -= damage;
        if (_movingObjectUI != null)
        {
            _movingObjectUI.SetHealth(_health, startingHealth);
        }
        
        if (_health <= 0)
        {
            Die();
        }
    }

    public virtual void ResetHealth()
    {
        _health = startingHealth;
        _movingObjectUI.SetHealth(_health, startingHealth);
    }
}
