using System;
using System.Collections.Generic;

using UnityEngine;

using FinalFrontier.Serialization;

namespace FinalFrontier
{
    namespace Managers
    {
        public class PropertiesManager : ManagerBase
        {
            private List<Properties> _properties;

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
                //Serialize all properties
            }

            public override void OnLoad()
            {
                //Load all properties
            }

            public override void OnExit()
            {

            }

            public void RegisterProperties(Properties properties)
            {
                if (!_properties.Contains(properties))
                    _properties.Add(properties);
                else
                    Debug.LogError("Properties already registered");
            }
        }
    }
}
