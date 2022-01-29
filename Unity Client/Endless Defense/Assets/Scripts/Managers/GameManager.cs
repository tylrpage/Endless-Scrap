using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public BattleManager BattleManager;
    public BuildManager BuildManager;
    public GridManager GridManager;
    
    private static GameManager _instance;
    public static GameManager Instance => _instance;
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }
}
