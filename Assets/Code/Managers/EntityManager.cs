using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Reflection;

using UnityEngine;

using EndlessExpedition.Entities;
using EndlessExpedition.Entities.BehvaiourScripts;
using EndlessExpedition.Terrain;
using EndlessExpedition.Graphics;
using EndlessExpedition.Serialization;
using EndlessExpedition.UI;
using EndlessExpedition.Managers.Base;

namespace EndlessExpedition
{
    namespace Managers
    {
        public class EntityManager : ManagerBase
        {
            private List<Entity> m_entityCache;
            private TerrainDataMap<bool> m_entityPlacementMap;

            private List<Entity> m_entities;
            private int m_entityCount;

            //behaviour scripts
            private List<EntityBehaviourScript> m_entityBehaviourScripts;

            public override void OnStart()
            {
                m_entityBehaviourScripts = new List<EntityBehaviourScript>();
                m_entityPlacementMap.UpdateAllOverlays();
                m_entityPlacementMap.ApplyAllOverlays();

                //if we are loading a savegame, instantiate our loaded entities
                if (GameManager.gameState == GameState.StartingSave)
                {
                    Properties[] loadedEntities = GameManager.saveDataContainer.saveGame.entityProperties.ToArray();
                    for (int i = 0; i < loadedEntities.Length; i++)
                    {
                        int x = loadedEntities[i].Get<int>("x");
                        int y = loadedEntities[i].Get<int>("y");
                        Entity entity;

                        switch (loadedEntities[i].Get<string>("type"))
                        {
                            case "actor":
                                entity = CreateEntity<Actor>(loadedEntities[i], x, y);
                                break;
                            case "prop":
                                entity = CreateEntity<Prop>(loadedEntities[i], x, y);
                                entity.properties.SetAll(loadedEntities[i]);
                                break;
                            case "building":
                                entity = CreateEntity<Building>(loadedEntities[i], x, y);
                                entity.properties.SetAll(loadedEntities[i]);
                                ManagerInstance.Get<BuildManager>().RegisterBuilding(x, y, entity as Building);
                                break;
                        }
                    }
                }
            }

            public override void OnTick()
            {
                for (int i = 0; i < m_entityCount; i++)
                {
                    m_entities[i].OnTick();
                }
            }

            public override void OnUpdate()
            {
                for (int i = 0; i < m_entityCount; i++)
                {
                    m_entities[i].OnUpdate();
                }
            }

            public override void OnLoad()
            {
                if (m_entityCache == null)
                    m_entityCache = new List<Entity>();

                LoadEntities("actors");
                LoadEntities("props");
                LoadEntities("buildings");

                //entity placement datamap
                m_entityPlacementMap = new TerrainDataMap<bool>();

                //creating overlay
                BuildableTerrainOverlay buildableOverlay = new BuildableTerrainOverlay();
                buildableOverlay.BuildOverlay();

                //registering overlay
                ManagerInstance.Get<UIManager>().AddTerrainOverlay(buildableOverlay);
                ManagerInstance.Get<InputManager>().AddEventListener(InputPressType.Up, KeyCode.F1, buildableOverlay.ToggleOverlay);

                //adding overlay to datamap
                m_entityPlacementMap.AddTerrainOverlay(buildableOverlay);

                //entity list & count
                m_entities = new List<Entity>();
                m_entityCount = 0;

                m_entityPlacementMap.SetAllData(false, false);

                for (int i = 0; i < m_entityCache.Count; i++)
                {
                    m_entityCache[i].OnLoad();
                }
            }

            public override void OnExit()
            {
                throw new NotImplementedException();
            }

            public Entity CreateEntity<T>(Entity entityPrefab, float x, float y, EntityBehaviourScript[] behaviourScripts = null) where T : Entity
            {
                if (GameManager.gameState != GameState.StartingNew && GameManager.gameState != GameState.Playing && GameManager.gameState != GameState.StartingSave)
                    Debug.LogError("Creating entity in wrong gamestate");
                
                bool allowed = true;
                int tileWidth = entityPrefab.properties.Get<int>("tileWidth");
                int tileHeight = entityPrefab.properties.Get<int>("tileHeight");
                
                if(entityPrefab.properties.Get<string>("movementMode") == EntityMovementMode.Static)
                {
                    for (int sX = 0; sX < tileWidth; sX++)
			        {
			            for (int sY = 0; sY < tileHeight; sY++)
			            {
			                if(m_entityPlacementMap.GetDataAt(((int)x) + sX, ((int)y) + sY))
                            {
                                allowed = false;
                                break;
                            }
			            }
			        }
                }
                if (!allowed)
                {
                    CMD.Warning("Cannot place prop, " + x + ", " + y + "already filled");
                    return null;
                }

                for (int sX = 0; sX < tileWidth; sX++)
                {
                    for (int sY = 0; sY < tileHeight; sY++)
                    {
                        m_entityPlacementMap.SetDataAt(((int)x) + sX, ((int)y) + sY, true);
                    }
                }
                m_entityPlacementMap.ApplyAllOverlays();

                //Create the new entity
                T newEntity = Activator.CreateInstance(typeof(T)) as T;
                newEntity.properties.SetAll(entityPrefab.properties);
                newEntity.SetGraphics(entityPrefab.GetGraphics());

                GameObject parent = GameObject.Find("Entities");
                if (parent == null)
                {
                    parent = new GameObject("Entities");
                    parent.transform.parent = GameObject.Find("World").transform;
                }

                //Setup the graphics & GameObject for the new entity
                GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
                gameObject.name = newEntity.properties.Get<string>("displayName") + " [" + x + ", " + y + "]";
                gameObject.transform.parent = parent.transform;
                gameObject.layer = LayerForEntityType(newEntity.properties.Get<string>("type"));

                switch (entityPrefab.properties.Get<string>("type"))
                {
                    case "actor":
                        gameObject.transform.localScale = new Vector3(entityPrefab.properties.Get<int>("pixWidth") / TerrainTileGraphics.TILE_TEXTURE_RESOLUTION,
                            entityPrefab.properties.Get<int>("pixHeight") / TerrainTileGraphics.TILE_TEXTURE_RESOLUTION, 1);
                        break;
                    default:
                        gameObject.transform.localScale = new Vector3(entityPrefab.properties.Get<int>("tileWidth"), entityPrefab.properties.Get<int>("tileHeight"), 1);
                        break;
                }
                gameObject.transform.localPosition = new Vector3(0, 0, -0.05f);

                //Setup graphics
                newEntity.gameObject = gameObject;
                gameObject.GetComponent<Renderer>().material = Resources.Load("Materials/Entity") as Material;
                gameObject.GetComponent<Renderer>().material.SetTexture("_MainTex", newEntity.GetGraphics().texture(UnityEngine.Random.Range(0, newEntity.GetGraphics().variants)));
                gameObject.GetComponent<Renderer>().sortingOrder = 1;

                //Some final touches
                newEntity.GoToGamePos(x, y, newEntity.gameObject.transform.position.z, true);
                newEntity.GenerateCollision();
                //newEntity.positionStatus = EntityPositionStatus.OnGround;

                //Register the new entity
                m_entities.Add(newEntity);
                m_entityCount++;

                //Start
                newEntity.OnStart();

                //add behaviour scripts
                if (behaviourScripts != null)
                {
                    for (int i = 0; i < behaviourScripts.Length; i++)
                    {
                        behaviourScripts[i].AttachToEntity(newEntity);
                    }
                }
                return newEntity;
            }
            public Entity CreateEntity<T>(Properties properties, float x, float y, EntityBehaviourScript[] behaviourScripts = null) where T : Entity
            {
                if (GameManager.gameState != GameState.StartingNew && GameManager.gameState != GameState.Playing && GameManager.gameState != GameState.StartingSave)
                    Debug.LogError("Creating entity in wrong gamestate");

                bool allowed = true;
                int tileWidth = properties.Get<int>("tileWidth");
                int tileHeight = properties.Get<int>("tileHeight");

                if (properties.Get<string>("movementMode") == EntityMovementMode.Static)
                {
                    for (int sX = 0; sX < tileWidth; sX++)
                    {
                        for (int sY = 0; sY < tileHeight; sY++)
                        {
                            if (m_entityPlacementMap.GetDataAt(((int)x) + sX, ((int)y) + sY))
                            {
                                allowed = false;
                                break;
                            }
                        }
                    }
                }
                if (!allowed)
                {
                    CMD.Warning("Cannot place prop, " + x + ", " + y + "already filled");
                    return null;
                }

                for (int sX = 0; sX < tileWidth; sX++)
                {
                    for (int sY = 0; sY < tileHeight; sY++)
                    {
                        m_entityPlacementMap.SetDataAt(((int)x) + sX, ((int)y) + sY, true);
                    }
                }
                m_entityPlacementMap.ApplyAllOverlays();

                //Create the new entity
                T newEntity = Activator.CreateInstance(typeof(T)) as T;
                newEntity.properties.SetAll(properties);
                newEntity.SetGraphics(FindGraphics(properties.Get<string>("identity")));

                GameObject parent = GameObject.Find("Entities");
                if (parent == null)
                {
                    parent = new GameObject("Entities");
                    parent.transform.parent = GameObject.Find("World").transform;
                }

                //Setup the graphics & GameObject for the new entity
                GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
                gameObject.name = newEntity.properties.Get<string>("displayName") + " [" + x + ", " + y + "]";
                gameObject.transform.parent = parent.transform;
                gameObject.layer = LayerForEntityType(properties.Get<string>("type"));

                switch (properties.Get<string>("type"))
                {
                    case "actor":
                        gameObject.transform.localScale = new Vector3(properties.Get<int>("pixWidth") / TerrainTileGraphics.TILE_TEXTURE_RESOLUTION,
                            properties.Get<int>("pixHeight") / TerrainTileGraphics.TILE_TEXTURE_RESOLUTION, 1);
                        break;
                    default:
                        gameObject.transform.localScale = new Vector3(properties.Get<int>("tileWidth"), properties.Get<int>("tileHeight"), 1);
                        break;
                }
                gameObject.transform.localPosition = new Vector3(0, 0, -0.05f);

                //Setup graphics
                newEntity.gameObject = gameObject;
                gameObject.GetComponent<Renderer>().material = Resources.Load("Materials/Entity") as Material;
                gameObject.GetComponent<Renderer>().material.SetTexture("_MainTex", newEntity.GetGraphics().texture(UnityEngine.Random.Range(0, newEntity.GetGraphics().variants)));
                gameObject.GetComponent<Renderer>().sortingOrder = 1;

                //Some final touches
                newEntity.GoToGamePos(x, y, newEntity.gameObject.transform.position.z, true);
                newEntity.GenerateCollision();
                //newEntity.positionStatus = EntityPositionStatus.OnGround;

                //Register the new entity
                m_entities.Add(newEntity);
                m_entityCount++;

                //add our behaviour scripts
                if (behaviourScripts != null)
                {
                    for (int i = 0; i < behaviourScripts.Length; i++)
                    {
                        behaviourScripts[i].AttachToEntity(newEntity);
                    }
                }

                newEntity.OnStart();

                return newEntity;
            }

            public void DestroyEntity(Entity entity)
            {
                if (m_entities.Contains(entity))
                {
                    //Deselect entity (just in case)
                    entity.OnDeselect();

                    //Remove behaviour scripts
                    EntityBehaviourScript[] scripts = entity.BehaviourScripts;
                    for (int i = 0; i < scripts.Length; i++)
                    {
                        UnregisterEntityBehaviourScript(scripts[i]);
                    }

                    //Remove reference & destroy gameobject
                    m_entities.Remove(entity);
                    entity.gameObject.AddComponent<GameObjectDestroyer>().Destroy();
                }

            }

            public void UnregisterEntity(Entity entity)
            {
                m_entities.Remove(entity);
                m_entityCount--;
            }

            public void RegisterEntityBehaviourScript(EntityBehaviourScript script)
            {
                m_entityBehaviourScripts.Add(script);
            }

            public void UnregisterEntityBehaviourScript(EntityBehaviourScript script)
            {
                if (m_entityBehaviourScripts.Contains(script))
                {
                    Entity[] entities = script.AttachedTo;
                    for (int i = 0; i < entities.Length; i++)
                    {
                        script.RemoveFromEntity(entities[i]);
                    }
                    m_entityBehaviourScripts.Remove(script);
                }
                else
                    CMD.Error("You are trying to unregister a behaviour script that has not been registered yet.");
            }

            public void LoadEntities(string folder)
            {
                if(m_entityCache == null)
                    m_entityCache = new List<Entity>();

                string[] folders = Directory.GetDirectories(Properties.dataRootPath + "entities/" + folder);

                for (int i = 0; i < folders.Length; i++)
                {
                    string[] split = folders[i].Split('\\');
                    folders[i] = split[split.Length - 1];

                    string entityFolder = Properties.dataRootPath + "entities/" + folder + "/" + folders[i];
                    string[] propertyFiles = Directory.GetFiles(entityFolder, "*.xml");

                    Properties p = new Properties("entities/" + folder);

                    for (int file = 0; file < propertyFiles.Length; file++)
                    {
                        string[] splitFile = propertyFiles[file].Split('\\');
                        string propFile = splitFile[splitFile.Length - 1];

                        p.Load(folders[i] + "/" + propFile);
                    }
                    
                    Entity entity = null;

                    string type = p.Get<string>("type");
                    switch (type)
                    {
                        case "actor":
                        entity = new Actor();
                        entity.SetGraphics(entity.GenerateGraphics<Actor>());
                        break;
                        case "prop":
                        entity = new Prop();
                        entity.SetGraphics(entity.GenerateGraphics<Prop>());
                        break;
                        case "building":
                        entity = new Building();
                        entity.SetGraphics(entity.GenerateGraphics<Building>());
                        break;
                    }
                    GraphicsBase graphics = entity.GetGraphics();
                    graphics.variants = p.Get<int>("spriteVariants");

                    if(p.Has("tileHeight"))
                        graphics.tileHeight = p.Get<int>("tileHeight");
                    if (p.Has("tileWidth"))
                        graphics.tileWidth = p.Get<int>("tileWidth");

                    graphics.LoadFrom(folders[i] + "/" + folders[i], p);

                    if (entity != null)
                    {
                        entity.properties.SetAll(p);

                        m_entityCache.Add(entity);
                    }
                    else
                    {
                        Debug.LogError("This Entity's type is not set or is invalid | " + p.Get<string>("identity"));
                    }
                }
            }

            public Prop[] GetLoadedFlora(string planetType = "any")
            {
                List<Prop> result = new List<Prop>();
                for (int i = 0; i < m_entityCache.Count; i++)
                {
                    if (m_entityCache[i].properties.Get<string>("type") != "prop")
                        continue;
                    else
                    if(planetType != "any")
                    {
                        if (planetType == m_entityCache[i].properties.Get<string>("planetType") || m_entityCache[i].properties.Get<string>("planetType") == "any")
                            if (m_entityCache[i].properties.Get<string>("propType") == "flora")
                                result.Add(m_entityCache[i] as Prop);
                    }
                    else
                    {
                        if (m_entityCache[i].properties.Get<string>("propType") == "flora")
                            result.Add(m_entityCache[i] as Prop);
                    }
                }
                return result.ToArray();
            }

            public Building[] GetLoadedBuildings()
            {
                List<Building> result = new List<Building>();
                for (int i = 0; i < m_entityCache.Count; i++)
                {
                    if (m_entityCache[i].properties.Get<string>("type") != "building")
                        continue;
                    else
                        result.Add(m_entityCache[i] as Building);
                }
                return result.ToArray();
            }

            public T FindFromCache<T>(string identity) where T : Entity
            {
                T result = default(T);
                
                for (int i = 0; i < m_entityCache.Count; i++)
                {
                    if (m_entityCache[i].GetType() == typeof(T))
                        if (m_entityCache[i].properties.Get<string>("identity") == identity)
                            return (T)m_entityCache[i];
                }
                return result;
            }
            public T[] FindAllFromCache<T>() where T : Entity
            {
                List<T> result = new List<T>();
                for (int i = 0; i < m_entityCache.Count; i++)
                {
                    if (m_entityCache[i].GetType() == typeof(T))
                        result.Add((T)m_entityCache[i]);
                }
                return result.ToArray();
            }
            public GraphicsBase FindGraphics(string identity)
            {
                for (int i = 0; i < m_entityCache.Count; i++)
                {
                    if (m_entityCache[i].Identity == identity)
                        return m_entityCache[i].GetGraphics();
                }
                return null;
            }

            public T[] FindAll<T>() where T : Entity
            {
                List<T> result = new List<T>();
                for (int i = 0; i < m_entities.Count; i++)
                {
                    if (m_entities[i].GetType() == typeof(T))
                        result.Add((T)m_entities[i]);
                }
                return result.ToArray();
            }

            public int activeEntityCount
            {
                get
                {
                    return m_entities.Count;
                }
            }
            public int cachedEntityCount
            {
                get
                {
                    return m_entityCache.Count;
                }
            }
            public List<Entity> entities
            {
                get
                {
                    return m_entities;
                }
            }
            public List<Properties> allEntityProperties
            {
                get
                {
                    List<Properties> properties = new List<Properties>();

                    for (int i = 0; i < m_entities.Count; i++)
                    {
                        properties.Add(m_entities[i].properties);
                    }

                    return properties;
                }
            }
            public bool isTileAvaiableAt(int x, int y)
            {
                if (m_entityPlacementMap.GetDataAt(x, y))
                    return false;
                return true;
            }

            public int LayerForEntityType(string type)
            {
                if (type == "actor")
                    return 11;
                if (type == "building")
                    return 13;
                if (type == "prop")
                    return 12;
                return 9;
            }

            //Console Commands
            [ConsoleCommand("List entity identity of all loaded tiles (NOTE: not instantiated entities)")]
            public static void CMDListEntityCache()
            {
                EntityManager EM = ManagerInstance.Get<EntityManager>();

                //Actors
                Actor[] actors = EM.FindAllFromCache<Actor>();
                for (int i = 0; i < actors.Length; i++)
                {
                    CMD.Log(actors.GetType() + " : " + actors[i].Identity);
                }
                //Props
                Prop[] props = EM.FindAllFromCache<Prop>();
                for (int i = 0; i < props.Length; i++)
                {
                    CMD.Log(props.GetType() + " : " + props[i].Identity);
                }
                //Buildings
                Building[] buildings = EM.FindAllFromCache<Building>();
                for (int i = 0; i < buildings.Length; i++)
                {
                    CMD.Log(buildings.GetType() + " : " + buildings[i].Identity);
                }
            }
            [ConsoleCommand("Spawns an actor at given coordinates")]
            public static Entity CMDCreateActor(string _identity, string _x, string _y)
            {
                float x = float.Parse(_x);
                float y = float.Parse(_y);
                Entity prefab = ManagerInstance.Get<EntityManager>().FindFromCache<Actor>(_identity);

                if (prefab == null)
                {
                    CMD.Error("Actor not found!: " + _identity);
                    return null;
                }

                return ManagerInstance.Get<EntityManager>().CreateEntity<Actor>(prefab, x, y);
            }
            [ConsoleCommand("Spawns a prop at given coordinates")]
            public static Entity CMDCreateProp(string _identity, string _x, string _y)
            {
                float x = float.Parse(_x);
                float y = float.Parse(_y);
                Entity prefab = ManagerInstance.Get<EntityManager>().FindFromCache<Prop>(_identity);

                if (prefab == null)
                {
                    CMD.Error("Prop not found!: " + _identity);
                    return null;
                }

                return ManagerInstance.Get<EntityManager>().CreateEntity<Prop>(prefab, x, y);
            }
            [ConsoleCommand("Builds a building at given coordinates")]
            public static Entity CMDCreateBuilding(string _identity, string _x, string _y)
            {
                int x = int.Parse(_x);
                int y = int.Parse(_y);
                Building prefab = ManagerInstance.Get<EntityManager>().FindFromCache<Building>(_identity);

                if (prefab == null)
                {
                    CMD.Error("Building not found!: " + _identity);
                    return null;
                }

                return ManagerInstance.Get<BuildManager>().BuildBuilding(x, y, prefab, true);
            }
        }
    }
}
