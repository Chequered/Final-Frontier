using System;
using System.Collections.Generic;

using UnityEngine;

using FinalFrontier.Managers;
using FinalFrontier.Graphics;

namespace FinalFrontier
{
    namespace Entities
    {
        public class Actor : Entity
        {
            private ActorGraphics m_graphics;

            public override void OnStart()
            {
                base.OnStart();
                p_properties.Set("identity", "unnamedActor");
                p_properties.Set("entityType", "actor");

                m_graphics = new ActorGraphics(this);
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
                m_graphics = graphics as ActorGraphics;
            }
        }
    }
}