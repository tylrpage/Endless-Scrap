using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;

public class BuildManager : MonoBehaviour
{
    public enum BuildingType
    {
        TurretLevel1, TurretLevel2, TurretLevel3
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
    [SerializeField] private float gridsPerUnit;
    
    private Currencies _currencies = new Currencies()
    {
        Scrap = 50
    };
    private Buildable _selectedBuilding;
    private Camera _camera;

    private (uint, uint) _gridSize;
    private Buildable[,] _placedBuildings;
    private HashSet<Buildable> _previouslyOverlappedBuildings = new HashSet<Buildable>();

    private void Awake()
    {
        UpdateCurrencies();
        buildingIndicator.SetActive(false);
        _camera = Camera.main;

        InitializeGrid(11, 11);
    }

    public void InitializeGrid(uint width, uint height)
    {
        _gridSize = (width, height);
        _placedBuildings = new Buildable[width, height];
    }

    /// <summary>
    /// Get all the of unique buildables in an area, giving a center position and search size
    /// </summary>
    private HashSet<Buildable> GetBuildablesAtPosition(Vector2 position, Vector2 size)
    {
        List<(uint x, uint y)> gridIndexes = GetPlacedBuildingIndexes(position, size);

        // Get collection of unique buildables in these indexes
        HashSet<Buildable> buildables = new HashSet<Buildable>();
        foreach (var gridIndex in gridIndexes)
        {
            // Ignore if out of bounds
            if (gridIndex.x >= _gridSize.Item1 || gridIndex.y >= _gridSize.Item2)
            {
                continue;
            }
            
            Buildable buildable = _placedBuildings[gridIndex.Item1, gridIndex.Item2];
            if (buildable != null)
            {
                buildables.Add(buildable);
            }
        }

        return buildables;
    }

    /// <summary>
    /// Get a list of indexes a building would take up
    /// </summary>
    private List<(uint x, uint y)> GetPlacedBuildingIndexes(Vector2 position, Vector2 size, bool debugging = false)
    {
        position = position * gridsPerUnit;
        (uint, uint) flooredSize = ((uint) Mathf.Floor(size.x), (uint) Mathf.Floor(size.y));
        (float, float) roundedPosition = (GetRoundedPosition(position.x, flooredSize.Item1), GetRoundedPosition(position.y, flooredSize.Item2));

        if (debugging)
        {
            Debug.Log($"roundedPosition: {roundedPosition}");
        }

        var xIndexes = GetGridIndexes(roundedPosition.Item1, flooredSize.Item1, _gridSize.Item1);
        var yIndexes = GetGridIndexes(roundedPosition.Item2, flooredSize.Item2, _gridSize.Item2);

        List<(uint x, uint y)> indexes = new List<(uint x, uint y)>();
        for (uint i = xIndexes.start; i < xIndexes.endExclusive; i++)
        {
            for (uint j = yIndexes.start; j < yIndexes.endExclusive; j++)
            {
                indexes.Add((i, j));
            }
        }
        //Debug.Log($"position: {position}, gridPosition: {gridPosition}");
        return indexes;
    }

    /// <summary>
    /// Get the snapped world space position on a single axis based on building size
    /// </summary>
    private float GetRoundedPosition(float position, uint size)
    {
        if (size % 2 != 0)
        {
            // Odd
            return Mathf.Round(position);
        }
        else
        {
            // Even
            return Mathf.Round(position - 0.5f) + 0.5f;
        }
    }

    /// <summary>
    /// Get the starting grid index and ending grid index that one axis of a rectangle would take up
    /// </summary>
    private (uint start, uint endExclusive) GetGridIndexes(float roundedPosition, uint size, uint gridSize)
    {
        if (size % 2 != 0)
        {
            // Odd
            float middleGridIndex = roundedPosition + (gridSize / 2f);
            uint startIndex = (uint)Mathf.Round(middleGridIndex - (size / 2f));
            uint endIndex = (uint)Mathf.Round(middleGridIndex + (size / 2f));
            return (startIndex, endIndex);
        }
        else
        {
            // Even
            float middleGridIndex = roundedPosition + (gridSize / 2f) - 0.5f;
            uint startIndex = (uint)Mathf.Round(middleGridIndex - (size / 2f) + 0.5f);
            uint endIndex = (uint)Mathf.Round(middleGridIndex + (size / 2f) + 0.5f);
            return (startIndex, endIndex);
        }
    }

    public void OnBuildButtonClicked(BuildingTypeComponent buildingType)
    {
        // Setup and activate the building indicator
        if (buildingPrefabs.TryGetValue(buildingType.BuildingType, out Buildable buildable))
        {
            _selectedBuilding = buildable;
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
        if (_selectedBuilding != null && _camera != null)
        {
            // Snap to grid under mouse
            Vector2 mouseWorldPosition = _camera.ScreenToWorldPoint(Input.mousePosition);
            uint selectedBuildSizeX = (uint) Mathf.Floor(_selectedBuilding.Size.x);
            uint selectedBuildSizeY = (uint) Mathf.Floor(_selectedBuilding.Size.y);
            Vector2 snappedPosition = new Vector2(GetRoundedPosition(mouseWorldPosition.x, selectedBuildSizeX), GetRoundedPosition(mouseWorldPosition.y, selectedBuildSizeY));
            buildingIndicator.transform.position = snappedPosition;
            
            // DEBUGGING
            if (Input.GetKeyDown(KeyCode.D))
            {
                var indexes = GetPlacedBuildingIndexes(snappedPosition, _selectedBuilding.Size, true);
                Debug.Log($"DEBUG INFO, {String.Join(", ", indexes)}");
            }
            
            // Un highlight buildables that are no longer overlapped
            HashSet<Buildable> overlappingBuildables = GetBuildablesAtPosition(snappedPosition, _selectedBuilding.Size);
            foreach (var noLongerOverlappedBuildable in _previouslyOverlappedBuildings.Except(overlappingBuildables))
            {
                noLongerOverlappedBuildable.GridIndicator.SetActive(false);
            }
            // Highlight buildables that are overlapped
            foreach (var overlappedBuildable in overlappingBuildables)
            {
                overlappedBuildable.GridIndicator.SetActive(true);
                overlappedBuildable.GridIndicator.SetSuccess(false);
            }
            _previouslyOverlappedBuildings = overlappingBuildables;
            // Set the building indicator's grid indicator's color based on if we are overlapping with something
            bool overlapping = overlappingBuildables.Count > 0;
            buildingIndicator.GridIndicator.SetSuccess(!overlapping);
            
            // todo: check that we are clicking on the grid and not on a ui element
            if (Input.GetMouseButtonDown(0))
            {
                // Check if we can afford it
                Currencies cost = _selectedBuilding.GetCost();
                if (_currencies >= cost)
                {
                    // Check that this position isn't on top of any other buildables
                    if (!overlapping)
                    {
                        // Create the building
                        Buildable newBuildable = Instantiate(_selectedBuilding, snappedPosition, Quaternion.identity, buildingContainer);
                        List<(uint, uint)> gridIndexes = GetPlacedBuildingIndexes(snappedPosition, newBuildable.Size);
                        // Place reference to buildable in each grid is occupies
                        foreach (var gridIndex in gridIndexes)
                        {
                            _placedBuildings[gridIndex.Item1, gridIndex.Item2] = newBuildable;
                        }
                    
                        // Debit the currencies
                        _currencies = _currencies - cost;
                        UpdateCurrencies();
                        buildingIndicator.SetActive(false);
                        _selectedBuilding = null;
                    }
                    else
                    {
                        Debug.Log("On top of built on square!");
                    }
                }
                else
                {
                    Debug.Log("Cannot afford!");
                }
            }
        }
    }
    
    private void UpdateCurrencies()
    {
        CurrenciesUpdated?.Invoke(_currencies);
    }
}
