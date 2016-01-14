using System;
using System.Collections.Generic;

using UnityEngine;

using EndlessExpedition.Serialization;
using EndlessExpedition.Managers;
using EndlessExpedition.Graphics;

namespace EndlessExpedition
{
    namespace Entities
    {
        public class Prop : Entity
        {
            private PropGraphics m_graphics;
            
            public override void OnStart()
            {
                base.OnStart();
                m_graphics = new PropGraphics(this);

                properties.Secure("tileWidth", 1);
                properties.Secure("tileHeight", 1);
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
                return m_graphics;
            }

            public override void SetGraphics(GraphicsBase graphics)
            {
                m_graphics = graphics as PropGraphics;
            }
        }
    }
}
