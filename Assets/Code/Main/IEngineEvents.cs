using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FinalFrontier
{
    interface IEngineEvents
    {
        void OnStart();
        void OnTick();
        void OnUpdate();
    }
}
