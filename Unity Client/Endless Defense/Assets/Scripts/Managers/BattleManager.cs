using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    [SerializeField] private float stepsPerSecond;
    [SerializeField] private GameObject battleObjectContainer;
    
    private List<BattleObject> _battleObjects;
    private bool _playing = false;
    private float _lastStepTime;
    private int _stepCount;

    public float SecondsPerStep => Util.LosePrecision(1f / stepsPerSecond);

    private void Awake()
    {
        _battleObjects = new List<BattleObject>();
    }

    private void Update()
    {
        if (_playing)
        {
            float secondsPerStep = 1f / stepsPerSecond;
            float timeSinceLastStep = Time.time - _lastStepTime;
            int stepsToDo = (int) Mathf.Floor(timeSinceLastStep / secondsPerStep);
        
            // We are about to do steps, record when this happens
            if (stepsToDo > 0)
            {
                _lastStepTime = Time.time;
            }
            // Performs the number of steps we are meant to, this will usually be 0, sometimes 1, almost
            // never more
            for (int i = 0; i < stepsToDo; i++)
            {
                StepAll();
            }
        }
        else
        {
            // For debugging purposes
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                StepAll();
            }
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                StartBattle();
            }
        }
    }

    public void StartBattle()
    {
        _lastStepTime = Time.time;
        _playing = true;
    }

    public void AddBattleObject(BattleObject battleObject)
    {
        _battleObjects.Add(battleObject);
    }

    private void StepAll()
    {
        _stepCount++;
        
        foreach (var battleObject in _battleObjects)
        {
            // Step alive battle objects
            if (battleObject.Health > 0)
            {
                battleObject.OnStep();
            }
        }
        
        GameManager.Instance.HordeManager.OnStep(_stepCount);
    }
}
