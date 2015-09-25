using System;
using System.Collections.Generic;
using UnityEngine;

namespace FinalFrontier
{
    namespace Entities.Player
    {
        public class Player : Actor
        {
            private PlayerMovementHandler _movement;
            private GameObject _gameObject;

            public override void OnStart()
            {
                base.OnStart();
                _properties.Secure("identity", "Playername");
                _properties.Secure("entityType", "player");
            }

            public override void OnTick()
            {
                base.OnTick();

                if (_movement != null)
                    _movement.OnTick();
            }

            public override void OnUpdate()
            {
                base.OnUpdate();

                if(_movement != null)
                    _movement.OnUpdate();
            }

            public GameObject gameObject
            {
                get
                {
                    return _gameObject;
                }
                set
                {
                    _gameObject = value;
                    _movement = new PlayerMovementHandler(_gameObject.GetComponent<Rigidbody2D>());
                    _movement.OnStart();
                }
            }
        }
    }
}