using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterSupply : Buildable
{
    [SerializeField] private List<Sprite> waterSupplySprites;

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
    
    public override void StepAction()
    {
        base.StepAction();
        
        // debugging
        UseWater(10);
        
        // Set water
        SetWater(_waterLevel);
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
