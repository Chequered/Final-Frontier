using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

using UnityEngine;

using EndlessExpedition.Entities;
using EndlessExpedition.Terrain;
using EndlessExpedition.Terrain.Generation;
using EndlessExpedition.Serialization;
using EndlessExpedition.Graphics;
using EndlessExpedition.Managers.Base;

namespace EndlessExpedition
{
    namespace Managers
    {
        public class TerrainManager : ManagerBase
        {
            //constants
            public const int WORLD_WIDTH = 8;

            //static vars
            private TerrainOperationHandler m_operations;

            //Graphics & cache
            private List<TerrainTileCache> m_tileCache;
            private List<TerrainTileGraphicsCached> m_tileGraphicsCache;
            private List<TerrainChunk> m_terrainChunkDrawQueue;

            //Datamaps
            private TerrainChunk[,] m_terrainChunks;
            private TerrainTile[,] m_terrainTiles;

            //Info
            private TerrainGenerationWorldInfo m_worldInfo;
            private TerrainGenerationTerrainInfo m_terrainInfo;

            public override void OnStart()
            {
                BuildTerrain();

                if (GameManager.gameState == GameState.StartingNew)
                    GenerateTerrain();
                else
                    LoadTerrain();

            }

            private void BuildTerrain()
            {
                //Create a terrainBuilder and build the terrain
                GameObject world = GameObject.Find("World");
                if (world == null)
                    world = new GameObject("World");

                m_operations = world.AddComponent<TerrainOperationHandler>();

                GameObject terrain = new GameObject("Terrain");//Parent gameobject of all terrain objects
                terrain.transform.parent = world.transform;

                for (int cX = 0; cX < TerrainManager.WORLD_WIDTH; cX++)
                {
                    for (int cY = 0; cY < TerrainManager.WORLD_WIDTH; cY++)
                    {
                        //Gameobject for the chunk
                        GameObject terrainChunk = GameObject.Instantiate(Resources.Load("Terrain/TerrainChunk") as GameObject,
                            new Vector2(cX * TerrainChunk.SIZE, cY * TerrainChunk.SIZE),
                            Quaternion.identity) as GameObject;

                        //Set some transform info
                        terrainChunk.transform.parent = terrain.transform;
                        terrainChunk.transform.name += " [" + cX + ", " + cY + "]";

                        //Set the chunks data and start it
                        m_terrainChunks[cX, cY] = new TerrainChunk(cX, cY);
                        m_terrainChunks[cX, cY].gameObject = terrainChunk;
                        m_terrainChunks[cX, cY].OnStart();

                        //Get the void terrainTile
                        TerrainTileCache voidTile = ManagerInstance.Get<TerrainManager>().FindTerrainTileCache("void");

                        for (int tX = 0; tX < TerrainChunk.SIZE; tX++)
                        {
                            for (int tY = 0; tY < TerrainChunk.SIZE; tY++)
                            {
                                //Set the Tile info and start it
                                TerrainTile tile = m_terrainTiles[cX * TerrainChunk.SIZE + tX, cY * TerrainChunk.SIZE + tY] = new TerrainTile(tX, tY,
                                                                                                                                              cX * TerrainChunk.SIZE + tX, 
                                                                                                                                              cY * TerrainChunk.SIZE + tY, 
                                                                                                                                              m_terrainChunks[cX, cY]);
                                tile.properties.SetAll(voidTile.properties);
                                tile.OnStart();

                                m_terrainChunks[cX, cY].SetTerrainTile(tile, tX, tY);
                            }
                        }

                        m_terrainChunks[cX, cY].SetupGraphics();
                        m_terrainChunks[cX, cY].InitialDraw();
                    }
                }
            }

            private void GenerateTerrain()
            {
                m_worldInfo = new TerrainGenerationWorldInfo();
                m_worldInfo.GenerateTerrainProperties(GameManager.saveDataContainer.newGameSeed);

                m_terrainInfo = new TerrainGenerationTerrainInfo(m_worldInfo);
                m_terrainInfo.GenerateTerrain(m_terrainTiles);
            }

            private void LoadTerrain()
            {
                m_worldInfo = new TerrainGenerationWorldInfo();
                m_worldInfo.properties = GameManager.saveDataContainer.saveGame.worldProperties;

                m_terrainInfo = new TerrainGenerationTerrainInfo(m_worldInfo);
                m_terrainInfo.LoadTerrain(m_terrainTiles);
            }

            public void SetWorldProperties(Properties properties)
            {
                if (m_worldInfo != null)
                    m_worldInfo.properties.SetAll(properties);
            }

            //Logic
            public override void OnTick()
            {
                CycleDrawQueue();
            }

            public override void OnUpdate()
            {

            }

            public override void OnLoad()
            {
                LoadTiles();
                m_terrainChunkDrawQueue = new List<TerrainChunk>();
                m_tileGraphicsCache = new List<TerrainTileGraphicsCached>();

                //Create our datamaps
                m_terrainChunks = new TerrainChunk[WORLD_WIDTH, WORLD_WIDTH];
                m_terrainTiles = new TerrainTile[WORLD_WIDTH * TerrainChunk.SIZE, WORLD_WIDTH * TerrainChunk.SIZE];
            }

            public override void OnExit()
            {

            }

            //Data
            private void LoadTiles()
            {
                m_tileCache = new List<TerrainTileCache>();

                //Find al terrainTile Property files
                string[] folders = Directory.GetDirectories(Properties.dataRootPath + "tiles");

                for (int i = 0; i < folders.Length; i++)
                {
                    //Remove everything but the filename
                    string[] split = folders[i].Split('\\');
                    folders[i] = split[split.Length - 1];

                    //Load the properties
                    Properties p = new Properties("tiles");
                    p.Load(folders[i] + "/" + folders[i] + ".xml");

                    //Load the graphics
                    TerrainTileGraphics graphics = new TerrainTileGraphics();
                    graphics.variants = p.Get<int>("spriteVariants");
                    graphics.LoadFrom(folders[i] + "/" + folders[i], p);
                    graphics.GeneratePrimaryColor();

                    //Create the cache and assign its data
                    TerrainTileCache tile = new TerrainTileCache();
                    tile.loadID = i;
                    tile.properties.SetAll(p);
                    tile.graphics = graphics;

                    //Keep track of it
                    if (tile != null)
                        m_tileCache.Add(tile);
                }
            }

            public TerrainTileCache[] AvaivableTilesForPlanetType(string planetType, string tileType)
            {
                List<TerrainTileCache> result = new List<TerrainTileCache>();

                for (int i = 0; i < m_tileCache.Count; i++)
                {
                    if (m_tileCache[i].properties.Get<string>("tileType") != tileType)
                        continue;
                    string tilePlanetType = m_tileCache[i].properties.Get<string>("planetType");
                    if (tilePlanetType == planetType || tilePlanetType == "any")
                        result.Add(m_tileCache[i]);
                }

                return result.ToArray();
            }

            public void InspectWorldProperties()
            {
                ManagerInstance.Get<UIManager>().propertyInspector.InspectProperties(m_worldInfo.properties);
            }

            //Graphics
            public void CycleDrawQueue()
            {
                //Go through the drawQueue
                for (int i = 0; i < m_terrainChunkDrawQueue.Count; i++)
                {
                    m_operations.Draw(m_terrainChunkDrawQueue[i]);   
                }
                //Clear the drawQueue
                m_terrainChunkDrawQueue.Clear();
            }

            public Color[] GetCachedTexture(TerrainTile tile)
            {
                //check for cached version
                for (int i = 0; i < m_tileGraphicsCache.Count; i++)
                {
                    if (m_tileGraphicsCache[i].IsAs(tile.neighborInfo))
                    {
                        return m_tileGraphicsCache[i].textureData;
                    }
                }
                return null;
            }

            public void AddChunkToDrawQueue(TerrainChunk chunk)
            {
                //Add the chunk to the drawQueue if it's not present yet.
                if (!m_terrainChunkDrawQueue.Contains(chunk))
                    m_terrainChunkDrawQueue.Add(chunk);
            }

            public void CacheTexture(TerrainTileGraphicsCached graphicsCache)
            {
                m_tileGraphicsCache.Add(graphicsCache);
            }

            //Shapes
            public void DrawCircle(int centerX, int centerY, int radius, TerrainTileCache tile)
            {
                //Setup vars
                int r = radius;
                int ox = centerX, oy = centerY;
                int size = WORLD_WIDTH * TerrainChunk.SIZE;

                for (int x = -r; x < r; x++)
                {
                    int height = (int)Mathf.Sqrt(r * r - x * x);

                    for (int y = -height; y < height; y++)
                    {
                        if (x + ox >= 0 && x + ox < size && y + oy >= 0 && y + oy < size)
                            m_terrainTiles[x + ox, y + oy].SetTo(tile);
                    }
                }
            }

            public bool[,] DataCircle(int centerX, int centerY, int radius)
            {
                bool[,] result = new bool[radius * 2, radius * 2];
                
                int r = radius;
                int ox = centerX, oy = centerY;
                int size = WORLD_WIDTH * TerrainChunk.SIZE;

                for (int x = -r; x < r; x++)
                {
                    int height = (int)Mathf.Sqrt(r * r - x * x);

                    for (int y = -height; y < height; y++)
                    {
                        if (x + ox >= 0 && x + ox < size && y + oy >= 0 && y + oy < size)
                            result[x + ox, y + oy] = true;
                    }
                }
                return result;
            }

            //Getters
            public static int worldSize
            {
                get
                {
                    return WORLD_WIDTH * TerrainChunk.SIZE;
                }
            }

            public TerrainTileCache FindTerrainTileCache(string identity)
            {
                for (int i = 0; i < m_tileCache.Count; i++)
                {
                    if (m_tileCache[i].properties.Get<string>("identity") == identity)
                        return m_tileCache[i];
                }
                return null;
            }

            public TerrainTile[,] tiles
            {
                get
                {
                    return m_terrainTiles;
                }
            }

            public Properties worldProperties
            {
                get
                {
                    return m_worldInfo.properties;
                }
            }

            public Properties[] terrainTilesProperties
            {
                get
                {
                    Properties[] properties = new Properties[(WORLD_WIDTH * TerrainChunk.SIZE) * (WORLD_WIDTH * TerrainChunk.SIZE)];

                    for (int x = 0; x < WORLD_WIDTH * TerrainChunk.SIZE; x++)
                    {
                        for (int y = 0; y < WORLD_WIDTH * TerrainChunk.SIZE; y++)
                        {
                            properties[y * (WORLD_WIDTH * TerrainChunk.SIZE) + x] = tiles[x, y].properties;
                        }
                    }

                    return properties;
                }
            }

            public bool isInPlanetsShape(int x, int y)
            {
                return m_terrainInfo.IsInPlanetsShape(x, y);
            }

            public static bool isLocationValid(int x, int y)
            {
                if(x >= 0 && x < worldSize)
                {
                    if(y >= 0 && y < worldSize)
                        return true;
                    return false;
                }
                return false;
            }

            [ConsoleCommand("Print all the properties used for the terrain generation")]
            public static void CMDWorldInfo()
            {
                Properties p = ManagerInstance.Get<TerrainManager>().worldProperties;
                for (int i = 0; i < p.GetAll().Count; i++)
                {
                    CMD.Log(p.GetAll()[i].Key + " = " + p.GetAll()[i].Value);
                }
            }

            [ConsoleCommand("List tile identity of all loaded tiles (NOTE: not instantiated tiles)")]
            public static void CMDListTileCache()
            {
                TerrainTileCache[] tiles = ManagerInstance.Get<TerrainManager>().m_tileCache.ToArray();
                for (int i = 0; i < tiles.Length; i++)
                {
                    CMD.Log(tiles[i].properties.Get<string>("identity"));
                }
            }

            [ConsoleCommand("Log all properties of given tile")]
            public static void CMDInspectCachedTile(string identity)
            {
                TerrainTileCache tile = ManagerInstance.Get<TerrainManager>().FindTerrainTileCache(identity.Trim());
                if(tile != null)
                {
                    tile.properties.LogAll();
                }
                else
                {
                    CMD.Warning("Tile not found!: " + identity);
                }
            }
        }
    }
}
