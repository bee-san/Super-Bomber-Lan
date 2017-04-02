using System;
using System.Collections.Generic;
using System.Text;

namespace SuperBomberLanServer.Local.Entities
{
    class BombEntity : IEntity
    {
        public EntityType EntityType { get; } = EntityType.Bomb;

        public Int32 Radius { get; } = 3;
    }
}
