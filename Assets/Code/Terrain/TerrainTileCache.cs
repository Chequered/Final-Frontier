using System;
using System.Collections.Generic;
using UnityEngine;
using FinalFrontier.Serialization;
using FinalFrontier.Graphics;

namespace FinalFrontier
{
    namespace Terrain
    {
        public class TerrainTileCache
        {
            public Properties properties;
            public TerrainTileGraphics graphics;

            public TerrainTileCache()
            {
                properties = new Properties("terrainTiles");
                graphics = new TerrainTileGraphics();
            }
        }
    }
}
