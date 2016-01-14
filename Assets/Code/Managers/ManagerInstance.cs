using UnityEngine;
using System.Collections.Generic;

using EndlessExpedition.Serialization;
using EndlessExpedition.Managers.Base;

namespace EndlessExpedition
{
    namespace Managers
    {
        public static class ManagerInstance
        {
            private static List<ManagerBase> m_managers = new List<ManagerBase>();

            //Start
            public static void OnStart()
            {
                if(GameManager.saveDataContainer.state == SaveDataState.New)
                    GameManager.gameState = GameState.StartingNew;
                else
                    GameManager.gameState = GameState.StartingSave;

                for (int i = 0; i < m_managers.Count; i++)
                {
                    m_managers[i].OnStart();
                }
                GameManager.gameState = GameState.Playing;

                //Savegame save = new Savegame();
                //save.Save();
            }

            private static void AddManagers()
            {
                Add(new InputManager());
                Add(new UIManager());
                Add(new TerrainManager());
                Add(new EntityManager());
                Add(new GameManager());
                Add(new BuildManager());
                Add(new SimulationManager());
                Add(new ItemManager());
            }

            //Methods
            public static MT Get<MT>()
            {
                ManagerBase m = default(ManagerBase);
                for (int i = 0; i < m_managers.Count; i++)
                {
                    if (m_managers[i].GetType() == typeof(MT))
                    {
                        m = m_managers[i];
                        break;
                    }
                }
                return (MT)System.Convert.ChangeType(m, typeof(MT));
            }

            public static ManagerBase Add(ManagerBase manager)
            {
                m_managers.Add(manager);
                return manager;
            }

            //Updates
            public static void OnTick()
            {
                for (int i = 0; i < m_managers.Count; i++)
                {
                    m_managers[i].OnTick();
                }
            }

            public static void OnUpdate()
            {
                for (int i = 0; i < m_managers.Count; i++)
                {
                    m_managers[i].OnUpdate();
                }
            }

            public static void OnSave()
            {

            }

            public static void OnLoad()
            {
                AddManagers();
                for (int i = 0; i < m_managers.Count; i++)
                {
                    m_managers[i].OnLoad();
                }
            }
        }
    }
}
