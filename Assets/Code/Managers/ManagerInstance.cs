using UnityEngine;
using System.Collections.Generic;

namespace FinalFrontier
{
    namespace Managers
    {
        public static class ManagerInstance
        {
            private static List<ManagerBase> _managers = new List<ManagerBase>();

            //Start
            public static void OnStart()
            {
                for (int i = 0; i < _managers.Count; i++)
                {
                    _managers[i].OnStart();
                }
            }

            private static void AddManagers()
            {
                Add(new GameManager());
                Add(new UIManager());
                Add(new PropertiesManager());
                Add(new TerrainManager());
                Add(new EntityManager());
            }

            //Methods
            public static MT Get<MT>()
            {
                ManagerBase m = default(ManagerBase);
                for (int i = 0; i < _managers.Count; i++)
                {
                    if (_managers[i].GetType() == typeof(MT))
                    {
                        m = _managers[i];
                        break;
                    }
                }
                return (MT)System.Convert.ChangeType(m, typeof(MT));
            }

            public static ManagerBase Add(ManagerBase manager)
            {
                _managers.Add(manager);
                return manager;
            }

            //Updates
            public static void OnTick()
            {
                for (int i = 0; i < _managers.Count; i++)
                {
                    _managers[i].OnTick();
                }
            }

            public static void OnUpdate()
            {
                for (int i = 0; i < _managers.Count; i++)
                {
                    _managers[i].OnUpdate();
                }
            }

            public static void OnSave()
            {
                for (int i = 0; i < _managers.Count; i++)
                {
                    _managers[i].OnSave();
                }
            }

            public static void OnLoad()
            {
                AddManagers();
                for (int i = 0; i < _managers.Count; i++)
                {
                    _managers[i].OnLoad();
                }
            }
        }
    }
}
