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

    private static BattleManager _instance;
    public static BattleManager Instance => _instance;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
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
                if (_battleObjects == null)
                {
                    GetBattleObjects();
                }
                
                StepAll();
            }
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                StartBattle();
            }
        }
    }

    private void GetBattleObjects()
    {
        _battleObjects = battleObjectContainer.GetComponentsInChildren<BattleObject>().ToList();
    }

    public void StartBattle()
    {
        GetBattleObjects();
        _lastStepTime = Time.time;
        _playing = true;
    }

    private void StepAll()
    {
        Debug.Log("Stepping");
        foreach (var battleObject in _battleObjects)
        {
            battleObject.Step();
        }
    }
}
