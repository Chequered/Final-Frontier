using UnityEngine;

using FinalFrontier.UI;
using FinalFrontier.Serialization;

namespace FinalFrontier
{
    namespace Managers
    {
        public class UIManager : ManagerBase
        {

            private PropertyInspector _propertyInspector;
            private bool mouseOnEntity = false;

            public override void OnStart()
            {
                _propertyInspector = GameObject.FindGameObjectWithTag("PropertyInspector").GetComponent<PropertyInspector>();
                _propertyInspector.OnStart();
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

            //Property Inspector
            public void InspectPropeties(Properties properties)
            {
                _propertyInspector.SetInspectingProperty(properties);                    
            }

            public void ClosePropertyInspector()
            {
                _propertyInspector.Close();
            }

            public bool isInspectorOpen
            {
                get
                {
                    return _propertyInspector.isOpen;
                }
            }

            public InspectingType inspectingType
            {
                get
                {
                    return _propertyInspector.inspectingType;
                }
            }
        }
    }
}
