using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using SuperBomberLanServer.Local.Entities;

namespace SuperBomberLanServer.Local
{
    public class Map
    {
        [JsonProperty("width")]
        public Int32 Width { get; internal set; }

        [JsonProperty("height")]
        public Int32 Height { get; internal set; }

        [JsonProperty("tiles")]
        public Tile[,] Tiles { get; internal set; }

        public static Map CreateMap(int width, int height)
        {
            Map map = new Map
            {
                Width = width,
                Height = height,
                Tiles = new Tile[width, height]
            };

            CreateTiles(ref map);
            CreateWalls(ref map);

            return map;
        }

        private static void CreateTiles(ref Map map)
        {
            for (int i = 0; i < map.Width; i++)
            {
                for (int j = 0; j < map.Height; j++)
                {
                    map.Tiles[i, j] = new Tile
                    {
                        IsSafe = i <= 10 && j <= 10 //For now the top left 10x10 area
                    };
                }
            }
        }

        private static void CreateWalls(ref Map map)
        {
            for (int i = 0; i < map.Width; i++)
            {
                for (int j = 0; j < map.Height; j++)
                {
                    if (i % 2 == 0 && j % 2 == 0)
                    {
                        map.Tiles[i, j].Entities.Add(new WallEntity());
                    }
                }
            }
        }
    }
}
