using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Pathfinding;
using UnityEngine;
using UnityEngine.EventSystems;
using Point = Pathfinding.Point;

public class BuildManager : MonoBehaviour
{
    public enum BuildingType
    {
        TurretLevel1, TurretLevel2, Barricade
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
        
        public static Currencies operator *(Currencies a, int b)
        {
            return new Currencies()
            {
                Scrap = a.Scrap * b
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

        public override bool Equals(object obj)
        {
            if (obj is Currencies currencies)
            {
                return this == currencies;
            }
            else
            {
                return base.Equals(obj);
            }
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public static event Action<Currencies> CurrenciesUpdated;

    public (int, int) GridSize => _gridSize;
    public Currencies CurrenciesAmount => _currencies;
    public Grid<Node<Buildable>> BuildablesGrid => _buildablesGrid;

    [SerializeField] private SerializableDictionary<BuildingType, Buildable> buildingPrefabs;
    [SerializeField] private BuildingIndicator buildingIndicator;
    [SerializeField] private Transform buildingContainer;
    [SerializeField] private float gridsPerUnit;
    [SerializeField] private RectTransform buildButtonsContainer;
    [SerializeField] private AudioClip placeBuildingSound;

    private Currencies _currencies = new Currencies()
    {
        Scrap = 30
    };
    private Buildable _selectedBuilding;
    private Camera _camera;

    private (int, int) _gridSize;
    private Grid<Node<Buildable>> _buildablesGrid;
    private HashSet<Buildable> _previouslyOverlappedBuildings = new HashSet<Buildable>();
    private bool _enableBuilding;

    private void Awake()
    {
        UpdateCurrencies();
        buildingIndicator.SetActive(false);
        _camera = Camera.main;

        InitializeGrid(21, 21, 1);
    }

    public void InitializeGrid(int width, int height, float cellSize)
    {
        _gridSize = (width, height);
        Vector2 centeredPosition = new Vector2(-width / 2f * cellSize, -height / 2f * cellSize);
        _buildablesGrid = new Grid<Node<Buildable>>(width, height, cellSize, centeredPosition, (grid, x, y) => new Node<Buildable>(x, y));
    }

    public void EnableBuilding()
    {
        _enableBuilding = true;
        buildButtonsContainer.gameObject.SetActive(true);
    }
    
    public void DisableBuilding()
    {
        _enableBuilding = false;
        ClearSelection();
        buildButtonsContainer.gameObject.SetActive(false);
    }

    /// <summary>
    /// Get all the of unique buildables in an area, giving a center position and search size
    /// </summary>
    private HashSet<Buildable> GetBuildablesAtPosition(Vector2 position, Vector2 size)
    {
        List<(int x, int y)> gridIndexes = GetPlacedBuildingIndexes(position, size);

        // Get collection of unique buildables in these indexes
        HashSet<Buildable> buildables = new HashSet<Buildable>();
        foreach (var gridIndex in gridIndexes)
        {
            // Ignore if out of bounds
            if (!_buildablesGrid.IsValidPosition(gridIndex.x, gridIndex.y))
            {
                continue;
            }
            
            Buildable buildable = _buildablesGrid.GetGridObject(gridIndex.Item1, gridIndex.Item2).Data;
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
    private List<(int x, int y)> GetPlacedBuildingIndexes(Vector2 position, Vector2 size, bool debugging = false)
    {
        position = position * gridsPerUnit;
        (int, int) flooredSize = ((int) Mathf.Floor(size.x), (int) Mathf.Floor(size.y));
        (float, float) roundedPosition = (GetRoundedPosition(position.x, flooredSize.Item1), GetRoundedPosition(position.y, flooredSize.Item2));

        if (debugging)
        {
            Debug.Log($"roundedPosition: {roundedPosition}");
        }

        var xIndexes = GetGridIndexes(roundedPosition.Item1, flooredSize.Item1, _gridSize.Item1);
        var yIndexes = GetGridIndexes(roundedPosition.Item2, flooredSize.Item2, _gridSize.Item2);

        List<(int x, int y)> indexes = new List<(int x, int y)>();
        for (int i = xIndexes.start; i < xIndexes.endExclusive; i++)
        {
            for (int j = yIndexes.start; j < yIndexes.endExclusive; j++)
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
    private float GetRoundedPosition(float position, int size)
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
    private (int start, int endExclusive) GetGridIndexes(float roundedPosition, int size, int gridSize)
    {
        if (size % 2 != 0)
        {
            // Odd
            float middleGridIndex = roundedPosition + (gridSize / 2f);
            int startIndex = (int)Mathf.Round(middleGridIndex - (size / 2f));
            int endIndex = (int)Mathf.Round(middleGridIndex + (size / 2f));
            return (startIndex, endIndex);
        }
        else
        {
            // Even
            float middleGridIndex = roundedPosition + (gridSize / 2f) - 0.5f;
            int startIndex = (int)Mathf.Round(middleGridIndex - (size / 2f) + 0.5f);
            int endIndex = (int)Mathf.Round(middleGridIndex + (size / 2f) + 0.5f);
            return (startIndex, endIndex);
        }
    }

    public void OnBuildButtonClicked(BuildingTypeComponent buildingType)
    {
        // Setup and activate the building indicator
        if (buildingPrefabs.TryGetValue(buildingType.BuildingType, out Buildable buildable))
        {
            _selectedBuilding = buildable;
            buildingIndicator.SetActive(true);
            upAfterBuildButtonClicked = false;
        }
        else
        {
            Debug.LogError($"Attempted to build a building type that isn't in buildingPrefabs, building type: {buildingType.BuildingType}");
        }
    }

    private bool upAfterBuildButtonClicked;
    private Vector2? dragBeginSnappedPosition;
    
    //Returns 'true' if we touched or hovering on Unity UI element.
    public bool IsPointerOverUIElement()
    {
        return IsPointerOverUIElement(GetEventSystemRaycastResults());
    }
 
 
    //Returns 'true' if we touched or hovering on Unity UI element.
    private bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaysastResults)
    {
        for (int index = 0; index < eventSystemRaysastResults.Count; index++)
        {
            RaycastResult curRaysastResult = eventSystemRaysastResults[index];
            if (curRaysastResult.gameObject.layer == 5)
                return true;
        }
        return false;
    }
 
 
    //Gets all event system raycast results of current mouse or touch position.
    static List<RaycastResult> GetEventSystemRaycastResults()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> raysastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raysastResults);
        return raysastResults;
    }

    private void Update()
    {
        if (_selectedBuilding != null && _camera != null && upAfterBuildButtonClicked)
        {
            bool onUI = false;
            if (IsPointerOverUIElement())
            {
                onUI = true;
                Debug.Log("onui");
            }
            // Snap to grid under mouse
            Vector2 mouseWorldPosition = _camera.ScreenToWorldPoint(Input.mousePosition);
            int selectedBuildSizeX = (int) Mathf.Floor(_selectedBuilding.Size.x);
            int selectedBuildSizeY = (int) Mathf.Floor(_selectedBuilding.Size.y);
            Vector2 snappedPosition = new Vector2(GetRoundedPosition(mouseWorldPosition.x, selectedBuildSizeX), GetRoundedPosition(mouseWorldPosition.y, selectedBuildSizeY));

            Vector2[] points;
            if (dragBeginSnappedPosition != null && dragBeginSnappedPosition != snappedPosition && _selectedBuilding.Size.x == 1 && _selectedBuilding.Size.y == 1)
            {
                Vector2 dragBeginSnappedPositionNotNull = (Vector2) dragBeginSnappedPosition;
                
                // get diagonal distance
                float dx = snappedPosition.x - dragBeginSnappedPositionNotNull.x;
                float dy = snappedPosition.y - dragBeginSnappedPositionNotNull.y;
                float diagonalDistance = Math.Max(Math.Abs(dx), Math.Abs(dy));

                int diagonalDistanceCeil = (int)Mathf.Ceil(diagonalDistance);
                // get points
                points = new Vector2[diagonalDistanceCeil + 1];
                for (int step = 0; step <= diagonalDistanceCeil; step++)
                {
                    float t = Util.LosePrecision((float)step / diagonalDistance);
                    Vector2 lerpedPoint = new Vector2(
                        (int)Math.Round(Mathf.Lerp(dragBeginSnappedPositionNotNull.x, snappedPosition.x, t)),
                        (int)Math.Round(Mathf.Lerp(dragBeginSnappedPositionNotNull.y, snappedPosition.y, t)));
                    points[step] = lerpedPoint;
                }
            }
            else
            {
                points = new[] { snappedPosition };
            }
            
            // DEBUGGING
            if (Input.GetKeyDown(KeyCode.D))
            {
                var indexes = GetPlacedBuildingIndexes(snappedPosition, _selectedBuilding.Size, true);
                Debug.Log($"DEBUG INFO, {String.Join(", ", indexes)}");
            }

            // Get all buildables overlapping with our points
            HashSet<Buildable> overlappingBuildables = new HashSet<Buildable>();
            bool outOfBounds = false;
            foreach (var point in points)
            {
                var buildablesOverlappingWithThisPoint = GetBuildablesAtPosition(point, _selectedBuilding.Size);
                overlappingBuildables.UnionWith(buildablesOverlappingWithThisPoint); 
                
                _buildablesGrid.GetXY(point, out int x, out int y);
                if (!_buildablesGrid.IsValidPosition(x, y))
                {
                    outOfBounds = true;
                }
            }
            
            // Un highlight buildables that are no longer overlapped
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
            // Check if we can afford it
            Currencies cost = _selectedBuilding.GetCost() * points.Length;
            bool canAfford = _currencies >= cost;
            bool success = !overlapping && canAfford && !outOfBounds;
            buildingIndicator.SetPoints(_selectedBuilding, points, success);

            if (!CameraControls.IsDragging())
            {
                if (Input.GetMouseButtonDown(0) && onUI)
                {
                    ClearSelection();
                }
                else if (Input.GetMouseButtonDown(0))
                {
                    dragBeginSnappedPosition = snappedPosition;
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    if (success)
                    {
                        // Create the buildings
                        foreach (var point in points)
                        {
                            Buildable newBuildable = Instantiate(_selectedBuilding, point, Quaternion.identity, buildingContainer);
                        }

                        // Debit the currencies
                        _currencies = _currencies - cost;
                        UpdateCurrencies();
                        ClearSelection(false);
                        GameManager.Instance.MusicManager.PlayOneShot(placeBuildingSound);
                    }
                    else
                    {
                        ClearSelection();
                    }
                }
            }

            if (Input.GetMouseButtonDown(1))
            {
                ClearSelection();
            }
        }
        
        // When the mouse button comes up after the building button is pressed, now we can build
        // THis prevents the building being immedietly placed when clicking the button
        if (Input.GetMouseButtonUp(0) && !upAfterBuildButtonClicked)
        {
            upAfterBuildButtonClicked = true;
        }
    }

    private void ClearSelection(bool clearSelection = true)
    {
        if (clearSelection)
        {
            _selectedBuilding = null;
            buildingIndicator.SetActive(false);
        }
        dragBeginSnappedPosition = null;
        
        buildingIndicator.Clear();
        
        foreach (var building in _previouslyOverlappedBuildings)
        {
            building.GridIndicator.SetActive(false);
        }
    }

    public void AddBuildableToGrid(Buildable buildable, Vector2 worldPosition)
    {
        List<(int, int)> gridIndexes = GetPlacedBuildingIndexes(worldPosition, buildable.Size);
        // Place reference to buildable in each grid it occupies
        foreach (var gridIndex in gridIndexes)
        {
            Node<Buildable> node = _buildablesGrid.GetGridObject(gridIndex.Item1, gridIndex.Item2);
            if (node.Data != null)
            {
                Debug.LogError($"Attempted to add buildable to an occupied grid space, gridIndex: {gridIndex}, existingBuildable: {node.Data.gameObject.name}, newBuildable: {buildable.gameObject.name}");
            }
            
            node.Data = buildable;
        }
    }

    public void RemoveBuildableFromGrid(Buildable buildable, Vector2 worldPosition)
    {
        List<(int, int)> gridIndexes = GetPlacedBuildingIndexes(worldPosition, buildable.Size);
        // Place reference to buildable in each grid it occupies
        foreach (var gridIndex in gridIndexes)
        {
            Node<Buildable> node = _buildablesGrid.GetGridObject(gridIndex.Item1, gridIndex.Item2);
            if (node.Data == null)
            {
                Debug.LogError($"Attempted to add remove to an empty grid space, gridIndex: {gridIndex}, buildable: {buildable.gameObject.name}");
            }
            
            node.Data = null;
        }
    }
    
    private void UpdateCurrencies()
    {
        CurrenciesUpdated?.Invoke(_currencies);
    }

    public void AddScrap(int scrap)
    {
        _currencies.Scrap += scrap;
        UpdateCurrencies();
    }

    public void RemoveAllScrap()
    {
        _currencies.Scrap = 0;
        UpdateCurrencies();
    }
}
