using System;
using System.Collections.Generic;
using UnityEngine;

using FinalFrontier.Serialization;
using FinalFrontier.Managers;
using FinalFrontier.UI;

namespace FinalFrontier
{
    namespace Terrain
    {
        public class TerrainTile : IEngineEvents
        {
            //References
            private TerrainChunk m_chunk;

            //Properties
            public int x, y;
            private Properties m_properties;

            //Demographic
            private TerrainTileNeighborInfo m_neighborInfo;
            
            public TerrainTile(int x, int y, TerrainChunk chunk)
            {
                //Properties
                m_properties = new Properties("terrainTiles");
                
                m_properties.Secure("type", "terrainTile");
                m_properties.Secure("spriteVariants", 1);

                //Constructor values
                m_chunk = chunk;
                this.x = x; this.y = y;

                //Misc
                m_neighborInfo = new TerrainTileNeighborInfo();
                m_neighborInfo.identity = identity;
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

            //MouseEvnets
            public void OnMouseEnter()
            {
                //Draw the tile's properties to the inspector
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
                    return new Vector2(x + m_chunk.x * TerrainChunk.SIZE, y + m_chunk.y * TerrainChunk.SIZE);
                }
            }

            //Setters
            public void SetTo(TerrainTileCache tile)
            {
                m_properties.SetAll(tile.properties.GetAll());
                m_chunk.AddTileToDrawQueue(this);
                m_neighborInfo.identity = identity;
            }
        }
    }
}
