using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EndlessExpedition.Managers;

namespace EndlessExpedition
{
    namespace Terrain
    {
        public class TerrainTileNeighborInfo
        {
            public TerrainTile tile;

            public string identity
            {
                get
                {
                    return tile.identity;
                }
            }

            public string left
            {
                get
                {
                    if (tile.x > 0)
                        return ManagerInstance.Get<TerrainManager>().tiles[tile.x - 1, tile.y].identity;
                    return "none";
                }
            }

            public string top
            {
                get
                {
                    if (tile.y < TerrainManager.worldSize - 1)
                        return ManagerInstance.Get<TerrainManager>().tiles[tile.x, tile.y + 1].identity;
                    return "none";
                }
            }

            public string right
            {
                get
                {
                    if (tile.x < TerrainManager.worldSize - 1)
                        return ManagerInstance.Get<TerrainManager>().tiles[tile.x + 1, tile.y].identity;
                    return "none";
                }
            }

            public string bottom
            {
                get
                {
                    if (tile.y > 0)
                        return ManagerInstance.Get<TerrainManager>().tiles[tile.x, tile.y - 1].identity;
                    return "none";
                }
            }
        }
    }
}
