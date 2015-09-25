using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Reflection;

using UnityEngine;

using FinalFrontier.Entities;
using FinalFrontier.Entities.Player;
using FinalFrontier.Graphics;
using FinalFrontier.Serialization;

namespace FinalFrontier
{
    namespace Managers
    {
        public class EntityManager : ManagerBase
        {
            private List<Entity> _entityCache;
            private bool[,] _entityPlacementMap;

            private List<Entity> _entities;
            private int _entityCount;


            public override void OnStart()
            {

            }

            public override void OnTick()
            {
                for (int i = 0; i < _entityCount; i++)
                {
                    _entities[i].OnTick();
                }
            }

            public override void OnUpdate()
            {
                for (int i = 0; i < _entityCount; i++)
                {
                    _entities[i].OnUpdate();
                }
            }

            public override void OnSave()
            {
                //
            }

            public override void OnLoad()
            {
                LoadEntities("actors");
                LoadEntities("props");

                _entities = new List<Entity>();
                _entityPlacementMap = new bool[TerrainManager.worldSize, TerrainManager.worldSize];
                for (int x = 0; x < TerrainManager.worldSize; x++)
                {
                    for (int y = 0; y < TerrainManager.worldSize; y++)
                    {
                        _entityPlacementMap[x, y] = false;
                    }
                }
                _entityCount = 0;

                for (int i = 0; i < _entityCache.Count; i++)
                {
                    _entityCache[i].OnLoad();
                }
            }

            public override void OnExit()
            {
                throw new NotImplementedException();
            }

            public Entity CreateEntityFrom<T>(Entity entityPrefab, int x, int y) where T : Entity
            {
                bool allowed = true;
                
                if(entityPrefab.properties.Get<string>("movementMode") == EntityMovementMode.Static)
                {
                    if(_entityPlacementMap[x, y])
                        allowed = false;
                    else
                        _entityPlacementMap[x, y] = true;
                }
                if (!allowed)
                    return entityPrefab;

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
                GameObject gameObject = new GameObject(newEntity.properties.Get<string>("displayName") + " [" + x + ", " + y + "]");
                gameObject.transform.parent = parent.transform;
                gameObject.transform.localScale = new Vector2(3.2f, 3.2f);
                gameObject.transform.localPosition = new Vector3(0, 0, -2f);

                newEntity.gameObject = gameObject;
                gameObject.AddComponent<SpriteRenderer>().sprite = newEntity.GetGraphics().randomSprite;

                //Some final touches
                newEntity.GoToWorldPos(x, y);
                newEntity.SetupCollision();

                //Register the new entity
                _entities.Add(newEntity);
                _entityCount++;
                newEntity.OnStart();

                return newEntity;
            }

            public void UnRegisterEntity(Entity entity)
            {
                _entities.Remove(entity);
                _entityCount--;
            }

            public void LoadEntities(string folder)
            {
                if(_entityCache == null)
                    _entityCache = new List<Entity>();

                string[] folders = Directory.GetDirectories(Properties.dataRootPath + "entities/" + folder);

                for (int i = 0; i < folders.Length; i++)
                {
                    string[] split = folders[i].Split('\\');
                    folders[i] = split[split.Length - 1];

                    Properties p = new Properties("entities/" + folder);
                    p.Load(folders[i] + "/" + folders[i] + ".xml");

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
                    }
                    GraphicsBase graphics = entity.GetGraphics();
                    graphics.variants = p.Get<int>("spriteVariants");
                    graphics.LoadFrom(folders[i] + "/" + folders[i]);

                    if (entity != null)
                    {
                        entity.properties.SetAll(p);
                        //entity

                        _entityCache.Add(entity);
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
                for (int i = 0; i < _entityCache.Count; i++)
                {
                    if (_entityCache[i].properties.Get<string>("type") != "prop")
                        continue;
                    else
                    if(planetType != "any")
                    {
                        if (planetType == _entityCache[i].properties.Get<string>("planetType") || _entityCache[i].properties.Get<string>("planetType") == "any")
                            if (_entityCache[i].properties.Get<string>("propType") == "flora")
                                result.Add(_entityCache[i] as Prop);
                    }
                    else
                    {
                        if (_entityCache[i].properties.Get<string>("propType") == "flora")
                            result.Add(_entityCache[i] as Prop);
                    }
                }
                return result.ToArray();
            }

            public int entityCount
            {
                get
                {
                    return _entityCount;
                }
            }
        }
    }
}
