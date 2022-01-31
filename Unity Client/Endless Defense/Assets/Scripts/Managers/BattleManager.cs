using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    [SerializeField] private float stepsPerSecond;
    [SerializeField] private GameObject battleObjectContainer;
    [SerializeField] private RectTransform scrapCounter;
    [SerializeField] private GameObject rebuildPanel;
    [SerializeField] private WaterSupply waterSupply;
    
    private List<BattleObject> _battleObjects;
    private bool _playing = false;
    private float _lastStepTime;
    private int _stepCount;

    public float SecondsPerStep => Util.LosePrecision(1f / stepsPerSecond);

    private void Awake()
    {
        _battleObjects = new List<BattleObject>();
        GameManager.Instance.BuildManager.EnableBuilding();
        WaterSupply.WaterSupplyDied += WaterSupplyOnWaterSupplyDied;
        GameManager.Instance.MusicManager.PlayBuildMusic();
    }

    private void WaterSupplyOnWaterSupplyDied()
    {
        _playing = false;
        rebuildPanel.SetActive(true);
    }

    private void Update()
    {
        if (_playing)
        {
            // For debugging purposes
            if (Input.GetKeyDown(KeyCode.E))
            {
                EndBattle();
                _playing = false;
                return;
            }
            
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
        GameManager.Instance.BuildManager.DisableBuilding();
        _lastStepTime = Time.time;
        _playing = true;
        GameManager.Instance.MusicManager.PlayBattleMusic();
    }

    public void AddBattleObject(BattleObject battleObject)
    {
        _battleObjects.Add(battleObject);
    }

    public void RemoveBattleObject(BattleObject battleObject)
    {
        _battleObjects.Remove(battleObject);
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

    public void EndBattle()
    {
        waterSupply.ResetHealth();
        rebuildPanel.SetActive(false);
        GameManager.Instance.MusicManager.PlayBuildMusic();
        
        GameManager.Instance.BuildManager.EnableBuilding();
        Vector3 scrapCounterWorldPosition = scrapCounter.transform.position;
        foreach (var deadRobot in GameManager.Instance.GridManager.DeadRobots)
        {
            StartCoroutine(deadRobot.FlyScrap(scrapCounterWorldPosition, OnComplete));
        }
        GameManager.Instance.GridManager.DeadRobots.Clear();
        void OnComplete()
        {
            GameManager.Instance.BuildManager.AddScrap(10);
            // todo: play sound
        }

        GameManager.Instance.GridManager.ClearEnemyGrid();
    }
}
