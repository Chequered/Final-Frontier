using System;
using System.Collections.Generic;

using UnityEngine;

using EndlessExpedition.Serialization;
using EndlessExpedition.Managers;
using EndlessExpedition.Graphics;
using EndlessExpedition.UI;
using EndlessExpedition.Terrain;
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
            protected List<Entity> p_spawnedEntities;
            private List<EntityBehaviourScript> m_behaviourScripts;

            //Properties & Graphics
            protected Properties p_properties;

            //active vars
            protected bool p_selected = false;
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
                
                p_spawnedEntities = new List<Entity>();

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
                if(properties.Has("spawnOnStart"))
                {
                    string[] split = properties.Get<string>("spawnOnStart").Split('/');
                    for (int i = 0; i < split.Length; i++)
                    {
                        string type = split[i].Split(':')[0];
                        string id = split[i].Split(':')[1];

                        Entity prefab = null;
                        Entity entity = null;

                        switch (type)
	                    {
                            case "Actor":
                                prefab = ManagerInstance.Get<EntityManager>().FindFromCache<Actor>(id);
                                if (prefab != null)
                                {
                                    entity = ManagerInstance.Get<EntityManager>().CreateEntity<Actor>(prefab, tilePosition.x, tilePosition.y);
                                } 
                                break;
                            case "Prop":
                                prefab = ManagerInstance.Get<EntityManager>().FindFromCache<Prop>(id);
                                if (prefab != null)
                                {
                                    entity = ManagerInstance.Get<EntityManager>().CreateEntity<Prop>(prefab, tilePosition.x, tilePosition.y);
                                }
                                break;
                            case "Building":
                                prefab = ManagerInstance.Get<EntityManager>().FindFromCache<Building>(id);
                                if (prefab != null)
                                {
                                    entity = ManagerInstance.Get<EntityManager>().CreateEntity<Building>(prefab, tilePosition.x, tilePosition.y);
                                }
                                break;
		                    default:
                            break;
                        }
                        if(entity != null)
                        {
                            entity.GenerateBehaviourScripts();
                            p_spawnedEntities.Add(entity);
                        }
                    }
                }
                #endregion
                GenerateBehaviourScripts();
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
            /// Called when the managers are loaded. use this when loading your graphics / instatiating vars.
            /// </summary>
            public virtual void OnLoad()
            {

            }

            public virtual void OnSelect()
            {
                p_selected = true;

                if (OnSelectEvent != null)
                    OnSelectEvent(this, true);

                p_UIGroup.Toggle(true);
            }

            public virtual void OnDeselect()
            {
                p_selected = false;


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

                if (GetType() == typeof(Building))
                    if (!(this as Building).IsBuilt)
                        m_light.enabled = false;
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
                    if (p_properties.Get<string>("movementMode") == "dynamic")
                        return;

                p_properties.Set("x", (int)x);
                p_properties.Set("y", (int)y);
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
            public void AddBehaviourScript(EntityBehaviourScript script)
            {
                if(m_behaviourScripts == null)
                    m_behaviourScripts = new List<EntityBehaviourScript>();

                m_behaviourScripts.Add(script);
            }
            public T GetBehaviourScript<T>() where T : EntityBehaviourScript
            {
                if (m_behaviourScripts == null)
                    return default(T);
                for (int i = 0; i < m_behaviourScripts.Count; i++)
                {
                    if (m_behaviourScripts[i].GetType() == typeof(T))
                        return (T)m_behaviourScripts[i] as T;
                }
                return default(T);
            }
            public void RemoveBehaviourScript<T>() where T : EntityBehaviourScript
            {
                if (m_behaviourScripts == null)
                    return;
                for (int i = 0; i < m_behaviourScripts.Count; i++)
                {
                    if (m_behaviourScripts[i].GetType() == typeof(T))
                    {
                        m_behaviourScripts[i].RemoveFromEntity(this);
                        m_behaviourScripts.RemoveAt(i);
                        return;
                    }
                }
            }
            public void RemoveBehaviourScript(EntityBehaviourScript script)
            {
                if (m_behaviourScripts == null)
                    return;
                if (m_behaviourScripts.Contains(script))
                {
                    m_behaviourScripts.Remove(script);
                    script.RemoveFromEntity(this);
                }
            }

            #region Getters & Setters
            public EntityBehaviourScript[] BehaviourScripts
            {
                get
                {
                    return m_behaviourScripts.ToArray();
                }
            }
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
            public string Identity
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
                    return p_selected;
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
                    return gameObject.transform.position;
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
            public EntityUIGroup uiGroup
            {
                get
                {
                    return p_UIGroup;
                }
            }
            public void GenerateBehaviourScripts()
            {
                if(properties.Has("entityBehaviourScripts"))
                {
                    string[] names = properties.Get<string>("entityBehaviourScripts").Split('/');
                    EntityBehaviourScript[] scripts = new EntityBehaviourScript[names.Length];

                    for (int i = 0; i < names.Length; i++)
                    {
                        scripts[i] = EntityBehaviourScript.CreateInstanceOf(names[i]);

                        if (scripts[i] != null)
                            scripts[i].AttachToEntity(this);
                    }
                }
            }

            public abstract GraphicsBase GetGraphics();
            public abstract void SetGraphics(GraphicsBase graphics);
            #endregion

            //static helpers
            public static Vector2 ConvertGameToUnityPosition(Vector2 tilePosition, int tileWidth, int tileHeight)
            {
                Vector2 result = Vector2.zero;
                result.x = (tilePosition.x - TerrainChunk.SIZE / 2) + ((float)tileWidth / 2);
                result.y = (tilePosition.y - TerrainChunk.SIZE / 2) + ((float)tileHeight / 2);
                return result + new Vector2(TerrainChunk.SIZE / 2 - 0.5f, TerrainChunk.SIZE / 2 - 0.5f);
            }
        }
    }
}
