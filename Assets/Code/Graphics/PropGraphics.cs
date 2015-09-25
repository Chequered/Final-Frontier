using System;
using System.Collections.Generic;
using System.IO;

using UnityEngine;

namespace FinalFrontier
{
    namespace Graphics
    {
        public class PropGraphics : GraphicsBase
        {
            public PropGraphics()
            {
                _textureData = new List<Color[]>();
                folder = "entities/props";
            }
        }
    }
}
