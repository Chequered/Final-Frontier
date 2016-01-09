using System.Collections.Generic;
using System.IO;

using UnityEngine;

using FinalFrontier.Serialization;
using FinalFrontier.Entities;

namespace FinalFrontier
{
    namespace Graphics
    {
        public abstract class GraphicsBase
        {
            public const int TILE_TEXTURE_RESOLUTION = 32;
            public const int ICON_RESOLUTION = 16;

            public int variants;
            public int tileWidth = 1;
            public int tileHeight = 1;
            
            protected List<Color[]> p_textureData;
            protected string p_folder = "unspecified";
            protected Entity p_entity;

            public GraphicsBase(Entity entity = null)
            {
                if(entity != null)
                    p_entity = entity;
            }

            public virtual void LoadFrom(string fileName, Properties properties)
            {
                string[] split = fileName.Split('.');
                fileName = split[0];

                string dataPath = FinalFrontier.Serialization.Properties.dataRootPath + p_folder + "/" + fileName + ".png";

                Texture2D tex = new Texture2D(TILE_TEXTURE_RESOLUTION * variants, TILE_TEXTURE_RESOLUTION);
                tex.LoadImage(File.ReadAllBytes(dataPath));

                for (int i = 0; i < variants; i++)
                {
                    p_textureData.Add(tex.GetPixels(i * TILE_TEXTURE_RESOLUTION, 0, TILE_TEXTURE_RESOLUTION, TILE_TEXTURE_RESOLUTION));
                }
            }

            public Color[] GetTextureData(int variant = 0)
            {
                return p_textureData[variant];
            }
            
            public virtual Texture2D texture(int variant = 0)
            {
                Texture2D tex = new Texture2D(TILE_TEXTURE_RESOLUTION, TILE_TEXTURE_RESOLUTION);
                if (variant < p_textureData.Count)
                {
                    tex.filterMode = FilterMode.Point;
                    tex.SetPixels(p_textureData[variant]);
                    tex.wrapMode = TextureWrapMode.Clamp;
                    tex.Apply();
                }
                return tex;
            }

            public virtual Sprite randomSprite
            {
                get
                {
                    Sprite result = Sprite.Create(texture(Random.Range(0, variants)), new Rect(0, 0, TILE_TEXTURE_RESOLUTION, TILE_TEXTURE_RESOLUTION), new Vector2(0.5f, 0.5f));
                    return result;
                }
            }

            public virtual Sprite icon
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
