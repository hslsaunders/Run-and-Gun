﻿using System.Collections.Generic;
using _Project.Codebase.AssetGroups;
using _Project.Codebase.Gameplay.Characters;
using _Project.Codebase.UI;
using DanonFramework.Core;
using UnityEngine;

namespace _Project.Codebase.Gameplay.Player
{
    public class PlayerSelectionController : MonoBehaviour
    {
        private PlayerManager m_playerManager;
        private IPlayerSelectable Selection => m_playerManager.Selection;
        public readonly List<Vector2> desiredMovePath = new();
        public int PathActionPointCost { get; private set; }
        public bool IsValidSelectedPath { get; private set; }
        private NavPathRenderer m_pathRenderer;
        private SpriteRenderer m_tileBoxOutline;

        private void Awake()
        {
            m_playerManager = GetComponent<PlayerManager>();
            m_pathRenderer = ContentUtilities.Instantiate<GameObject>(PrefabAssetGroup.NAV_PATH_RENDERER)
                .GetComponent<NavPathRenderer>();
            m_tileBoxOutline = ContentUtilities.Instantiate<GameObject>(PrefabAssetGroup.TILE_BOX_OUTLINE)
                .GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Mouse0) && !GraphicMouseBlocker.IsMouseOverUI)
            {
                m_pathRenderer.Enabled = false;
                
                Collider2D mouseHitCollider = Physics2D.OverlapPoint(MiscUtilities.WorldMousePos);
                if (mouseHitCollider != null)
                {
                    if (mouseHitCollider.TryGetComponent(out CharacterObject characterObject))  
                    {
                        if (characterObject.Character.Selectable)
                        {
                            m_playerManager.SetSelection(characterObject.Character);
                            return;
                        }
                    }
                }
                
                m_playerManager.SetSelection(null);
            }
            
            
            if (Selection != null && Selection.SelectableType is PlayerSelectableType.Runner or PlayerSelectableType.Enemy)
            {
                if (Selection is Runner runner && !runner.agent.followPath)
                {
                    m_pathRenderer.Enabled = true;
                    
                    bool foundPos = runner.agent.TryGetClosestTilePosInRange(MiscUtilities.WorldMousePos,
                        runner.CurrentLargestPossibleTravelDistance,
                        out float distFromRunner, out Vector2 targetPos);

                    if (foundPos)
                    {
                        runner.agent.TryGetPath(targetPos, desiredMovePath);
                        PathActionPointCost = runner.CalcActionPointCostOfMove(distFromRunner);
                        IsValidSelectedPath = runner.actionPoints >= PathActionPointCost;
                        m_pathRenderer.SetPath(desiredMovePath);
                        m_tileBoxOutline.gameObject.SetActive(desiredMovePath.Count > 0);
                        //m_tileBoxOutline.color = IsValidSelectedPath ? Color.white : Color.red;
                        if (desiredMovePath.Count > 0)
                            m_tileBoxOutline.transform.position = targetPos;
                    }
                    else
                        SetPathEnabledState(false);
                }
            }
            else if (Selection == null)
            {
                SetPathEnabledState(false);
            }
        }

        private void SetPathEnabledState(bool state)
        {
            m_pathRenderer.Enabled = state;
            m_tileBoxOutline.gameObject.SetActive(state);
        }
    }
}