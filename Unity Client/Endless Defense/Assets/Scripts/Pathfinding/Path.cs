using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    public class Path
    {
        public PathfindingNode Start;
        public PathfindingNode End;
        public PathfindingNode Current;

        public Path(PathfindingNode start, PathfindingNode end)
        {
            this.Start = start;
            this.End = end;
            this.Current = start;
        }
    }
}