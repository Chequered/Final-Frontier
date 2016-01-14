using System.Collections.Generic;

using UnityEngine;

using EndlessExpedition.UI;
using EndlessExpedition.UI.Internal;
using EndlessExpedition.Serialization;
using EndlessExpedition.Terrain;
using EndlessExpedition.Entities;
using EndlessExpedition.Managers.Base;
using EndlessExpedition.Entities.BuildingModules;

namespace EndlessExpedition
{
    namespace Managers
    {
        public class UIManager : ManagerBase
        {
            private PropertyInspector m_propertyInspector;
            private Dictionary<string, TerrainOverlay> m_terrainOverlays;
            private List<IUI> m_UI;

            private BuildPlacementScreen m_buildingPlacementScreen;
            private AvailableBuildingList m_availableBuildingList;

            public override void OnStart()
            {
                m_propertyInspector.OnStart();
                m_buildingPlacementScreen.OnStart();

                foreach (KeyValuePair<string, TerrainOverlay>  overlay in m_terrainOverlays)
                {
                    overlay.Value.OnStart();
                }

                ActionMenuList list = new ActionMenuList();
                list.buttonSize = new Vector2(100, 30);
                list.AddButton(new ActionButton("Build List", ManagerInstance.Get<UIManager>().availableBuildingList.Toggle));
                list.AddButton(new ActionButton("Quick Save", ManagerInstance.Get<GameManager>().Save));
                list.AddButton(new ActionButton("Quick Load", ManagerInstance.Get<GameManager>().Load));
                list.AddButton(new ActionButton("Sim Tick", ManagerInstance.Get<SimulationManager>().SimulationTick));
                //list.AddButton(new ActionButton("Resume All Production", ResumeAllProduction));
                list.BuildUI();
                AddUI(list);

                EssentialsDisplayBar bar = new EssentialsDisplayBar();
                bar.BuildUI();
                bar.position = new Vector2(Screen.width / 2, 0);
                ManagerInstance.Get<SimulationManager>().OnSimulationEnd += bar.UpdateUI;
                AddUI(bar);
            }

            public override void OnTick()
            {

            }

            public override void OnUpdate()
            {
                if (m_buildingPlacementScreen.isToggledOn)
                    m_buildingPlacementScreen.OnUpdate();
            }

            public override void OnLoad()
            {
                m_terrainOverlays = new Dictionary<string, TerrainOverlay>();
                m_availableBuildingList = GameObject.Find("AvaibleBuildingList").GetComponent<AvailableBuildingList>();
                m_propertyInspector = GameObject.FindGameObjectWithTag("PropertyInspector").GetComponent<PropertyInspector>();
                m_buildingPlacementScreen = new BuildPlacementScreen();
                m_UI = new List<IUI>();
            }

            public override void OnExit()
            {

            }

            //UI Refs
            public PropertyInspector propertyInspector
            {
                get
                {
                    return m_propertyInspector;
                }
            }

            public TerrainOverlay FindOverlay(string overlayName)
            {
                foreach (KeyValuePair<string, TerrainOverlay> overlay in m_terrainOverlays)
                {
                    if (overlay.Key == overlayName)
                        return overlay.Value;
                }
                return null;
            }

            public BuildPlacementScreen buildingPlacementScreen
            {
                get
                {
                    return m_buildingPlacementScreen;
                }
            }

            public AvailableBuildingList availableBuildingList
            {
                get
                {
                    return m_availableBuildingList;
                }
            }

            public void AddUI(IUI UIElement)
            {
                m_UI.Add(UIElement);
            }

            //Overlays
            public void AddTerrainOverlay(TerrainOverlay terrainOverlay)
            {
                if (!m_terrainOverlays.ContainsKey(terrainOverlay.overlayName))
                {
                    m_terrainOverlays.Add(terrainOverlay.overlayName, terrainOverlay);
                    terrainOverlay.OnStart();
                }
            }

            public void SelectBuildingPlacementBuilding(Building newBuilding)
            {
                m_buildingPlacementScreen.OnBuildingSwitch(newBuilding);
            }

            public static bool hasMouseOver
            {
                get
                {
                    return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
                }
            }

            public Vector2 windowSize
            {
                get
                {
                    return GameObject.FindGameObjectWithTag("UI").GetComponent<RectTransform>().sizeDelta;
                }
            }

            //Private Actionmenu methods
            private void ResumeAllProduction()
            {
                Building[] buildings = ManagerInstance.Get<EntityManager>().FindAll<Building>();
                for (int i = 0; i < buildings.Length; i++)
                {
                    if (buildings[i].GetModule<ProductionModule>() != null)
                        buildings[i].GetModule<ProductionModule>().TogglePause(false);
                }
            }
        }
    }
}
