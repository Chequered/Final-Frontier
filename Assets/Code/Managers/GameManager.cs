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

            public static GameState gameState
            {
                get
                {
                    return m_gameState;
                }
                set
                {
                    CMD.Warning("Gamestate change, from: " + m_gameState + " to " + value);
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

            [ConsoleCommand("Saves the game using currnt timestamp as filename")]
            public static void CMDSave()
            {
                ManagerInstance.Get<GameManager>().Save();
            }
        }
    }
}
