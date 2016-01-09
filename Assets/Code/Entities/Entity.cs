using System;
using System.Collections.Generic;

using UnityEngine;

using FinalFrontier.Serialization;
using FinalFrontier.Managers;
using FinalFrontier.Graphics;
using FinalFrontier.UI;

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

        public enum EntityPositionStatus
        {
            InAir,
            OnGround
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
            public const int LAYER_ON_GROUND = 9;
            public const int LAYER_IN_AIR = 0;
            public const float ON_GROUND_HEIGHT = 0.1f;

            //Clone
            public object Clone(){return this.MemberwiseClone();}

            //References
            protected EntityManager p_EM;
            protected GameObject p_gameObject;

            //Properties & Graphics
            protected Properties p_properties;

            //active vars
            private bool m_selected = false;

            //UI
            protected EntityUIGroup p_UIGroup;
            protected bool p_mouseOver = false;

            //delegates
            public delegate void EntityEventHandler(Entity entity);
            public delegate void EntitySelectionEventHandler(Entity entity, bool state);
            public EntityEventHandler OnStartEvent;
            public EntityEventHandler OnTickEvent;
            public EntityEventHandler OnUpdateEvent;
            public EntitySelectionEventHandler OnSelectEvent;
            public EntitySelectionEventHandler OnDeselectEvent;
            public EntityEventHandler OnDestroyEvent;

            /// <summary>
            /// Called when starting the manager
            /// </summary>
            public virtual void OnStart()
            {
                if(p_properties == null)
                    p_properties = new Properties("entities");

                //create reference
                p_EM = ManagerInstance.Get<EntityManager>();

                if (OnStartEvent != null)
                    OnStartEvent(this);

                p_UIGroup = new EntityUIGroup();
            }

            /// <summary>
            /// called each frame while the game is running
            /// </summary>
            public virtual void OnTick()
            {
                if (OnTickEvent != null)
                    OnTickEvent(this);
            }

            /// <summary>
            /// called each frame while the game is not paused
            /// </summary>
            public virtual void OnUpdate()
            {
                if (OnUpdateEvent != null)
                    OnUpdateEvent(this);
            }

            /// <summary>
            /// Called when the managers are loaded. use this when loading your graphics.
            /// </summary>
            public virtual void OnLoad()
            {

            }

            public virtual void OnSelect()
            {
                ManagerInstance.Get<UIManager>().propertyInspector.SetInspectingEntity(this);
                m_selected = true;
                //TODO: turn outline on

                if (OnSelectEvent != null)
                    OnSelectEvent(this, true);

                p_UIGroup.Toggle(true);
            }

            public virtual void OnDeselect()
            {
                ManagerInstance.Get<UIManager>().propertyInspector.Close();
                m_selected = false;
                //TODO: turn outline off

                if (OnDeselectEvent != null)
                    OnDeselectEvent(this, false);

                p_UIGroup.Toggle(false);
            }

            //Unity Events
            public virtual void OnMouseOver()
            {

            }

            public virtual void OnMouseEnter()
            {
                p_mouseOver = true;
            }

            public virtual void OnMouseExit()
            {
                p_mouseOver = false;
            }
            
            public void SetupCollision()
            {
                if(p_gameObject == null)
                    return;

                //p_gameObject.AddComponent<BoxCollider2D>();
                p_gameObject.AddComponent<EntityCollision>().entity = this;
            }

            /// <summary>
            /// Destroy this entity
            /// </summary>
            //TODO: use destroy mode
            protected void Destroy(DestroyMode mode)
            {
                p_EM.UnregisterEntity(this);
                p_gameObject.AddComponent<GameObjectDestroyer>().Destroy();

                if (OnDestroyEvent != null)
                    OnDestroyEvent(this);
            }

            //Position
            public void GoToGamePos(int x, int y)
            {
                p_properties.Set("x", x);
                p_properties.Set("y", y);
                p_gameObject.transform.position = new Vector3(
                    (x - FinalFrontier.Terrain.TerrainChunk.SIZE / 2) + ((float)GetGraphics().tileWidth / 2),
                    (y - FinalFrontier.Terrain.TerrainChunk.SIZE / 2) + ((float)GetGraphics().tileHeight / 2), 
                    gameObject.transform.position.z);
            }

            //Getters & Setters
            public Properties properties
            {
                get
                {
                    if(p_properties == null)
                    {
                        p_properties = new Properties("entities");
                    }

                    return p_properties;
                }
            }
            public string identity
            {
                get
                {
                    return p_properties.Get<string>("identity");
                }
            }
            public bool isSelected
            {
                get
                {
                    return m_selected;
                }
            }
            public Vector2 tilePosition
            {
                get
                {
                    return new Vector2(properties.Get<int>("x"), properties.Get<int>("y"));
                }
            }
            public Vector2 unityPosition
            {
                get
                {
                    Vector2 result = Vector2.zero;
                    result.x = (tilePosition.x - FinalFrontier.Terrain.TerrainChunk.SIZE / 2) + ((float)GetGraphics().tileWidth / 2);
                    result.y = (tilePosition.y - FinalFrontier.Terrain.TerrainChunk.SIZE / 2) + ((float)GetGraphics().tileHeight / 2);
                    return result;
                }
            }
            public EntityPositionStatus positionStatus
            {
                set
                {
                    if (value == EntityPositionStatus.InAir)
                        p_gameObject.layer = Entity.LAYER_IN_AIR;
                    else
                    {
                        Vector3 groundPos = p_gameObject.transform.position;
                        groundPos.z = -ON_GROUND_HEIGHT;
                        p_gameObject.transform.position = groundPos;
                    }
                }
            }

            public GraphicsBase GenerateGraphics<T>() where T : Entity
            {
                Type type = typeof(T);
                GraphicsBase result = null;

                if(type == typeof(Actor))
                {
                    result = new ActorGraphics(this);
                }else if(type == typeof(Prop))
                {
                    result = new PropGraphics(this);
                }
                else if (type == typeof(Building))
                {
                    result = new BuildingGraphics(this);
                }

                return result;
            }

            public GameObject gameObject
            {
                get
                {
                    return p_gameObject;
                }
                set
                {
                    p_gameObject = value;
                }
            }

            public abstract GraphicsBase GetGraphics();
            public abstract void SetGraphics(GraphicsBase graphics);

            //static helpers
            public static Vector2 ConvertGameToUnityPosition(Vector2 tilePosition, int tileWidth, int tileHeight)
            {
                Vector2 result = Vector2.zero;
                result.x = (tilePosition.x - FinalFrontier.Terrain.TerrainChunk.SIZE / 2) + ((float)tileWidth / 2);
                result.y = (tilePosition.y - FinalFrontier.Terrain.TerrainChunk.SIZE / 2) + ((float)tileHeight / 2);
                return result;
            }
        }
    }
}
