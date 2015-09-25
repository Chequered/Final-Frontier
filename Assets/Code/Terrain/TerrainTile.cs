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
            private TerrainChunk _chunk;

            //Properties
            public int x, y;
            private Properties _properties;

            //Demographic
            private TerrainTileNeighborInfo _neighborInfo;

            //UI
            private UIManager _uiManager;

            public TerrainTile(int x, int y, TerrainChunk chunk)
            {
                //Properties
                _properties = new Properties("terrainTiles");
                _properties.Secure("type", "terrainTile");
                _properties.Secure("spriteVariants", 1);

                //Constructor values
                _chunk = chunk;
                this.x = x; this.y = y;

                //Misc
                _neighborInfo = new TerrainTileNeighborInfo();
                _neighborInfo.identity = identity;

                _uiManager = ManagerInstance.Get<UIManager>();
            }

            public void OnStart()
            {
                //default properties
                _properties.Secure("identity", "null");
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
                if(Input.GetMouseButtonUp(0))
                {
                    //select??
                }
                //move to on click and add selection sprite on tile
                    _uiManager.InspectPropeties(_properties);
            }

            public void OnMouseExit()
            {
                //Close the inspector
                    _uiManager.ClosePropertyInspector();
            }

            //Getters
            public Properties properties
            {
                get
                {
                    return _properties;
                }
            }

            public string identity
            {
                get
                {
                    return _properties.Get<string>("identity");
                }
            }

            public TerrainTileNeighborInfo neighborInfo
            {
                get
                {
                    return _neighborInfo;
                }
            }

            //Setters
            public void SetTo(TerrainTileCache tile)
            {
                _properties.SetAll(tile.properties.GetAll());
                _chunk.AddTileToDrawQueue(this);
                _neighborInfo.identity = identity;
            }
        }
    }
}
