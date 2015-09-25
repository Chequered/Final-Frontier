using UnityEngine;
using System.Collections;

namespace FinalFrontier
{
    namespace Entities
    {
        public class EntityCollision : MonoBehaviour
        {
            public Entity entity;

            public void OnMouseEnter()
            {
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
                entity.OnMouseExit();
            }
        }
    }
}
