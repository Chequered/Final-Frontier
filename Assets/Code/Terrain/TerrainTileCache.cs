using System;
using System.Collections.Generic;
using UnityEngine;
using EndlessExpedition.Serialization;
using EndlessExpedition.Graphics;

namespace EndlessExpedition
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
