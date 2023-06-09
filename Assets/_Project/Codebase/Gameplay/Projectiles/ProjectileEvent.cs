﻿using UnityEngine;

namespace _Project.Codebase.Gameplay.Projectiles
{
    public class ProjectileEvent
    {
        public readonly ProjectileEventType type;
        public readonly Vector2 location;
        public readonly float time;
        public readonly Vector2 travelDir;
        public readonly bool terminate;

        public ProjectileEvent(ProjectileEventType type, Vector2 location, float time, Vector2 travelDir, bool terminate = false)
        {
            this.type = type;
            this.location = location;
            this.time = time;
            this.travelDir = travelDir;
            this.terminate = terminate;
        }
    }
}