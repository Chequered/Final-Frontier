using UnityEngine;

using System.Collections.Generic;

using EndlessExpedition.UI;
using EndlessExpedition.Entities;
using EndlessExpedition.Entities.BehvaiourScripts;
using EndlessExpedition.Terrain;
using EndlessExpedition.Managers.Base;

namespace EndlessExpedition
{
    namespace Managers
    {
        public class BuildManager : ManagerBase
        {
            private TerrainDataMap<Building> m_buildings;

            public Building BuildBuilding(int x, int y, string buildingIdentity, bool instantBuild = false)
            {
                Building building = ManagerInstance.Get<EntityManager>().FindFromCache<Building>(buildingIdentity);

                return BuildBuilding(x, y, building, instantBuild);
            }
            public Building BuildBuilding(int x, int y, Building buildingPrefab, bool instantBuild = false)
            {
                int width = buildingPrefab.properties.Get<int>("tileWidth");
                int height = buildingPrefab.properties.Get<int>("tileHeight");
                Building builtBuilding = null;

                if (CheckBuildLocation(x, y, width, height))
                {
                    builtBuilding = ManagerInstance.Get<EntityManager>().CreateEntity<Building>(buildingPrefab, x, y) as Building;

                    if (instantBuild)
                        builtBuilding.OnBuild();
                    else
                    {
                        if (builtBuilding.properties.Get<string>("buildMode") == "dropDown")
                            new BuildingLanding().AttachToEntity(builtBuilding);
                        else if (builtBuilding.properties.Get<string>("buildMode") != "construction")
                            builtBuilding.OnBuild();
                    }

                    for (int _x = 0; _x < width; _x++)
                    {
                        for (int _y = 0; _y < height; _y++)
                        {
                            m_buildings.SetDataAt(x + _x, y + _y, builtBuilding, false);
                        }
                    }
                }
                else
                {
                    CMD.Warning("Invalid Build Location! (" + x + ", " + y + ") " + width + " x " + height);
                }

                return builtBuilding as Building;
            }
            public bool RegisterBuilding(int x, int y, Building building, bool instantBuild = false)
            {
                bool result = false;
                int width = building.properties.Get<int>("tileWidth");
                int height = building.properties.Get<int>("tileHeight");

                if (CheckBuildLocation(x, y, width, height))
                {
                    for (int _x = 0; _x < width; _x++)
                    {
                        for (int _y = 0; _y < height; _y++)
                        {
                            m_buildings.SetDataAt(x + _x, y + _y, building, false);
                        }
                    }
                }
                else
                {
                    CMD.Warning("Invalid Build Location! (" + x + ", " + y + ") " + width + " x " + height);
                }
                return result;
            }

            public bool CheckBuildLocation(int startX, int startY, int width, int height)
            {
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        if (!TerrainManager.isLocationValid(startX + x, startY + y))
                            return false;

                        if (!ManagerInstance.Get<EntityManager>().isTileAvaiableAt(startX + x, startY + y))
                            return false;

                        if (!ManagerInstance.Get<TerrainManager>().isInPlanetsShape(startX + x, startY + y))
                            return false;
                    }
                }
                return true;
            }
            
            public Building GetBuildingAt(int x, int y)
            {
                return m_buildings.GetDataAt(x, y);
            }

            public override void OnStart()
            {
            }

            public override void OnTick()
            {

            }

            public override void OnUpdate()
            {

            }
            
            public override void OnLoad()
            {
                m_buildings = new TerrainDataMap<Building>();
                m_buildings.SetAllData(null, false);
            }

            public override void OnExit()
            {
                throw new System.NotImplementedException();
            }
        }
    }
}
