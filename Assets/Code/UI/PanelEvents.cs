using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

using FinalFrontier.Managers;
namespace FinalFrontier
{
    namespace UI
    {
        public class ActionButton
        {
            private string m_buttonText;
            private ActionButtonMenu.ButtonPressEventHandler m_eventHandler;

            public ActionButton(string text, ActionButtonMenu.ButtonPressEventHandler eventHandler)
            {
                m_buttonText = text;
                m_eventHandler = eventHandler;
            }

            public string text
            {
                get
                {
                    return m_buttonText;
                }
            }

            public void OnButtonPress()
            {
                m_eventHandler();
            }
        }

        public class ActionButtonMenu : MonoBehaviour
        {
            //TODO: create UI interface
            public delegate void ButtonPressEventHandler();

            private List<ActionButton> m_buttons;

            private RectTransform m_transform;

            private void Start()
            {
                m_transform = GetComponent<RectTransform>();
            }

            private void BuildUI()
            {

            }

            public void AddButton(ActionButton button)
            {
                m_buttons.Add(button);
            }
        }
    }
}
