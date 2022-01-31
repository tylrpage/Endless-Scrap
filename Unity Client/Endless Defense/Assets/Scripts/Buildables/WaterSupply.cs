using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterSupply : Buildable
{
    public static event Action WaterSupplyDied;
    
    [SerializeField] private List<Sprite> waterSupplySprites;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private int _waterLevel;
    private MovingObjectUIWaterSupply _movingObjectUiWaterSupply;
    private int _maxWater;

    protected override void Awake()
    {
        base.Awake();
        
        _movingObjectUiWaterSupply = MovingObjectUI as MovingObjectUIWaterSupply;
        // todo: get this from the level of the building or something
        _maxWater = 100;
        _waterLevel = _maxWater;
        SetWater(_waterLevel);
    }

    public override void Die()
    {
        spriteRenderer.sprite = waterSupplySprites[2];
        WaterSupplyDied?.Invoke();
    }

    public override void ResetHealth()
    {
        base.ResetHealth();
        spriteRenderer.sprite = waterSupplySprites[0];
    }

    public override void StepAction()
    {
        base.StepAction();
        
        // debugging
        //UseWater(10);
        
        // Set water
        //SetWater(_waterLevel);
        
        if (Health <= StartingHealth / 2)
        {
            spriteRenderer.sprite = waterSupplySprites[1];
        }
        else
        {
            spriteRenderer.sprite = waterSupplySprites[0];
        }
    }
    
    public void UseWater(int waterUsage)
    {
        _waterLevel -= waterUsage;
    }
    
    private void SetWater(int water)
    {
        _movingObjectUiWaterSupply.SetWater(water, _maxWater);
    }
}
