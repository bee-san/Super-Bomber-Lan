using System;
using System.Collections.Generic;
using System.Text;

namespace SuperBomberLanServer.Local
{
    public interface IEntity
    {
        EntityType EntityType { get; }
    }

    public enum EntityType
    {
        Player = 0,
        Bomb = 1,
        Wall = 2,
        Box = 3
    }
}
