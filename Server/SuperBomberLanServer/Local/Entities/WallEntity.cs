using System;
using System.Collections.Generic;
using System.Text;

namespace SuperBomberLanServer.Local.Entities
{
    public class WallEntity : IEntity
    {
        public EntityType EntityType { get; } = EntityType.Wall;
    }
}
