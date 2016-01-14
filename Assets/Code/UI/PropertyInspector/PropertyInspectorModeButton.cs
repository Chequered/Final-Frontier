using UnityEngine.UI;
using UnityEngine;

using EndlessExpedition.Managers;

namespace EndlessExpedition
{
    namespace UI
    {
        public class PropertyInspectorModeButton : MonoBehaviour
        {
            private PropertyInspector m_inspector;
            private Text m_buttonText;

            private void Start()
            {
                m_inspector = ManagerInstance.Get<UIManager>().propertyInspector;
                m_buttonText = transform.FindChild("Text").GetComponent<Text>();
                m_buttonText.text = "" + m_inspector.inspectingType;
            }

            public void OnButtonPressed()
            {
                switch (m_inspector.inspectingType)
                {
                    case InspectingType.None:
                        break;
                    case InspectingType.Tile:
                        m_inspector.inspectingType = InspectingType.Entity;
                        break;
                    case InspectingType.Entity:
                        m_inspector.inspectingType = InspectingType.Tile;
                        break;
                }
                m_buttonText.text = "" + m_inspector.inspectingType;
            }
        }
    }
}
