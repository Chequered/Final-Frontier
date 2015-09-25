using UnityEngine;
using System.Collections.Generic;

namespace FinalFrontier
{
    namespace Managers
    {
        public class GameManager : ManagerBase
        {
            private GameState _gameState;
            
            public override void OnStart()
            {
                
            }

            public override void OnTick()
            {
                
            }

            public override void OnUpdate()
            {

            }

            public override void OnSave()
            {

            }

            public override void OnLoad()
            {

            }

            public override void OnExit()
            {

            }

            //---------- Getters / Setters ----------\\

            public GameState gameState
            {
                get
                {
                    return _gameState;
                }
                set
                {
                    _gameState = value;
                }
            }

        }
    }
}
