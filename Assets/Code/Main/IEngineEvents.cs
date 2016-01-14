using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EndlessExpedition
{
    interface IEngineEvents
    {
        void OnStart();
        void OnTick();
        void OnUpdate();
    }
}
