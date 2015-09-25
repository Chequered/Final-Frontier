using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

using UnityEngine;

using FinalFrontier.Entities;
using FinalFrontier.Terrain;
using FinalFrontier.Terrain.Generation;
using FinalFrontier.Serialization;
using FinalFrontier.Graphics;

namespace FinalFrontier
{
    namespace Managers
    {
        public class TerrainManager : ManagerBase
        {
            //constants
            public const int WORLD_WIDTH = 8;

            //static vars
            private TerrainOperationHandler _operations;

            //Graphics & cache
            private List<TerrainTileCache> _tileCache;
            private List<TerrainTileGraphicsCached> _tileGraphicsCache;
            private List<TerrainChunk> _terrainChunkDrawQueue;

            //Datamaps
            private TerrainChunk[,] _terrainChunks;
            private TerrainTile[,] _terrainTiles;

            public override void OnStart()
            {
                //Create our datamaps
                _terrainChunks = new TerrainChunk[WORLD_WIDTH, WORLD_WIDTH];
                _terrainTiles = new TerrainTile[WORLD_WIDTH * TerrainChunk.SIZE, WORLD_WIDTH * TerrainChunk.SIZE];

                BuildTerrain();
                GenerateTerrain();
                GenerateNeighborInfo();
            }

            private void BuildTerrain()
            {
                //Create a terrainBuilder and build the terrain
                GameObject world = new GameObject("World");
                _operations = world.AddComponent<TerrainOperationHandler>();

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
                        _terrainChunks[cX, cY] = new TerrainChunk(cX, cY);
                        _terrainChunks[cX, cY].gameObject = terrainChunk;
                        _terrainChunks[cX, cY].OnStart();

                        //Get the void terrainTile
                        TerrainTileCache voidTile = ManagerInstance.Get<TerrainManager>().FindFromCache("void");

                        for (int tX = 0; tX < TerrainChunk.SIZE; tX++)
                        {
                            for (int tY = 0; tY < TerrainChunk.SIZE; tY++)
                            {

                                //Set the Tile info and start it
                                _terrainTiles[cX * TerrainChunk.SIZE + tX, cY * TerrainChunk.SIZE + tY] = new TerrainTile(tX, tY, _terrainChunks[cX, cY]);
                                _terrainTiles[cX * TerrainChunk.SIZE + tX, cY * TerrainChunk.SIZE + tY].properties.SetAll(voidTile.properties);
                                _terrainTiles[cX * TerrainChunk.SIZE + tX, cY * TerrainChunk.SIZE + tY].OnStart();

                                _terrainChunks[cX, cY].SetTerrainTile(_terrainTiles[cX * TerrainChunk.SIZE + tX, cY * TerrainChunk.SIZE + tY], tX, tY);
                            }
                        }

                        _terrainChunks[cX, cY].SetupGraphics();
                        _terrainChunks[cX, cY].InitialDraw();
                    }
                }
            }

            private TerrainGenerationWorldInfo _worldInfo;
            private TerrainGenerationTerrainInfo _terrainInfo;
            private void GenerateTerrain()
            {
                if(_worldInfo == null)
                {
                    _worldInfo = new TerrainGenerationWorldInfo();
                    _worldInfo.GenerateTerrainProperties(UnityEngine.Random.Range(TerrainGenerationWorldInfo.MIN_SEED_VALUE, TerrainGenerationWorldInfo.MAX_SEED_VALUE));
                }
                _terrainInfo = new TerrainGenerationTerrainInfo(_worldInfo);
                _terrainInfo.GenerateTerrain(_terrainTiles);
            }

            private void GenerateNeighborInfo()
            {
                //Now that all the tiles have been created, its tile to set its neighbor info
                for (int x = 0; x < TerrainManager.WORLD_WIDTH * TerrainChunk.SIZE; x++)
                {
                    for (int y = 0; y < TerrainManager.WORLD_WIDTH * TerrainChunk.SIZE; y++)
                    {
                        TerrainTile tile = _terrainTiles[x, y];
                        tile.neighborInfo.identity = tile.identity;
                        tile.neighborInfo.x = x;
                        tile.neighborInfo.y = y;
                    }
                }
            }

            public void SetWorldProperties(Properties properties)
            {
                if (_worldInfo != null)
                    _worldInfo.properties.SetAll(properties);
            }

            //Logic
            public override void OnTick()
            {
                CycleDrawQueue();
            }

            public override void OnUpdate()
            {
                if(Input.GetKeyUp(KeyCode.F2))
                {
                    GameObject g = GameObject.Find("Perlin");

                    if (g == null)
                    {
                        g = new GameObject("Perlin");
                        g.AddComponent<SpriteRenderer>().sprite = _terrainInfo.perlinTexture;
                        g.transform.position = new Vector3(-15, 2, -2);
                        g.transform.localScale = new Vector2(35, 35);
                    }
                    else
                    {
                        if (g.activeInHierarchy)
                            g.SetActive(false);
                        else
                            g.SetActive(true);
                    }
                }
            }

            public override void OnSave()
            {

            }

            public override void OnLoad()
            {
                LoadTiles();
                _terrainChunkDrawQueue = new List<TerrainChunk>();
                _tileGraphicsCache = new List<TerrainTileGraphicsCached>();
            }

            public override void OnExit()
            {

            }

            //Data
            private void LoadTiles()
            {
                _tileCache = new List<TerrainTileCache>();

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
                    graphics.LoadFrom(folders[i] + "/" + folders[i]);
                    graphics.GeneratePrimaryColor();

                    //Create the cache and assign its data
                    TerrainTileCache tile = new TerrainTileCache();
                    tile.properties.SetAll(p);
                    tile.graphics = graphics;

                    //Keep track of it
                    if (tile != null)
                        _tileCache.Add(tile);
                }
            }

            public TerrainTileCache FindFromCache(string identity)
            {
                TerrainTileCache result = null;
                for (int i = 0; i < _tileCache.Count; i++)
                {
                    if(_tileCache[i].properties.Get<string>("identity") == identity)
                    {
                        //Found our result
                        return _tileCache[i];
                    }
                }
                return result;
            }

            public TerrainTileCache[] AvaivableTilesForPlanetType(string planetType, string tileType)
            {
                List<TerrainTileCache> result = new List<TerrainTileCache>();

                for (int i = 0; i < _tileCache.Count; i++)
                {
                    if (_tileCache[i].properties.Get<string>("tileType") != tileType)
                        continue;
                    string tilePlanetType = _tileCache[i].properties.Get<string>("planetType");
                    if (tilePlanetType == planetType || tilePlanetType == "any")
                        result.Add(_tileCache[i]);
                }

                return result.ToArray();
            }

            public void InspectWorldProperties()
            {
                ManagerInstance.Get<UIManager>().InspectPropeties(_worldInfo.properties);
            }

            //Graphics
            public void CycleDrawQueue()
            {
                //Go through the drawQueue
                for (int i = 0; i < _terrainChunkDrawQueue.Count; i++)
                {
                    _operations.Draw(_terrainChunkDrawQueue[i]);   
                }
                //Clear the drawQueue
                _terrainChunkDrawQueue.Clear();
            }

            public Color[] GetCachedTexture(TerrainTile tile)
            {
                //check for cached version
                for (int i = 0; i < _tileGraphicsCache.Count; i++)
                {
                    if (_tileGraphicsCache[i].identity == tile.identity)
                    {
                        return _tileGraphicsCache[i].textureData;
                    }
                }
                return null;
            }

            public void AddChunkToDrawQueue(TerrainChunk chunk)
            {
                //Add the chunk to the drawQueue if it's not present yet.
                if (!_terrainChunkDrawQueue.Contains(chunk))
                    _terrainChunkDrawQueue.Add(chunk);
            }

            public void CacheTexture(TerrainTileGraphicsCached graphicsCache)
            {
                _tileGraphicsCache.Add(graphicsCache);
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
                            _terrainTiles[x + ox, y + oy].SetTo(tile);
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

            public TerrainTile[,] tiles
            {
                get
                {
                    return _terrainTiles;
                }
            }

            public Properties worldProperties
            {
                get
                {
                    return _worldInfo.properties;
                }
            }
        }
    }
}
