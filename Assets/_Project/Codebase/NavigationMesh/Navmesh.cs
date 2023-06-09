﻿using System.Collections.Generic;
using UnityEngine;

namespace _Project.Codebase.NavigationMesh
{
    public sealed class Navmesh
    {
        public const float DIAGONAL_COST = 1f;
        public const float CARDINAL_COST = .707f;
        public const int SEARCH_LIMIT = 1000;
        
        private readonly Dictionary<Vector2Int, NavmeshNode> m_nodes = new();
        public readonly List<INavmeshChangeSubscriber> navmeshSubscribers = new();

        public Navmesh(Navmesh navmeshToCopy)
        {
            m_nodes = new Dictionary<Vector2Int, NavmeshNode>(navmeshToCopy.m_nodes);
        }
        
        public Navmesh(Dictionary<Vector2Int, bool> nodes)
        {
            foreach (KeyValuePair<Vector2Int, bool> node in nodes)
            {
                m_nodes[node.Key] = new NavmeshNode(node.Key, node.Value);
            }
        }

        public bool IsValidNode(Vector2Int pos) => true;

        public bool IsWalkableNode(Vector2Int pos)
        {
            return m_nodes.TryGetValue(pos, out NavmeshNode node) && node.walkable;
        }

        public NavmeshNode GetNodeAtPos(Vector2Int pos)
        {
            m_nodes.TryGetValue(pos, out NavmeshNode node);
            return node;
        }

        public void SetWalkable(Vector2Int pos, bool walkableState)
        {
            NavmeshNode node = m_nodes[pos];
            if (node.walkable == walkableState) return;
            
            m_nodes[pos].walkable = walkableState;

            for (var i = navmeshSubscribers.Count - 1; i >= 0; i--)
            {
                var subscriber = navmeshSubscribers[i];
                if (subscriber == null)
                {
                    navmeshSubscribers.RemoveAt(i);
                    continue;
                }
                subscriber.NavmeshReferenceDirty = true;
            }
        }
    }
}