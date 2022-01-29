using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    public class Node<T> where T : class
    {
        public int X;
        public int Y;
        public T Data;

        public Node(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
    }
    
    public class PathfindingNode : Node<PathfindingNode>
    {
        // public uint GCost;
        // public uint HCost;
        public PathfindingNode PathNextPathfindingNode
        {
            get => Data;
            set => Data = value;
        }
        public PathfindingNode PathPreviousPathfindingNode;

        public PathfindingNode(int x, int y) : base(x, y)
        {
        }
    }
}