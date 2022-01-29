using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buildable : BattleObject
{
    [SerializeField] private BuildManager.Currencies cost;
    [SerializeField] private Vector2 size;
    [SerializeField] private GridIndicator gridIndicator;

    public Sprite Sprite => GetComponent<SpriteRenderer>()?.sprite;
    public Vector2 Size => size;
    public GridIndicator GridIndicator => gridIndicator;

    protected override void Awake()
    {
        base.Awake();
        gridIndicator.SetSize(size);
        gridIndicator.SetActive(false);
    }

    public BuildManager.Currencies GetCost()
    {
        return cost;
    }
}
