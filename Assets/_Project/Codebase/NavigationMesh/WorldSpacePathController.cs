﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Codebase.NavigationMesh
{
    public sealed class WorldSpacePathController
    {
        private readonly AstarPathfinder m_astarPathfinder;
        public List<Vector2> Path { get; }
        public Vector2 NextNode { get; private set; }
        public Vector2 LastNode { get; private set; }
        public Vector2 DirToNextNode { get; private set; }
        public float DistFromLastToNextNode { get; private set; }
        public bool AtPathEnd { get; private set; }
        public Action<Vector2, Vector2Int> onReachPathEnd;
        private int m_pathIndex;
        private readonly List<Vector2Int> m_gridPath = new();
        private readonly bool m_cardinalOnly;
        private readonly Func<Vector2, Vector2Int> m_worldToGrid;
        private Func<Vector2Int, Vector2> m_gridToWorld;
        private readonly Converter<Vector2Int, Vector2> m_gridToWorldConverter;
        private readonly Converter<Vector2, Vector2Int> m_worldToGridConverter;

        public WorldSpacePathController(Navmesh navmesh, Func<Vector2, Vector2Int> worldToGrid, 
            Func<Vector2Int, Vector2> gridToWorld, bool cardinalOnly)
        {
            m_astarPathfinder = new AstarPathfinder(navmesh);
            m_worldToGrid = worldToGrid;
            m_gridToWorld = gridToWorld;
            m_gridToWorldConverter = new Converter<Vector2Int, Vector2>(gridToWorld);
            m_worldToGridConverter = new Converter<Vector2, Vector2Int>(worldToGrid);
            m_cardinalOnly = cardinalOnly;
            Path = new List<Vector2>();
            AtPathEnd = true;
        }

        public PathResults GenerateAndSetPath(Vector2Int source, Vector2Int target, bool allowPartialPaths = false, 
            float maxDistance = Mathf.Infinity)
        {
            PathResults results = GeneratePath(source, target, Path, m_gridPath, allowPartialPaths, maxDistance);
            m_pathIndex = 0;
            if (results.type is PathResultType.FullPath or PathResultType.PartialPath)
                UpdateData();
            return results;
        }

        public PathResults GenerateAndSetPath(Vector2 source, Vector2Int target, bool allowPartialPaths = false, 
            float maxDistance = Mathf.Infinity)
        {
            return GenerateAndSetPath(m_worldToGrid(source), target, allowPartialPaths, maxDistance);
        }
        
        public PathResults GenerateAndSetPath(Vector2 source, Vector2 target, bool allowPartialPaths = false, 
            float maxDistance = Mathf.Infinity)
        {
            return GenerateAndSetPath(m_worldToGrid(source), m_worldToGrid(target), allowPartialPaths, maxDistance);
        }

        public PathResults GeneratePath(Vector2 source, Vector2 target, bool allowPartialPaths = false, float maxDistance = Mathf.Infinity)
        {
            return GeneratePath(source, target, new List<Vector2>(), new List<Vector2Int>(), allowPartialPaths, maxDistance);
        }
        
        public PathResults GeneratePath(Vector2 source, Vector2 target, in List<Vector2> path, bool allowPartialPaths = false, 
            float maxDistance = Mathf.Infinity)
        {
            List<Vector2Int> gridPath = new List<Vector2Int>();
            return GeneratePath(source, target, path, gridPath, allowPartialPaths, maxDistance);
        }

        public void ForceSetPath(in List<Vector2> worldPath)
        {
            Path.Clear();
            Path.AddRange(worldPath);
            m_gridPath.Clear();
            m_gridPath.AddRange(Path.ConvertAll(m_worldToGridConverter));
            m_pathIndex = 0;
            UpdateData();
        }

        private PathResults GeneratePath(Vector2 source, Vector2 target, in List<Vector2> path, in List<Vector2Int> gridPath,
            bool allowPartialPaths = false, float maxDistance = Mathf.Infinity)
        {
            return GeneratePath(m_worldToGrid(source), m_worldToGrid(target), path, gridPath, allowPartialPaths, maxDistance);
        }

        private PathResults GeneratePath(Vector2Int source, Vector2Int target, in List<Vector2> path, in List<Vector2Int> gridPath,
            bool allowPartialPaths = false, float maxDistance = Mathf.Infinity)
        {
            PathResults results = m_astarPathfinder.FindPath(source, target, m_cardinalOnly, gridPath, allowPartialPaths, maxDistance);
            path.Clear();
            gridPath.Reverse();
            path.AddRange(gridPath.ConvertAll(m_gridToWorldConverter));
            return results;
        }

        public bool TryProgressToNextNode()
        {
            if (AtPathEnd) return false;
            m_pathIndex++;
            UpdateData();
            return true;
        }

        private void UpdateData()
        {
            AtPathEnd = m_pathIndex >= Path.Count - 1;
            if (AtPathEnd)
            {
                onReachPathEnd?.Invoke(Path[^1], m_gridPath[^1]);
            }
            if (Path.Count == 0) return;
            LastNode = Path[m_pathIndex];
            if (!AtPathEnd)
                NextNode = Path[m_pathIndex + 1];
            DirToNextNode = NextNode - Path[m_pathIndex];
            DistFromLastToNextNode = DirToNextNode.magnitude;
            DirToNextNode.Normalize();
        }
    }
}