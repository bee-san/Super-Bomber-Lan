using System;
using System.Collections.Generic;
using System.Text;

namespace SuperBomberLanServer.Local
{
    public class Tile
    {
        public List<IEntity> Entities { get; set; } = new List<IEntity>();

        public Boolean IsSafe { get; set; }
    }
}
