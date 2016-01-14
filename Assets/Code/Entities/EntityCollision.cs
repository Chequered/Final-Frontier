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

            private void Update()
            {
                if(EndlessExpedition.Managers.GameManager.gameState == GameState.Playing)
                {
                    if(Input.GetMouseButtonUp(0))
                    {
                        if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                        {
                            if (m_mouseOver)
                                entity.OnSelect();
                            else if(entity.isSelected)
                            {
                                entity.OnDeselect();
                            }
                        }
                    }
                }
            }
        }
    }
}
