using System.Collections.Generic;
using System.IO;

using UnityEngine;

namespace FinalFrontier
{
    namespace Graphics
    {
        public abstract class GraphicsBase
        {
            public const int TEXTURE_RESOLUTION = 32;

            public int variants;
            protected List<Color[]> _textureData;
            protected string folder = "unspecified";

            public void LoadFrom(string fileName)
            {
                string[] split = fileName.Split('.');
                fileName = split[0];

                string dataPath = FinalFrontier.Serialization.Properties.dataRootPath + folder + "/" + fileName + ".png";

                Texture2D tex = new Texture2D(TEXTURE_RESOLUTION * variants, TEXTURE_RESOLUTION);
                tex.LoadImage(File.ReadAllBytes(dataPath));

                for (int i = 0; i < variants; i++)
                {
                    _textureData.Add(tex.GetPixels(i * TEXTURE_RESOLUTION, 0, TEXTURE_RESOLUTION, TEXTURE_RESOLUTION));
                }
            }

            public Color[] GetTextureData(int variant = 0)
            {
                return _textureData[variant];
            }

            public Texture2D texture(int variant = 0)
            {
                Texture2D tex = new Texture2D(TEXTURE_RESOLUTION, TEXTURE_RESOLUTION);
                if (variant < _textureData.Count)
                {
                    tex.filterMode = FilterMode.Point;
                    tex.SetPixels(_textureData[variant]);
                    tex.Apply();
                }
                return tex;
            }

            public Sprite randomSprite
            {
                get
                {
                    Sprite result = Sprite.Create(texture(Random.Range(0, variants)), new Rect(0, 0, TEXTURE_RESOLUTION, TEXTURE_RESOLUTION), new Vector2(0.5f, 0.5f));
                    return result;
                }
            }
        }
    }
}
