using UnityEngine;

using FinalFrontier.Managers;
using FinalFrontier.Terrain;
using FinalFrontier.Entities;

namespace FinalFrontier
{
    namespace UI
    {
        public class BuildPlacementScreen : IEngineEvents
        {

            private Building m_selectedBuilding;
            private bool m_enabled = false;
            private GameObject m_exampleBuilding;


            public void OnStart()
            {
                ManagerInstance.Get<UIManager>().availableBuildingList.OnVisiblityToggle += Toggle;
                ManagerInstance.Get<InputManager>().AddEventListener(InputPressType.Up, 0, AttemptToBuild);
            }

            public void OnTick()
            {

            }

            public void OnUpdate()
            {
                if (m_selectedBuilding == null)
                    return;

                TerrainTile currentTile = ManagerInstance.Get<InputManager>().currentMouseOverTile;

                if (currentTile == null)
                    return;

                m_exampleBuilding.transform.position = Entity.ConvertGameToUnityPosition(currentTile.gamePosition,
                    m_selectedBuilding.properties.Get<int>("tileWidth"),
                    m_selectedBuilding.properties.Get<int>("tileHeight"));
            }

            private void AttemptToBuild()
            {
                TerrainTile currentTile = ManagerInstance.Get<InputManager>().currentMouseOverTile;

                if (currentTile == null || m_selectedBuilding == null || !m_enabled || UIManager.hasMouseOver)
                    return;

                if (ManagerInstance.Get<BuildManager>().CheckBuildLocation((int)currentTile.gamePosition.x, (int)currentTile.gamePosition.y,
                    m_selectedBuilding.properties.Get<int>("tileWidth"),
                    m_selectedBuilding.properties.Get<int>("tileHeight")))
                {
                    ManagerInstance.Get<BuildManager>().BuildBuildingAt((int)currentTile.gamePosition.x, (int)currentTile.gamePosition.y, m_selectedBuilding);
                }
            }

            public void Toggle(bool state)
            {
                m_enabled = state;
                if(m_enabled)
                {
                    //turning on screen
                    m_exampleBuilding = new GameObject("Building Example Sprite");
                    m_exampleBuilding.AddComponent<SpriteRenderer>();

                    if(m_selectedBuilding != null)
                    {
                        BuildSprite();
                    }
                }
                else
                {
                    //turing off screen
                    if (m_exampleBuilding != null)
                        m_exampleBuilding.AddComponent<GameObjectDestroyer>().Destroy();
                }
            }

            public void OnBuildingSwitch(Building newBuilding)
            {
                m_selectedBuilding = newBuilding;
                BuildSprite();
            }

            private void BuildSprite()
            {
                m_exampleBuilding.GetComponent<SpriteRenderer>().sprite = m_selectedBuilding.GetGraphics().randomSprite;
                m_exampleBuilding.transform.localScale = new Vector2(3.2f, 3.2f);
                m_exampleBuilding.layer = Entity.LAYER_ON_GROUND;
            }

            public bool isToggledOn
            {
                get
                {
                    return m_enabled;
                }
            }
        }
    }
}
