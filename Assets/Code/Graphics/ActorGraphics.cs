using System.Collections.Generic;

namespace FinalFrontier
{
    namespace Graphics
    {
        public class ActorGraphics : GraphicsBase
        {
            public ActorGraphics()
            {
                _textureData = new List<UnityEngine.Color[]>();
                folder = "entities/actors";
            }
        }
    }
}
