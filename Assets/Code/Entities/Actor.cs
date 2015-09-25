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
            public override void OnStart()
            {
                base.OnStart();
                _properties.Set("identity", "John Cena");
                _properties.Set("entityType", "actor");
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
                throw new NotImplementedException();
            }

            public override void SetGraphics(GraphicsBase graphics)
            {
                throw new NotImplementedException();
            }
        }
    }
}