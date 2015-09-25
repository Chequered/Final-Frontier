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
        //Interfaces
        public interface IEntity
        {
            void OnStart();
            void OnUpdate();
            void OnTick();
        }

        //Serializable enums
        public class EntityMovementMode
        {
            public const string Static = "static";
            public const string Dynamic = "dynamic";
        }

        public class DestroyMode
        {
            public const string Vanish = "vanish";
            public const string Particles = "particles";
            public const string Explode = "explode";
        }

        //Classes
        //GameObjects can only be destroyed inside a monobehaviour script
        internal class GameObjectDestroyer : MonoBehaviour
        {
            public void Destroy()
            {
                Destroy(this.gameObject);
            }
        }

        public abstract class Entity : IEntity, ICloneable
        {
            //Clone
            public object Clone()
            {
                return this.MemberwiseClone();
            }

            //References
            protected EntityManager _EM;
            protected GameObject _gameObject;

            //Properties & Graphics
            protected Properties _properties;

            //active vars
            private bool selected = false;

            /// <summary>
            /// Called when starting the manager
            /// </summary>
            public virtual void OnStart()
            {
                //Set default properties
                //TODO read out of XML file
                if(_properties == null)
                    _properties = new Properties("entities");

                _properties.Secure("type", "entity");
                _properties.Secure("identity", "entity");
                _properties.Secure("displayName", "Unamed Entity");
                _properties.Secure("x", 0);
                _properties.Secure("y", 0);
                _properties.Secure("traversable", true);
                _properties.Secure("movementMode", EntityMovementMode.Static);

                _EM = ManagerInstance.Get<EntityManager>();
            }

            /// <summary>
            /// called each frame while the game is running
            /// </summary>
            public virtual void OnTick()
            {

            }

            /// <summary>
            /// called each frame while the game is not paused
            /// </summary>
            public virtual void OnUpdate()
            {

            }

            /// <summary>
            /// Called when the managers are loaded. use this when loading your graphics.
            /// </summary>
            public virtual void OnLoad()
            {

            }

            public virtual void OnSelect()
            {
                ManagerInstance.Get<UIManager>().InspectPropeties(_properties);
                selected = true;
                //TODO: turn outline on
            }

            public virtual void OnDeselect()
            {
                ManagerInstance.Get<UIManager>().ClosePropertyInspector();
                selected = false;
                //TODO: turn outline off
            }

            //Unity Events
            public void OnMouseOver()
            {

            }

            public void OnMouseEnter()
            {
                OnSelect();
            }

            public void OnMouseExit()
            {
                OnDeselect();
            }

            public void SetupCollision()
            {
                if(_gameObject == null)
                    return;

                _gameObject.AddComponent<BoxCollider2D>();
                _gameObject.AddComponent<EntityCollision>().entity = this;
            }

            /// <summary>
            /// Destroy this entity
            /// </summary>
            protected void Destroy(DestroyMode mode)
            {
                _EM.UnRegisterEntity(this);
                _gameObject.AddComponent<GameObjectDestroyer>().Destroy();
            }

            //Position
            public void GoToWorldPos(int x, int y)
            {
                _properties.Set("x", x);
                _properties.Set("y", y);
                _gameObject.transform.position = new Vector3(x - 7.5f, y - 7.5f, _gameObject.transform.position.z);
            }

            //Getters & Setters
            public Properties properties
            {
                get
                {
                    if(_properties == null)
                        _properties = new Properties("entities");

                    return _properties;
                }
            }

            public GraphicsBase GenerateGraphics<T>() where T : Entity
            {
                Type type = typeof(T);
                GraphicsBase result = null;

                if(type == typeof(Actor))
                {
                    result = new ActorGraphics();
                }else if(type == typeof(Prop))
                {
                    result = new PropGraphics();
                }

                return result;
            }

            public GameObject gameObject
            {
                get
                {
                    return _gameObject;
                }
                set
                {
                    _gameObject = value;
                }
            }

            public abstract GraphicsBase GetGraphics();
            public abstract void SetGraphics(GraphicsBase graphics);
        }
    }
}
