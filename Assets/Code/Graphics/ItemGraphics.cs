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
        public enum ItemIconType
        {
            Default,
            Deficit
        }

        public class ItemGraphics : GraphicsBase
        {
            public ItemGraphics()
            {
                p_textureData = new List<Color[]>();
                p_folder = "items";
            }

            public override void LoadFrom(string fileName, Properties properties)
            {
                string[] split = fileName.Split('.');
                fileName = split[0];

                string dataPath = FinalFrontier.Serialization.Properties.dataRootPath + p_folder + "/" + fileName + ".png";

                Texture2D tex = new Texture2D(ICON_RESOLUTION, ICON_RESOLUTION );
                tex.LoadImage(File.ReadAllBytes(dataPath));

                //load default icon
                p_textureData.Add(tex.GetPixels(0, 0, ICON_RESOLUTION, ICON_RESOLUTION));

                //load deficit icon
                p_textureData.Add(tex.GetPixels(ICON_RESOLUTION, 0, ICON_RESOLUTION, ICON_RESOLUTION));
            }

            public override Texture2D texture(int variant = 0)
            {
                Texture2D tex = new Texture2D(ICON_RESOLUTION, ICON_RESOLUTION);
                tex.filterMode = FilterMode.Point;
                tex.SetPixels(p_textureData[variant]);
                tex.Apply();
                return tex;
            }

            public override Sprite icon
            {
                get
                {
                    Sprite result = Sprite.Create(texture(0), new Rect(0, 0, ICON_RESOLUTION, ICON_RESOLUTION), new Vector2(0.5f, 0.5f));
                    return result;
                }
            }

            public Sprite GetIcon(ItemIconType type)
            {
                if(type == ItemIconType.Default)
                    return Sprite.Create(texture(0), new Rect(0, 0, ICON_RESOLUTION, ICON_RESOLUTION), new Vector2(0.5f, 0.5f));
                else
                    return Sprite.Create(texture(1), new Rect(0, 0, ICON_RESOLUTION, ICON_RESOLUTION), new Vector2(0.5f, 0.5f));
            }
        }
    }
}
