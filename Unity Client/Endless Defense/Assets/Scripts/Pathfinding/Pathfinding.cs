using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    public struct Point
    {
        public int X;
        public int Y;

        public Point(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
    }
    
    public class Pathfinding
    {
        public static Path GetDirectPath(Grid<PathfindingNode> grid, Point start, Point end)
        {
            // get diagonal distance
            int dx = end.X - start.X;
            int dy = end.Y - start.Y;
            int diagonalDistance = Math.Max(Math.Abs(dx), Math.Abs(dy));
            
            // get points
            Point[] points = new Point[diagonalDistance + 1];
            for (int step = 0; step <= diagonalDistance; step++)
            {
                float t = Util.LosePrecision((float)step / diagonalDistance);
                Point lerpedPoint = new Point()
                {
                    X = (int)Math.Round(Mathf.Lerp(start.X, end.X, t)),
                    Y = (int)Math.Round(Mathf.Lerp(start.Y, end.Y, t)),
                };
                points[step] = lerpedPoint;
            }
            
            // Create a path
            PathfindingNode previousPathfindingNode = null;
            PathfindingNode firstPathfindingNode = null;
            PathfindingNode currentPathfindingNode = null;
            foreach (var point in points)
            {
                currentPathfindingNode = new PathfindingNode(point.X, point.Y) { PathPreviousPathfindingNode = previousPathfindingNode};

                // start node is the first node we create
                firstPathfindingNode ??= currentPathfindingNode;
                
                if (previousPathfindingNode != null)
                {
                    previousPathfindingNode.PathNextPathfindingNode = currentPathfindingNode;
                }

                previousPathfindingNode = currentPathfindingNode;
            }

            Path path = new Path(firstPathfindingNode, currentPathfindingNode);
            return path;
        }

        public static bool IsPathBlocked(Grid<PathfindingNode> pathGrid, Grid<Node<Buildable>> obstaclesGrid, Path path, out Node<Buildable> blockingObstacleNode)
        {
            PathfindingNode currentPathfindingNode = path.Current;
            PathfindingNode nextPathfindingNode = currentPathfindingNode.PathNextPathfindingNode;

            if (nextPathfindingNode == null)
            {
                blockingObstacleNode = null;
                return false;
            }
            
            int dx = nextPathfindingNode.X - currentPathfindingNode.X;
            int dy = nextPathfindingNode.Y - currentPathfindingNode.Y;

            // Check the tile horizontal to the destination
            if (Mathf.Abs(dx) > 0)
            {
                Vector2 checkWorldPos = pathGrid.GetWorldPosition(nextPathfindingNode.X, currentPathfindingNode.Y);
                Node<Buildable> buildableNode = obstaclesGrid.GetGridObject(checkWorldPos);
                if (buildableNode.Data != null)
                {
                    blockingObstacleNode = buildableNode;
                    return true;
                }
            }
            // Check the tile vertical to the destination
            if (Mathf.Abs(dy) > 0)
            {
                Vector2 checkWorldPos = pathGrid.GetWorldPosition(currentPathfindingNode.X, nextPathfindingNode.Y);
                Node<Buildable> buildableNode = obstaclesGrid.GetGridObject(checkWorldPos);
                if (buildableNode.Data != null)
                {
                    blockingObstacleNode = buildableNode;
                    return true;
                }
            }
            // Check the tile at the diagonal destination if the tile is diagonal
            if (Mathf.Abs(dx) > 0 && Mathf.Abs(dy) > 0)
            {
                Vector2 checkWorldPos = pathGrid.GetWorldPosition(nextPathfindingNode.X, nextPathfindingNode.Y);
                Node<Buildable> buildableNode = obstaclesGrid.GetGridObject(checkWorldPos);
                if (buildableNode.Data != null)
                {
                    blockingObstacleNode = buildableNode;
                    return true;
                }
            }
            
            // If we got to this point, there is no obstacle
            blockingObstacleNode = null;
            return false;
        }
    }
}