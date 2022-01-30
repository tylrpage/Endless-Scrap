using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Pathfinding;

public class GridManager : MonoBehaviour
{
    [SerializeField] private MeshFilter debugGridMesh;

    public Grid<PathfindingNode> SmallEnemyPathfindingGrid => _smallEnemyPathfindingGrid;

    public Grid<EnemiesNode> EnemyGrid => _enemyGrid;

    private Mesh _mesh;
    private Grid<PathfindingNode> _smallEnemyPathfindingGrid;
    private Grid<EnemiesNode> _enemyGrid;

    private void Awake()
    {
        _mesh = new Mesh();
        debugGridMesh.mesh = _mesh;
        float cellSize = 0.5f;
        int gridWidth = GameManager.Instance.BuildManager.GridSize.Item1 * (int)(1f / cellSize);
        int gridHeight = GameManager.Instance.BuildManager.GridSize.Item2 * (int)(1f / cellSize);
        Vector2 centeredPosition = new Vector2(-gridWidth / 2f * cellSize, -gridHeight / 2f * cellSize);
        _smallEnemyPathfindingGrid = new Grid<PathfindingNode>(gridWidth, gridHeight, cellSize, centeredPosition, (grid, x, y) => new PathfindingNode(x, y));
        _enemyGrid = new Grid<EnemiesNode>(gridWidth, gridHeight, cellSize, centeredPosition, (grid, x, y) => new EnemiesNode(x, y));
        
        // Test it
        // Path path = Pathfinding.Pathfinding.GetDirectPath(_smallEnemyGrid, new Point(0, 0), new Point(5, 10));
        // DrawPath(path);
    }

    public void DrawPath<T>(Grid<T> grid, Path path) where T : class
    {
        PathfindingNode current = path.Start;
        MeshUtils.CreateEmptyMeshArrays(grid.GetWidth() * grid.GetHeight(), out Vector3[] vertices, out Vector2[] uv, out int[] triangles);
        while (current != null)
        {
            Vector3 quadSize = new Vector3(1, 1) * grid.GetCellSize();
            int index = current.X * grid.GetHeight() + current.Y;
            float gridValueNormalized = 1f;
            Vector2 gridValueUV = new Vector2(gridValueNormalized, 0f);
            MeshUtils.AddToMeshArrays(vertices, uv, triangles, index, grid.GetWorldPosition(current.X, current.Y) + quadSize * .5f, 0f, quadSize, gridValueUV, gridValueUV);

            current = current.PathNextPathfindingNode;
        }
        
        _mesh.vertices = vertices;
        _mesh.uv = uv;
        _mesh.triangles = triangles;
    }

    public void DrawTiles<T>(Grid<T> grid, IEnumerable<(int, int)> tiles) where T : class
    {
        MeshUtils.CreateEmptyMeshArrays(grid.GetWidth() * grid.GetHeight(), out Vector3[] vertices, out Vector2[] uv, out int[] triangles);
        foreach (var tile in tiles)
        {
            Vector3 quadSize = new Vector3(1, 1) * grid.GetCellSize();
            int index = tile.Item1 * grid.GetHeight() + tile.Item2;
            float gridValueNormalized = 1f;
            Vector2 gridValueUV = new Vector2(gridValueNormalized, 0f);
            MeshUtils.AddToMeshArrays(vertices, uv, triangles, index, grid.GetWorldPosition(tile.Item1, tile.Item2) + quadSize * .5f, 0f, quadSize, gridValueUV, gridValueUV);
        }
        
        _mesh.vertices = vertices;
        _mesh.uv = uv;
        _mesh.triangles = triangles;
    }
    
    public void AddEnemyToGrid(Grid<EnemiesNode> grid, BattleObject battleObject, Vector2 worldPosition)
    {
        List<(int, int)> gridIndexes = GetPlacedBuildingIndexes(grid, worldPosition, battleObject.Size);
        // Place reference to buildable in each grid it occupies
        foreach (var gridIndex in gridIndexes)
        {
            EnemiesNode node = grid.GetGridObject(gridIndex.Item1, gridIndex.Item2);
            
            node.Data.Add(battleObject);
        }
    }

    public void RemoveEnemyFromGrid(Grid<EnemiesNode> grid, BattleObject battleObject, Vector2 worldPosition)
    {
        List<(int, int)> gridIndexes = GetPlacedBuildingIndexes(grid, worldPosition, battleObject.Size);
        // Place reference to buildable in each grid it occupies
        foreach (var gridIndex in gridIndexes)
        {
            EnemiesNode node = grid.GetGridObject(gridIndex.Item1, gridIndex.Item2);
            
            node.Data.Remove(battleObject);
        }
    }

    public void MoveEnemyOnGrid(Grid<EnemiesNode> grid, BattleObject battleObject, Vector2 originalPosition,
        Vector2 newPosition)
    {
        HashSet<(int, int)> oldGridIndexes = new HashSet<(int, int)>(GetPlacedBuildingIndexes(grid, originalPosition, battleObject.Size));
        HashSet<(int, int)> newGridIndexes = new HashSet<(int, int)>(GetPlacedBuildingIndexes(grid, newPosition, battleObject.Size));
        foreach (var indexToRemove in oldGridIndexes.Except(newGridIndexes))
        {
            EnemiesNode node = grid.GetGridObject(indexToRemove.Item1, indexToRemove.Item2);
            node.Data.Remove(battleObject);
        }
        foreach (var indexToAdd in newGridIndexes.Except(oldGridIndexes))
        {
            EnemiesNode node = grid.GetGridObject(indexToAdd.Item1, indexToAdd.Item2);
            node.Data.Add(battleObject);
        }
        
        // Debugging
        //DrawTiles(grid, newGridIndexes);
    }
    
    /// <summary>
    /// Get a list of indexes a building would take up
    /// </summary>
    private List<(int x, int y)> GetPlacedBuildingIndexes<T>(Grid<T> grid, Vector2 position, Vector2 size, bool debugging = false) where T : class
    {
        // Scale it up if we are on a smaller grid so rounding operations work
        position = position / grid.GetCellSize();
        size = size / grid.GetCellSize();
        
        (int, int) flooredSize = ((int) Mathf.Floor(size.x), (int) Mathf.Floor(size.y));
        (float, float) roundedPosition = (GetRoundedPosition(position.x, flooredSize.Item1), GetRoundedPosition(position.y, flooredSize.Item2));

        if (debugging)
        {
            Debug.Log($"roundedPosition: {roundedPosition}");
        }

        var xIndexes = GetGridIndexes(roundedPosition.Item1, flooredSize.Item1, grid.GetWidth());
        var yIndexes = GetGridIndexes(roundedPosition.Item2, flooredSize.Item2, grid.GetHeight());

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
}
