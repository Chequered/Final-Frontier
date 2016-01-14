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
            public Building BuildBuildingAt(int x, int y, string buildingIdentity, bool instantBuild = false)
            {
                Building building = ManagerInstance.Get<EntityManager>().Find<Building>(buildingIdentity);
                //Debug.Log(building + " - " + buildingIdentity);
                int width = building.properties.Get<int>("tileWidth");
                int height = building.properties.Get<int>("tileHeight");
                Entity builtBuilding = null;

                if (CheckBuildLocation(x, y, width, height))
                {
                    builtBuilding = ManagerInstance.Get<EntityManager>().CreateEntity<Building>(building, x, y);
                    if (!instantBuild)
                        if (builtBuilding.properties.Get<string>("buildMode") == "dropDown")
                            new BuildingLanding().AttachToEntity(builtBuilding);
                }
                else
                {
                    Debug.LogError("Invalid Build Location! (" + x + ", " + y + ") " + width + " x " + height);
                }

                return builtBuilding as Building;
            }
            public Building BuildBuildingAt(int x, int y, Building buildingPrefab, bool instantBuild = false)
            {
                int width = buildingPrefab.properties.Get<int>("tileWidth");
                int height = buildingPrefab.properties.Get<int>("tileHeight");
                Entity builtBuilding = null;

                if (CheckBuildLocation(x, y, width, height))
                {
                    builtBuilding = ManagerInstance.Get<EntityManager>().CreateEntity<Building>(buildingPrefab, x, y);
                    if (!instantBuild)
                        if (builtBuilding.properties.Get<string>("buildMode") == "dropDown")
                            new BuildingLanding().AttachToEntity(builtBuilding);
                }
                else
                {
                    Debug.LogError("Invalid Build Location! (" + x + ", " + y + ") " + width + " x " + height);
                }

                return builtBuilding as Building;
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

            }

            public override void OnExit()
            {
                throw new System.NotImplementedException();
            }
        }
    }
}
