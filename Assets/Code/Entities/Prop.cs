using System;
using System.Collections.Generic;

using UnityEngine;

using EndlessExpedition.Serialization;
using EndlessExpedition.Managers;
using EndlessExpedition.Graphics;
using EndlessExpedition.UI;

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

                EntityPropertyDisplay display = new EntityPropertyDisplay();
                display.properties = p_properties;
                display.displaySize = new Vector2(400, 15);
                display.AddDisplay("Name: ", "displayName");
                display.AddDisplay("Description: ", "description");
                display.AddDisplay("Type: ", "propType");
                display.menuName = properties.Get<string>("displayName") + ": Propety Menu";
                display.BuildUI();
                display.Toggle(false);
                ManagerInstance.Get<UIManager>().AddUI(display);
                p_UIGroup.AddUIElement(display);
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
