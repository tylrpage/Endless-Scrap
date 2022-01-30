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

    public int Health => _health;
    // todo: have robot pathfinding use this size
    public Vector2 Size => size;

    private int _health;
    private MovingObjectUI _movingObjectUI;

    protected virtual void Awake()
    {
        _health = startingHealth;
        
        Vector3 offset = Vector3.up * (size.y / 2);
        _movingObjectUI = GameManager.Instance.MovingObjectUIManager.CreateMovingObject(this, movingObjectUIPrefabOverride, offset, size.x);
        _movingObjectUI.SetHealth(_health, startingHealth);
    }

    public virtual void Step()
    {
    }

    public virtual void Die()
    {
        Destroy(gameObject);
        GameManager.Instance.MovingObjectUIManager.RemoveMovingObject(this);
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
}
