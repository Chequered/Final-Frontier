using System;
using System.Collections.Generic;
using UnityEngine;

using EndlessExpedition.Serialization;
using EndlessExpedition.Managers;
using EndlessExpedition.UI;

namespace EndlessExpedition
{
    namespace Terrain
    {
        public class TerrainTile : IEngineEvents
        {
            //References
            private TerrainChunk m_chunk;

            //Properties
            private int m_x, m_y;
            private int m_LX, m_LY;
            private Properties m_properties;

            //Demographic
            private TerrainTileNeighborInfo m_neighborInfo;
            
            public TerrainTile(int localX, int localY, int x, int y, TerrainChunk chunk)
            {
                //Properties
                m_properties = new Properties("terrainTiles");
                
                m_properties.Secure("type", "terrainTile");
                m_properties.Secure("spriteVariants", 1);

                //Constructor values
                m_chunk = chunk;
                m_x = x; m_y = y;
                m_LX = localX;
                m_LY = localY;

                m_neighborInfo = new TerrainTileNeighborInfo();
                m_neighborInfo.tile = this;
            }

            public void OnStart()
            {
                //default properties
                m_properties.Secure("identity", "null");
            }

            public void OnTick()
            {

            }

            public void OnUpdate()
            {

            }

            //MouseEvents
            public void OnMouseEnter()
            {

            }

            public void OnMouseOver()
            {

            }

            public void OnMouseExit()
            {

            }

            //Getters
            public Properties properties
            {
                get
                {
                    return m_properties;
                }
            }

            public string identity
            {
                get
                {
                    return m_properties.Get<string>("identity");
                }
            }

            public TerrainTileNeighborInfo neighborInfo
            {
                get
                {
                    return m_neighborInfo;
                }
            }

            public Vector2 gamePosition
            {
                get
                {
                    return new Vector2((m_LX + m_chunk.x * TerrainChunk.SIZE) + 0.5f - TerrainChunk.SIZE / 2,
                                       (m_LY + m_chunk.y * TerrainChunk.SIZE) + 0.5f - TerrainChunk.SIZE / 2);
                }
            }

            public int localX
            {
                get
                {
                    return m_LX;
                }
            }

            public int localY
            {
                get
                {
                    return m_LY;
                }
            }

            public int x
            {
                get
                {
                    return m_x;
                }
            }

            public int y
            {
                get
                {
                    return m_y;
                }
            }

            //Setters
            public void SetTo(TerrainTileCache tile)
            {
                m_properties.SetAll(tile.properties.GetAll());
                m_chunk.AddTileToDrawQueue(this);
            }
        }
    }
}
