using UnityEngine;
using System.Collections;

namespace EndlessExpedition
{
    namespace Entities
    {
        /// <summary>
        /// Component attached to entity gameobjects, this will fire off mouse and on entity events
        /// </summary>
        public class EntityCollision : MonoBehaviour
        {
            public Entity entity;

            private bool m_mouseOver = false;
            private bool m_mouseDownWasOnUI = false;

            public void OnMouseEnter()
            {
                if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                    return;

                m_mouseOver = true;
                entity.OnMouseEnter();
            }

            public void OnMouseOver()
            {
                if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                    return;

                entity.OnMouseOver();
            }

            public void OnMouseExit()
            {
                m_mouseOver = false;
                entity.OnMouseExit();
            }

            public void OnClick()
            {
                m_mouseDownWasOnUI = true;

                if (m_mouseOver)
                    entity.OnSelect();
            }
        }
    }
}
