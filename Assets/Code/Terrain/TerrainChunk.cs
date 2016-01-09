using System;
using System.Collections.Generic;
using UnityEngine;
using FinalFrontier.Graphics;
using FinalFrontier.Managers;

namespace FinalFrontier
{
    namespace Terrain
    {
        public class TerrainChunk : IEngineEvents
        {
            //Constants
            public const int SIZE = 16;

            //References
            public GameObject gameObject;
            private TerrainTile[,] m_tiles;

            //Graphics
            private List<TerrainTile> m_terrainTileDrawQueue;

            //active vars
            public int x, y;

            public TerrainChunk(int x, int y)
            {
                this.x = x;
                this.y = y;
            }

            public void OnStart()
            {
                m_tiles = new TerrainTile[SIZE, SIZE];

                //Link the collision
                gameObject.GetComponent<TerrainChunkCollision>().SetTerrainChunk(this);
            }

            public void OnTick()
            {
                for (int x = 0; x < SIZE; x++)
                {
                    for (int y = 0; y < SIZE; y++)
                    {
                        m_tiles[x, y].OnTick();
                    }
                }
            }

            public void OnUpdate()
            {
                for (int x = 0; x < SIZE; x++)
                {
                    for (int y = 0; y < SIZE; y++)
                    {
                        m_tiles[x, y].OnUpdate();
                    }
                }
            }

            //Data
            public void SetTerrainTile(TerrainTile tile, int tX, int tY)
            {
                for (int x = 0; x < SIZE; x++)
                {
                    for (int y = 0; y < SIZE; y++)
                    {
                        m_tiles[tX, tY] = tile;
                    }
                }
            }

            //Graphics
            public void SetupGraphics()
            {
                Texture2D tex = new Texture2D(SIZE * TerrainTileGraphics.TILE_TEXTURE_RESOLUTION, SIZE * TerrainTileGraphics.TILE_TEXTURE_RESOLUTION);
                tex.filterMode = FilterMode.Point;
                tex.wrapMode = TextureWrapMode.Clamp;

                gameObject.GetComponent<Renderer>().sharedMaterial = new Material(gameObject.GetComponent<Renderer>().material);
                gameObject.GetComponent<Renderer>().sharedMaterial.mainTexture = tex;

                m_terrainTileDrawQueue = new List<TerrainTile>();
            }

            public void SetTerrainTexture(Texture2D texture)
            {
                gameObject.GetComponent<Renderer>().material.mainTexture = texture;
            }

            public void AddTileToDrawQueue(TerrainTile tile)
            {
                if(!m_terrainTileDrawQueue.Contains(tile))
                {
                    m_terrainTileDrawQueue.Add(tile);
                }
                ManagerInstance.Get<TerrainManager>().AddChunkToDrawQueue(this);
            }

            public void InitialDraw()
            {
                m_terrainTileDrawQueue.Clear();
                for (int x = 0; x < SIZE; x++)
                {
                    for (int y = 0; y < SIZE; y++)
                    {
                        m_terrainTileDrawQueue.Add(m_tiles[x, y]);
                    }
                }
                ManagerInstance.Get<TerrainManager>().AddChunkToDrawQueue(this);
            }

            //Getters
            public Texture2D texture
            {
                get
                {
                    return gameObject.GetComponent<Renderer>().material.mainTexture as Texture2D;
                }
            }

            public List<TerrainTile> terrainTileDrawQueue
            {
                get
                {
                    return m_terrainTileDrawQueue;
                }
            }

            public TerrainTile[,] tiles
            {
                get
                {
                    return m_tiles;
                }
            }
        }
    }
}
