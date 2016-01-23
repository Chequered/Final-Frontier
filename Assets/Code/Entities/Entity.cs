using System;
using System.Collections.Generic;

using UnityEngine;

using EndlessExpedition.Serialization;
using EndlessExpedition.Managers;
using EndlessExpedition.Graphics;
using EndlessExpedition.UI;
using EndlessExpedition.Items;

namespace EndlessExpedition
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

            //Constants
            private const float LIGHT_HEIGHT = 1f;

            //References
            protected EntityManager p_EM;
            protected GameObject p_gameObject;
            private List<int> m_behaviourScriptIDs;

            //Properties & Graphics
            protected Properties p_properties;

            //active vars
            private bool m_selected = false;
            private Light m_light;
            private ItemContainer m_itemContainer;

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
                OnSelectEvent += ToggleSelectionEmission;
                OnDeselectEvent += ToggleSelectionEmission;

                #region Read properties
                if (properties.Has("itemContainerSlots"))
                {
                    AddItemContainerSpace(properties.Get<int>("itemContainerSlots"));
                }

                if (properties.Has("itemContainerSlots"))
                {
                    ItemContainerDisplay containerDisplay = new ItemContainerDisplay(itemContainer);
                    containerDisplay.BuildUI();
                    containerDisplay.Toggle(false);
                    containerDisplay.scale = new Vector2(1.25f, 1.25f);
                    ManagerInstance.Get<UIManager>().AddUI(containerDisplay);
                    p_UIGroup.AddUIElement(containerDisplay);

                    containerDisplay.position = new Vector2(Screen.width -containerDisplay.windowSize.x, 0);
                }
                #endregion
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
                m_behaviourScriptIDs = new List<int>();
            }

            public virtual void OnSelect()
            {
                //ManagerInstance.Get<UIManager>().propertyInspector.SetInspectingEntity(this);
                m_selected = true;

                if (OnSelectEvent != null)
                    OnSelectEvent(this, true);

                p_UIGroup.Toggle(true);
            }

            public virtual void OnDeselect()
            {
                //ManagerInstance.Get<UIManager>().propertyInspector.Close();
                m_selected = false;


                if (OnDeselectEvent != null)
                    OnDeselectEvent(this, false);

                p_UIGroup.Toggle(false);
            }

            public void Destroy()
            {
                if (OnDestroyEvent != null)
                    OnDestroyEvent(this);

                ManagerInstance.Get<EntityManager>().DestroyEntity(this);
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
            

            #region Generation methods
            public void GenerateCollision()
            {
                if(p_gameObject == null)
                    return;

                p_gameObject.AddComponent<EntityCollision>().entity = this;
            }

            public void GenerateLight()
            {
                GameObject lightObj = new GameObject("Light");
                lightObj.transform.SetParent(p_gameObject.transform, false);

                m_light = lightObj.AddComponent<Light>();
                m_light.type = LightType.Point;
                if (properties.Has("lightStrength"))
                    m_light.intensity = properties.Get<int>("lightStrength");
                else
                    m_light.intensity = 3;
                if (properties.Has("lightSize"))
                {
                    m_light.range = properties.Get<int>("lightSize");
                }
                else
                {
                    m_light.range = (p_properties.Get<int>("tileWidth") * p_properties.Get<int>("tileHeight")) + LIGHT_HEIGHT;
                    if (m_light.range < 1)
                        m_light.range = 1 + LIGHT_HEIGHT;
                }

                lightObj.transform.Translate(new Vector3(0, 0, -m_light.intensity / 2));

                if (properties.Has("lightColor"))
                {
                    string p = properties.Get<string>("lightColor");
                    string[] split = p.Split('/');
                    if (split.Length == 3)
                    {
                        float r = float.Parse(split[0]) / 255;
                        float g = float.Parse(split[1]) / 255;
                        float b = float.Parse(split[2]) / 255;
                        m_light.color = new Color(r, g, b);
                    }
                    else if (split.Length == 4)
                    {
                        float r = float.Parse(split[0]) / 255;
                        float g = float.Parse(split[1]) / 255;
                        float b = float.Parse(split[2]) / 255;
                        float a = float.Parse(split[3]) / 255;
                        m_light.color = new Color(r, g, b, a);
                    }
                }
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

            public GraphicsBase GenerateGraphics<T>() where T : Entity
            {
                Type type = typeof(T);
                GraphicsBase result = null;

                if (type == typeof(Actor))
                {
                    result = new ActorGraphics(this);
                }
                else if (type == typeof(Prop))
                {
                    result = new PropGraphics(this);
                }
                else if (type == typeof(Building))
                {
                    result = new BuildingGraphics(this);
                }

                return result;
            }
            #endregion
            public void AddItemContainerSpace(int slots)
            {
                if (m_itemContainer == null)
                    m_itemContainer = new ItemContainer(slots);
                else
                    m_itemContainer.AddSlots(slots);
            }
            public virtual void GoToGamePos(float x, float y, float z, bool forceStatic = false)
            {
                if (p_properties.Has("movementMode") && !forceStatic)
                    if(p_properties.Get<string>("movementMode") == "dynamic")
                        return;

                p_properties.Set("x", x);
                p_properties.Set("y", y);
                p_gameObject.transform.position = new Vector3(
                    (x - EndlessExpedition.Terrain.TerrainChunk.SIZE / 2) + ((float)GetGraphics().tileWidth / 2),
                    (y - EndlessExpedition.Terrain.TerrainChunk.SIZE / 2) + ((float)GetGraphics().tileHeight / 2),
                    z);
            }
            public void ToggleSelectionEmission(Entity entity, bool state)
            {
                if (state)
                {
                    gameObject.GetComponent<Renderer>().material.SetColor("_EmissionColor", new Color(0.25f, 0.25f, 0.25f));
                    gameObject.GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
                }
                else
                {
                    gameObject.GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.black);
                    gameObject.GetComponent<Renderer>().material.DisableKeyword("_EMISSION");
                }

            }

            //Getters & Setters
            #region Getters & Setters
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
                    result.x = (tilePosition.x - EndlessExpedition.Terrain.TerrainChunk.SIZE / 2) + ((float)GetGraphics().tileWidth / 2);
                    result.y = (tilePosition.y - EndlessExpedition.Terrain.TerrainChunk.SIZE / 2) + ((float)GetGraphics().tileHeight / 2);
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
            public Light light
            {
                get
                {
                    return m_light;
                }
            }
            public ItemContainer itemContainer
            {
                get
                {
                    return m_itemContainer;
                }
            }
            public int[] behaviourScriptIDs
            {
                get
                {
                    return m_behaviourScriptIDs.ToArray();
                }
            }


            public abstract GraphicsBase GetGraphics();
            public abstract void SetGraphics(GraphicsBase graphics);
            #endregion
            //static helpers
            public static Vector2 ConvertGameToUnityPosition(Vector2 tilePosition, int tileWidth, int tileHeight)
            {
                Vector2 result = Vector2.zero;
                result.x = (tilePosition.x - EndlessExpedition.Terrain.TerrainChunk.SIZE / 2) + ((float)tileWidth / 2);
                result.y = (tilePosition.y - EndlessExpedition.Terrain.TerrainChunk.SIZE / 2) + ((float)tileHeight / 2);
                return result;
            }
        }
    }
}
