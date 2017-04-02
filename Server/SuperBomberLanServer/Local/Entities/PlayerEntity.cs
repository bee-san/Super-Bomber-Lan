using System;
using System.Collections.Generic;
using System.Text;

namespace SuperBomberLanServer.Local.Entities
{
    class PlayerEntity : IEntity
    {
        public EntityType EntityType { get; } = EntityType.Player;

        public string ConnectionId { get; set; }
    }
}
