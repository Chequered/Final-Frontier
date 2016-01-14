using System;
using System.Collections.Generic;

using UnityEngine;

using EndlessExpedition.Terrain;

namespace EndlessExpedition
{
    namespace Graphics
    {
        public class TerrainTileGraphicsCached
        {
            public Color[] textureData;

            public static int results = 0;

            public string identity;
            public string left;
            public string right;
            public string bottom;
            public string top;

            public bool IsAs(TerrainTileNeighborInfo other)
            {
                bool result = false;
                if (identity == other.identity
                    && left == other.left
                    && top == other.top
                    && right == other.right
                    && bottom == other.bottom)
                {
                    result = true;
                    results++;
                }
                return result;
            }

            public void CopyNeighborInfo(TerrainTileNeighborInfo other)
            {
                identity = "" + other.identity;
                left = "" + other.left;
                top = "" + other.top;
                right = "" + other.right;
                bottom = "" + other.bottom;
            }
        }
    }
}
