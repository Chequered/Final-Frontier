using System;
using System.Collections.Generic;
using System.IO;

using UnityEngine;

using FinalFrontier.Serialization;
using FinalFrontier.Entities;

namespace FinalFrontier
{
    namespace Graphics
    {
        public class BuildingGraphics : GraphicsBase
        {
            public BuildingGraphics(Entity entity) : base(entity)
            {
                p_textureData = new List<Color[]>();
                p_folder = "entities/buildings";
            }

            public override void LoadFrom(string fileName, Properties properties)
            {
                string[] split = fileName.Split('.');
                fileName = split[0];

                string dataPath = FinalFrontier.Serialization.Properties.dataRootPath + p_folder + "/" + fileName + ".png";

                Texture2D tex = new Texture2D(TILE_TEXTURE_RESOLUTION * tileWidth, TILE_TEXTURE_RESOLUTION * tileHeight);
                tex.LoadImage(File.ReadAllBytes(dataPath));

                p_textureData.Add(tex.GetPixels(0, 0, TILE_TEXTURE_RESOLUTION * tileWidth, TILE_TEXTURE_RESOLUTION * tileHeight));
            }

            public override Texture2D texture(int variant = 0)
            {
                Texture2D tex = new Texture2D(TILE_TEXTURE_RESOLUTION * tileWidth, TILE_TEXTURE_RESOLUTION * tileHeight);
                tex.filterMode = FilterMode.Point;
                tex.SetPixels(p_textureData[variant]);
                tex.Apply();
                return tex;
            }

            public override Sprite randomSprite
            {
                get
                {
                    Sprite result = Sprite.Create(texture(), new Rect(0, 0, TILE_TEXTURE_RESOLUTION * tileWidth, TILE_TEXTURE_RESOLUTION * tileHeight), new Vector2(0.5f, 0.5f));
                    return result;
                }
            }
        }
    }
}
