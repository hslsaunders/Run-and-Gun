﻿using _Project.Codebase.Gameplay;
using _Project.Codebase.Modules;
using CHR.UI;
using DanonFramework.Runtime.Core.Utilities;

namespace _Project.Codebase.UI
{
    public class EndTurnButton : CustomButton
    {
        private TurnController m_turnController;
        protected override void Start()
        {
            base.Start();

            onClick += OnClick;
            m_turnController = ModuleUtilities.Get<GameModule>().TurnController;
            m_turnController.OnTurnChange += OnTurnChange;
            SetEnabledState(m_turnController.Turn == Turn.Player);
        }

        protected override void OnDestroy()
        {
            onClick -= OnClick;
        }

        private void OnClick()
        {
            if (m_turnController.Turn == Turn.Player)
                m_turnController.NextTurn();
        }

        private void OnTurnChange(Turn turn)
        {
            SetEnabledState(turn == Turn.Player);
        }
    }
}