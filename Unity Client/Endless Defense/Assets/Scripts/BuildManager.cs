using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildManager : MonoBehaviour
{
    public enum BuildingType
    {
        TurrentLevel1
    }

    [Serializable]
    public struct Currencies
    {
        public int Scrap;

        public static Currencies operator +(Currencies a, Currencies b)
        {
            return new Currencies()
            {
                Scrap = a.Scrap + b.Scrap
            };
        }

        public static Currencies operator -(Currencies a, Currencies b)
        {
            return new Currencies()
            {
                Scrap = a.Scrap - b.Scrap
            };
        }

        public static bool operator >(Currencies a, Currencies b)
        {
            return a.Scrap > b.Scrap;
        }

        public static bool operator <(Currencies a, Currencies b)
        {
            return a.Scrap < b.Scrap;
        }
        
        public static bool operator ==(Currencies a, Currencies b)
        {
            return a.Scrap == b.Scrap;
        }
        
        public static bool operator !=(Currencies a, Currencies b)
        {
            return a.Scrap != b.Scrap;
        }
        
        public static bool operator >=(Currencies a, Currencies b)
        {
            return a.Scrap >= b.Scrap;
        }

        public static bool operator <=(Currencies a, Currencies b)
        {
            return a.Scrap <= b.Scrap;
        }
    }

    public static event Action<Currencies> CurrenciesUpdated;

    [SerializeField] private SerializableDictionary<BuildingType, Buildable> buildingPrefabs;
    [SerializeField] private BuildingIndicator buildingIndicator;
    [SerializeField] private Transform buildingContainer;
    
    private Currencies _currencies = new Currencies()
    {
        Scrap = 50
    };
    private BuildingType? _selectedBuildingType = null;
    private Camera _camera;

    private void Awake()
    {
        UpdateCurrencies();
        buildingIndicator.SetActive(false);
        _camera = Camera.main;
    }

    public void OnBuildButtonClicked(BuildingTypeComponent buildingType)
    {
        if (buildingPrefabs.TryGetValue(buildingType.BuildingType, out Buildable buildable))
        {
            _selectedBuildingType = buildingType.BuildingType;
            buildingIndicator.SetBuilding(buildable);
            buildingIndicator.SetActive(true);
        }
        else
        {
            Debug.LogError($"Attempted to build a building type that isn't in buildingPrefabs, building type: {buildingType.BuildingType}");
        }
        
    }

    private void Update()
    {
        if (_selectedBuildingType != null && _camera != null)
        {
            // Snap to grid under mouse
            Vector2 mouseWorldPosition = _camera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 snappedPosition = new Vector2(Mathf.Round(mouseWorldPosition.x), Mathf.Round(mouseWorldPosition.y));
            buildingIndicator.transform.position = snappedPosition;
            
            if (Input.GetMouseButtonDown(0))
            {
                BuildingType selectedBuildingType = (BuildingType)_selectedBuildingType;
                if (buildingPrefabs.TryGetValue(selectedBuildingType, out Buildable buildable))
                {
                    // Check if we can afford it
                    Currencies cost = buildable.GetCost();
                    if (_currencies >= cost)
                    {
                        _currencies = _currencies - cost;
                        UpdateCurrencies();
                        buildingIndicator.SetActive(false);
                        _selectedBuildingType = null;
                    
                        // Create the building
                        Instantiate(buildable, snappedPosition, Quaternion.identity, buildingContainer);
                    }
                    else
                    {
                        Debug.Log("Cannot afford!");
                    }
                }
                else
                {
                    Debug.LogError($"Attempted to build a building type that isn't in buildingPrefabs, building type: {selectedBuildingType}");
                }
            }
        }
    }

    private void UpdateCurrencies()
    {
        CurrenciesUpdated?.Invoke(_currencies);
    }
}
