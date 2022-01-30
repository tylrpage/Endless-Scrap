using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleObject : MonoBehaviour
{
    [SerializeField] private float startingHealth;
    [SerializeField] private MovingObjectUI movingObjectUIPrefabOverride;
    [SerializeField] private Vector2 size;

    public float Health => _health;
    // todo: have robot pathfinding use this size
    public Vector2 Size => size;

    private float _health;
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

    public void TakeDamage(float damage)
    {
        _health = Util.LosePrecision(_health -= damage);
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
