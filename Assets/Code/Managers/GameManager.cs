using System;
using System.Collections.Generic;
using System.IO;

using UnityEngine;

using EndlessExpedition.Serialization;
using EndlessExpedition.Entities;
using EndlessExpedition.Managers.Base;
using EndlessExpedition.Entities.BehvaiourScripts;

namespace EndlessExpedition
{
    namespace Managers
    {
        public class GameManager : ManagerBase
        {
            private static GameState m_gameState = GameState.Booting;

            public override void OnStart()
            {
                ManagerInstance.Get<InputManager>().AddEventListener(InputPressType.Up, KeyCode.F8, Save);
                ManagerInstance.Get<InputManager>().AddEventListener(InputPressType.Up, KeyCode.F4, MakeTransportBot);
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

            }
            
            //save current game
            public void Save()
            {
                if (saveDataContainer.saveGame != null)
                    saveDataContainer.saveGame.Save(Main.instance.saveGameName);
                else
                {
                    Savegame save = new Savegame();
                    save.Save();
                }
            }

            public void Load()
            {

            }

            private void MakeTransportBot()
            {
                ManagerInstance.Get<EntityManager>().CreateEntity<Actor>(ManagerInstance.Get<EntityManager>().Find<Actor>("skybotTransportSmall"), 56, 56, new EntityBehaviourScript[] {new SkybotHover(), new SkybotTrailParticles()});
            }

            public static GameState gameState
            {
                get
                {
                    return m_gameState;
                }
                set
                {
                    Debug.LogWarning("Gamestate change, from: " + m_gameState + " to " + value);
                    m_gameState = value;
                }
            }
            public static SaveDataContainer saveDataContainer
            {
                get
                {
                    if (GameObject.Find("SaveData") == null)
                        new GameObject("SaveData").AddComponent<SaveDataContainer>();

                    return GameObject.Find("SaveData").GetComponent<SaveDataContainer>();
                }
            }
        }
    }
}
