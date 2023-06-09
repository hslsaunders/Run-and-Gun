﻿using System;
using _Project.Codebase.Gameplay.AI;
using _Project.Codebase.Gameplay.Player;
using _Project.Codebase.Gameplay.Projectiles;
using _Project.Codebase.Gameplay.World;
using _Project.Codebase.Modules;
using _Project.Codebase.NavigationMesh;
using Cysharp.Threading.Tasks;
using DanonFramework.Core;
using UnityEngine;

namespace _Project.Codebase.Gameplay.Characters
{
    public abstract class Character : IFloorObject, IProjectileHittable, IPlayerSelectable, IDamageable
    {
        public event Action OnCharacterDeath;
        public readonly NavmeshAgent agent;
        public readonly Transform transform;
        public bool Dead { get; private set; }
        public virtual PlayerSelectableType SelectableType { get; set; }
        public bool Selectable => !Dead;

        public Vector2Int FloorPos { get; set; }
        public int actionPoints;
        public int MaxActionPoints => c_default_max_action_points;
        public int moveDistancePerActionPoint = c_default_move_distance_per_action_point;
        public int CurrentLargestPossibleTravelDistance => moveDistancePerActionPoint * actionPoints;
        public int LargestPossibleTravelDistance => moveDistancePerActionPoint * MaxActionPoints;
        public int Health { get; private set; }
        public int MaxHealth { get; private set; }

        public Vector2 FacingDirection { get; private set; }

        private readonly CharacterRenderer m_renderer;

        private const int c_default_max_action_points = 2;
        private const int c_default_move_distance_per_action_point = 6;

        public Character(Vector2Int position, NavmeshAgent agent, CharacterRenderer characterRenderer, int maxHealth)  
        {
            this.agent = agent;
            actionPoints = MaxActionPoints;
            m_renderer = characterRenderer;
            m_renderer.Animator.Initialize(this);
            transform = agent.transform;
            agent.OnReachPathEnd += OnReachPathEnd;
            actionPoints = MaxActionPoints;
            MaxHealth = maxHealth;
            Health = MaxHealth;
            agent.OnGeneratePathTree += OnAgentGeneratePathTree;
            UpdateFloorPosition(position, true, false);
        }

        public void ResetActionPointsToMax() => actionPoints = MaxActionPoints;
        
        protected virtual void OnAgentGeneratePathTree(ShortestPathTree tree) {}

        public void SetFacingDir(Vector2 dir)
        {
            FacingDirection = dir;
            m_renderer.GraphicsTransform.right = dir;
        }

        public void CalculatePathTreeAtPos()
        {
            agent.CalculateAllPathsFromSource(FloorPos, LargestPossibleTravelDistance);
        }

        public void PreTurnStart()
        {
            CalculatePathTreeAtPos();
        }
        
        public async UniTask PerformAction(CharacterAction action)
        {
            if (Dead) return;

            actionPoints -= action.ActionPointCost;
            action.Run().Forget();
            while (!action.finished)
            {
                m_renderer.Animator.SetLegsAnimationState(true);
                await UniTask.Yield();
            }
            m_renderer.Animator.SetLegsAnimationState(false);
        }

        private void UpdateFloorPosition(Vector2Int gridPos, bool teleportToPos = false, bool clearOldPosition = true)
        {
            Building building = ModuleUtilities.Get<GameModule>().Building;
            if (clearOldPosition)
                building.SetFloorObjectAtPos(FloorPos, null);
            FloorPos = gridPos;
            building.SetFloorObjectAtPos(gridPos, this);
            if (teleportToPos)
                transform.position = building.GridToWorld(FloorPos);
        }

        protected virtual void OnReachPathEnd(Vector2 worldPos, Vector2Int gridPos)
        {
            UpdateFloorPosition(gridPos, true);
            agent.CalculateAllPathsFromSource(gridPos, LargestPossibleTravelDistance);
        }
        
        public int CalcActionPointCostOfMove(float distance) => Mathf.CeilToInt(distance / moveDistancePerActionPoint);

        public void OnProjectileHit(int damage)
        {
            TakeDamage(damage);
        }
        
        public void SetPlayerSelectState(bool state)
        {
            m_renderer.SelectionRenderer.SetSelectionState(state);
            m_renderer.RangeRenderer.SetDisplayedState(state);
        }
        
        public void TakeDamage(int damage)
        {
            Health = Mathf.Max(Health - damage, 0);
            m_renderer.HealthRenderer.SetValue((float)Health / MaxHealth);
            if (Health == 0)
                Die();
        }

        public void Die()
        {
            OnCharacterDeath?.Invoke();
            Dead = true;
        }
    }
}