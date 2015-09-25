using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FinalFrontier.Managers;

namespace FinalFrontier
{
    namespace Terrain
    {
        public class TerrainTileNeighborInfo
        {
            public string identity;
            public int x, y;


            public void Copy(TerrainTileNeighborInfo other)
            {
                //Adding to empty string so it becomes a new string instead of a reference to the string
                identity = "" + other.identity;
            }

            public string left
            {
                get
                {
                    if (x <= 0)
                        return "none";
                    return ManagerInstance.Get<TerrainManager>().tiles[x - 1, y].identity;
                }
            }

            public string top
            {
                get
                {
                    if (y >= TerrainManager.worldSize - 1)
                        return "none";
                    return ManagerInstance.Get<TerrainManager>().tiles[x, y + 1].identity;
                }
            }

            public string right
            {
                get
                {
                    if (x >= TerrainManager.worldSize - 1)
                        return "none";
                    return ManagerInstance.Get<TerrainManager>().tiles[x + 1, y].identity;
                }
            }

            public string bottom
            {
                get
                {
                    if (y <= 0)
                        return "none";
                    return ManagerInstance.Get<TerrainManager>().tiles[x, y - 1].identity;
                }
            }
        }
    }
}
