﻿using _Project.Codebase.Gameplay.World;

namespace _Project.Codebase.Gameplay.Items
{
    public abstract class Item : ICollectable
    {
        public ItemType type;

        public Item(ItemType type)
        {
            this.type = type;
        }
        
        public virtual void Collect(ICollector collector)
        {
            collector.PickUpCollectable(this);
        }
    }
}