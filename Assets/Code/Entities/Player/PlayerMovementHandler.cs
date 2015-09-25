using System;
using System.Collections.Generic;

using UnityEngine;

using FinalFrontier.Managers;

namespace FinalFrontier
{
    namespace Entities.Player
    {
        public class PlayerMovementHandler : IEngineEvents
        {
            private Vector2 _moveVector;
            private Rigidbody2D _rigidbody;

            public PlayerMovementHandler(Rigidbody2D rigidbody)
            {
                _rigidbody = rigidbody;
            }

            public void OnStart()
            {
                _moveVector = new Vector2();
            }

            public void OnTick()
            {

            }

            public void OnUpdate()
            {
                float x = Input.GetAxis("Horizontal");
                float y = Input.GetAxis("Vertical");

                _moveVector.x = x;
                _moveVector.y = y;

                _rigidbody.MovePosition(_rigidbody.position + _moveVector);
            }
        }
    }
}
