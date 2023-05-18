﻿using System.Collections.Generic;
using UnityEngine;

namespace _Project.Codebase.NavigationMesh
{
    public sealed class ShortestPathTree
    {
        public readonly Vector2Int source;
        public readonly Dictionary<Vector2Int, PathNode> nodes;
        
        public ShortestPathTree(Vector2Int source, Dictionary<Vector2Int, PathNode> nodes)
        {
            this.source = source;
            this.nodes = nodes;
        }

        public List<PathNode> GetNodesInRange(float range)
        {
            List<PathNode> nodesInRange = new List<PathNode>();
            foreach (PathNode node in nodes.Values)
                if (node.distance <= range)
                    nodesInRange.Add(node);
            return nodesInRange;
        }

        public bool ContainsPoint(Vector2Int point) => nodes.ContainsKey(point);

        public PathResults TryTracePath(Vector2Int pathEnd, in List<Vector2Int> path, float maxRange = Mathf.Infinity)
        {
            if (!nodes.TryGetValue(pathEnd, out PathNode node))
            {
                return new PathResults(PathResultType.NoPath, 0f);
            }

            float distance = node.distance;

            while (node != null)
            {
                path.Add(node.pos);
                node = node.parent;
            }
            
            path.Reverse();

            return new PathResults(PathResultType.FullPath, distance);
        }
    }
}