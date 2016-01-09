using UnityEngine;

using System.Collections.Generic;

using FinalFrontier.Entities;

namespace FinalFrontier
{
    namespace Graphics
    {
        public class ActorGraphics : GraphicsBase
        {
            public ActorGraphics(Entity entity) : base(entity)
            {
                p_textureData = new List<UnityEngine.Color[]>();
                p_folder = "entities/actors";
            }
        }
    }
}
