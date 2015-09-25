using System;
using System.Collections.Generic;

using UnityEngine;

using FinalFrontier.Serialization;
using FinalFrontier.Managers;
using FinalFrontier.Graphics;

namespace FinalFrontier
{
    namespace Entities
    {
        public class Prop : Entity
        {
            private PropGraphics _graphics;
            
            public override void OnStart()
            {
                base.OnStart();
                _graphics = new PropGraphics();
            }

            public override void OnTick()
            {
                base.OnTick();
            }

            public override void OnUpdate()
            {
                base.OnUpdate();
            }

            public override GraphicsBase GetGraphics()
            {
                return _graphics;
            }

            public override void SetGraphics(GraphicsBase graphics)
            {
                _graphics = graphics as PropGraphics;
            }
        }
    }
}
