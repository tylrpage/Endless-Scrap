using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class GridManager : MonoBehaviour
{
    [SerializeField] private MeshFilter debugGridMesh;

    public Grid<PathfindingNode> SmallEnemyGrid => _smallEnemyGrid;

    private Mesh _mesh;
    private Grid<PathfindingNode> _smallEnemyGrid;

    private void Awake()
    {
        _mesh = new Mesh();
        debugGridMesh.mesh = _mesh;
        float cellSize = 0.5f;
        int gridWidth = GameManager.Instance.BuildManager.GridSize.Item1 * (int)(1f / cellSize);
        int gridHeight = GameManager.Instance.BuildManager.GridSize.Item1 * (int)(1f / cellSize);
        Vector2 centeredPosition = new Vector2(-gridWidth / 2f * cellSize, -gridHeight / 2f * cellSize);
        _smallEnemyGrid = new Grid<PathfindingNode>(gridWidth, gridHeight, cellSize, centeredPosition, (grid, x, y) => new PathfindingNode(x, y));
        
        // Test it
        // Path path = Pathfinding.Pathfinding.GetDirectPath(_smallEnemyGrid, new Point(0, 0), new Point(5, 10));
        // DrawPath(path);
    }

    public void DrawPath(Path path)
    {
        PathfindingNode current = path.Start;
        MeshUtils.CreateEmptyMeshArrays(_smallEnemyGrid.GetWidth() * _smallEnemyGrid.GetHeight(), out Vector3[] vertices, out Vector2[] uv, out int[] triangles);
        while (current != null)
        {
            Vector3 quadSize = new Vector3(1, 1) * _smallEnemyGrid.GetCellSize();
            int index = current.X * _smallEnemyGrid.GetHeight() + current.Y;
            float gridValueNormalized = 1f;
            Vector2 gridValueUV = new Vector2(gridValueNormalized, 0f);
            MeshUtils.AddToMeshArrays(vertices, uv, triangles, index, _smallEnemyGrid.GetWorldPosition(current.X, current.Y) + quadSize * .5f, 0f, quadSize, gridValueUV, gridValueUV);

            current = current.PathNextPathfindingNode;
        }
        
        _mesh.vertices = vertices;
        _mesh.uv = uv;
        _mesh.triangles = triangles;
    }
}
