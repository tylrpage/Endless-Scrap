using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleObject : MonoBehaviour
{
    [SerializeField] private float startingHealth;

    private float _health;

    protected virtual void Awake()
    {
        _health = startingHealth;
    }

    public virtual void Step()
    {
        
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
}
