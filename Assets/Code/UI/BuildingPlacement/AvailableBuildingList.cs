using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using EndlessExpedition.Entities;
using EndlessExpedition.Managers;

namespace EndlessExpedition
{
    namespace UI
    {
        //TODO: rename to something more clear
        public class AvailableBuildingList : MonoBehaviour
        {
            private List<GameObject> m_buttons;

            private RectTransform m_rectTransform;
            private CanvasGroup m_canvasGroup;

            private const int OUT_SIDE_PADDING = 5;
            private const int OUT_TOP_PADDING = 4;
            private const int OUT_BOTTOM_PADDING = 2;
            private const int BUTTON_SPACING = 5;
            private const int BUTTON_WIDTH = 64;
            private const int UI_SCALE = 3;

            private bool m_visible = false;

            public delegate void VisibalityToggleEventHandler(bool newState);
            public VisibalityToggleEventHandler OnVisiblityToggle;

            private void Start()
            {
                m_rectTransform = GetComponent<RectTransform>();
                m_canvasGroup = GetComponent<CanvasGroup>();

                m_buttons = new List<GameObject>();
                
                BuildButtons();
                Toggle(false);
            }

            public void BuildButtons()
            {
                DestroyButtons();

                Building[] buildings = ManagerInstance.Get<EntityManager>().GetLoadedBuildings();

                int b = buildings.Length;

                m_rectTransform.sizeDelta = new Vector2(b * BUTTON_WIDTH + b * BUTTON_SPACING + BUTTON_SPACING, BUTTON_WIDTH + 2 * BUTTON_SPACING);

                for (int i = 0; i < buildings.Length; i++)
                {
                    GameObject button = new GameObject("Building button: " + buildings[i].properties.Get<string>("displayName"));
                    button.AddComponent<CanvasRenderer>();
                    button.AddComponent<Image>();
                    button.AddComponent<Button>();
                    button.AddComponent<AvailableBuildingButton>().building = buildings[i];

                    button.transform.SetParent(transform, false);
                    button.GetComponent<Image>().sprite = buildings[i].GetGraphics().randomSprite;
                    button.GetComponent<RectTransform>().sizeDelta = new Vector2(64, 64);
                    button.GetComponent<RectTransform>().localPosition = new Vector2(BUTTON_SPACING + i * BUTTON_WIDTH + i * BUTTON_SPACING + BUTTON_WIDTH / 2, BUTTON_SPACING + BUTTON_WIDTH / 2);
                    button.GetComponent<Button>().onClick.AddListener(() => button.GetComponent<AvailableBuildingButton>().OnClick());

                    m_buttons.Add(button);
                }
            }

            private void DestroyButtons()
            {
                for (int i = 0; i < m_buttons.Count; i++)
                {
                    Destroy(m_buttons[i]);
                }
                m_buttons.Clear();
            }

            public void Toggle()
            {
                m_visible = !m_visible;
                m_canvasGroup.alpha = System.Convert.ToInt32(m_visible);
                m_canvasGroup.blocksRaycasts = m_visible;
                m_canvasGroup.interactable = m_visible;

                if(OnVisiblityToggle != null)
                    OnVisiblityToggle(m_visible);
            }

            public void Toggle(bool state)
            {
                m_visible = state;
                m_canvasGroup.alpha = System.Convert.ToInt32(state);
                m_canvasGroup.blocksRaycasts = state;
                m_canvasGroup.interactable = state;

                if (OnVisiblityToggle != null)
                    OnVisiblityToggle(state);
            }
        }
    }
}
