using System;
using System.Collections.Generic;
using System.IO;

using UnityEngine;

using EndlessExpedition.Entities;

namespace EndlessExpedition
{
    namespace Graphics
    {
        public class PropGraphics : GraphicsBase
        {
            public PropGraphics(Entity entity) : base(entity)
            {
                p_textureData = new List<Color[]>();
                p_folder = "entities/props";
            }
        }
    }
}
