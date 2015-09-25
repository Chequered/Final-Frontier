using System;
using System.Collections.Generic;

using UnityEngine;

namespace FinalFrontier
{
    namespace Graphics
    {
        public class TerrainTileGraphics : GraphicsBase
        {
            private Color _primaryColor;
            public TerrainTileGraphics()
            {
                _textureData = new List<Color[]>();
                folder = "tiles";
            }

            public void GeneratePrimaryColor()
            {
                Color result = new Color();
                for (int i = 0; i < _textureData.Count; i++)
                {
                    for (int c = 0; c < _textureData[i].Length; c++)
                    {
                        result += _textureData[i][c];
                    }
                }
                result = result / ((variants * TerrainTileGraphics.TEXTURE_RESOLUTION) * TerrainTileGraphics.TEXTURE_RESOLUTION);
                _primaryColor = result;
            }

            public Color primaryColor
            {
                get
                {
                    if (_primaryColor == null)
                        GeneratePrimaryColor();

                    return _primaryColor;
                }
            }
        }
    }
}
