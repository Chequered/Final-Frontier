using System;
using System.Collections.Generic;

using UnityEngine;

using EndlessExpedition.Entities;

namespace EndlessExpedition
{
    namespace Graphics
    {
        public class TerrainTileGraphics : GraphicsBase
        {
            private Color m_primaryColor;
            public TerrainTileGraphics()
            {
                p_textureData = new List<Color[]>();
                p_folder = "tiles";
            }

            public void GeneratePrimaryColor()
            {
                Color result = new Color();
                for (int i = 0; i < p_textureData.Count; i++)
                {
                    for (int c = 0; c < p_textureData[i].Length; c++)
                    {
                        result += p_textureData[i][c];
                    }
                }
                result = result / ((variants * TerrainTileGraphics.TILE_TEXTURE_RESOLUTION) * TerrainTileGraphics.TILE_TEXTURE_RESOLUTION);
                m_primaryColor = result;
            }

            public Color primaryColor
            {
                get
                {
                    if (m_primaryColor == null)
                        GeneratePrimaryColor();

                    return m_primaryColor;
                }
            }
        }
    }
}
