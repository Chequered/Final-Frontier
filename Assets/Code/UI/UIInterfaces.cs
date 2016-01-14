using UnityEngine;
using UnityEngine.UI;

using System.Collections.Generic;

using EndlessExpedition.Managers;
using EndlessExpedition.Items;
using EndlessExpedition.UI.Internal;
using EndlessExpedition.Entities;

namespace EndlessExpedition
{
    namespace UI
    {
        namespace Internal
        {
            public interface IUI
            {
                void BuildUI();
                void Toggle();
                void Toggle(bool state);
                string menuName { get; set; }
                Vector2 position { get; set; }
                Vector2 windowSize { get; set; }
                RectTransform transform { get; }
            }

            public interface IInventoryDisplay : IUI
            {
                void UpdateUI();
            }
        }

        //interfaces
        public interface IActionMenu : IUI
        {
            void AddButton(ActionButton button);
            void RemoveButton(ActionButton button);
            ActionButton[] buttons { get; }
            Vector2 buttonSize { get; set; }
        }
        public interface IEssentialsDisplay : IInventoryDisplay
        {
            bool displayNames { get; set; }
            bool displayIcons { get; set; }
        }
        public interface ICurrencyDisplay : IInventoryDisplay
        {
            Item[] currencies { get; set; }
            bool displayNames { get; set; }
            bool displayIcons { get; set; }
        }

        public interface IItemContainerDisplay : IInventoryDisplay
        {
            ItemContainer itemContainer { get; set; }
            Vector2 gridSize { get; set; }
        }

        //displays
        public class EssentialDisplayElement
        {
            public MasterItemStack essential;
            public GameObject displayObject;

            public EssentialDisplayElement(MasterItemStack essential, GameObject displayObject)
            {
                this.essential = essential;
                this.displayObject = displayObject;
            }
        }
        public class EssentialsDisplayBar : IEssentialsDisplay
        {
            private MasterItemStack[] m_items;
            private bool m_displayNames;
            private bool m_displayIcons;

            private List<EssentialDisplayElement> m_displayElements;
            private GameObject m_gameObject;
            private GameObject m_elementPrefab;

            private RectTransform m_transform;
            private CanvasGroup m_group;
            private Vector2 m_elementSize;

            public EssentialsDisplayBar()
            {
                m_gameObject = GameObject.Instantiate(Resources.Load("UI/EssentialsDisplayBar") as GameObject, Vector2.zero, Quaternion.identity) as GameObject;
                m_gameObject.name = menuName;
                m_displayElements = new List<EssentialDisplayElement>();

                m_transform = m_gameObject.GetComponent<RectTransform>();
                m_group = m_gameObject.GetComponent<CanvasGroup>();
                m_transform.SetParent(GameObject.FindGameObjectWithTag("UI").GetComponent<RectTransform>(), false);

                m_elementPrefab = Resources.Load("UI/EssentialElement") as GameObject;
                m_elementSize = m_elementPrefab.GetComponent<RectTransform>().sizeDelta;
            }

            public void BuildUI()
            {

                m_transform.sizeDelta = new Vector2(getEssentials.Length * m_elementSize.x, m_transform.sizeDelta.y);
                MasterItemStack[] items = getEssentials;

                for (int i = 0; i < items.Length; i++)
                {
                    GameObject element = GameObject.Instantiate(m_elementPrefab, Vector3.zero, Quaternion.identity) as GameObject;
                    element.transform.SetParent(m_gameObject.transform, false);
                    element.transform.FindChild("Name").GetComponent<Text>().text = items[i].item.displayName;
                    element.transform.FindChild("Amount").GetComponent<Text>().text = "" + items[i].amount;
                    element.transform.FindChild("Image").GetComponent<Image>().sprite = items[i].item.GetGraphics().icon;

                    m_displayElements.Add(new EssentialDisplayElement(items[i], element));
                }
                RealligenElements();
                RecalculateWindowSize();
            }

            public void UpdateUI()
            {
                MasterItemStack[] items = getEssentials;

                m_transform.sizeDelta = new Vector2(getEssentials.Length * m_elementSize.x, m_transform.sizeDelta.y);

                for (int r = 0; r < items.Length; r++)
                {
                    bool elementFound = false;
                    for (int i = 0; i < m_displayElements.Count; i++)
                    {
                        if (m_displayElements[i].essential.item == items[r].item)
                        {
                            m_displayElements[i].displayObject.transform.FindChild("Amount").GetComponent<Text>().text = "" + items[r].amount;
                            elementFound = true;
                        }
                    }
                    if(!elementFound)
                    {
                        GameObject element = GameObject.Instantiate(m_elementPrefab, Vector3.zero, Quaternion.identity) as GameObject;
                        element.transform.SetParent(m_gameObject.transform, false);
                        element.transform.FindChild("Name").GetComponent<Text>().text = items[r].item.displayName;
                        element.transform.FindChild("Amount").GetComponent<Text>().text = "" + items[r].amount;
                        element.transform.FindChild("Image").GetComponent<Image>().sprite = items[r].item.GetGraphics().icon;

                        m_displayElements.Add(new EssentialDisplayElement(items[r], element));
                    }
                }
                RealligenElements();
                RecalculateWindowSize();
            }

            private void RealligenElements()
            {
                for (int i = 0; i < m_displayElements.Count; i++)
                {
                    if (i != 0)
                    {
                        float prevX = m_displayElements[i - 1].displayObject.GetComponent<RectTransform>().localPosition.x;
                        float prevTextWidth = m_displayElements[i - 1].displayObject.transform.FindChild("Name").GetComponent<RectTransform>().rect.width;
                        float prevAmountWidth = m_displayElements[i - 1].displayObject.transform.FindChild("Amount").GetComponent<RectTransform>().rect.width;

                        float biggestWidth = 0;
                        if (prevTextWidth > prevAmountWidth)
                            biggestWidth = prevTextWidth;
                        else
                            biggestWidth = prevAmountWidth;

                        float rest = m_displayElements[i - 1].displayObject.GetComponent<RectTransform>().rect.width - biggestWidth;

                        m_displayElements[i].displayObject.GetComponent<RectTransform>().localPosition = new Vector2(prevX + rest + 16 + (3 * 3), windowSize.y / 2);
                    }
                    else
                        m_displayElements[i].displayObject.GetComponent<RectTransform>().localPosition = new Vector2(0, windowSize.y / 2);
                }
            }

            private void RecalculateWindowSize()
            {
                float width = 0;
                for (int i = 0; i < m_displayElements.Count; i++)
                {
                    width += (m_displayElements[i].displayObject.GetComponent<RectTransform>().rect.width);
                }
                m_transform.sizeDelta = new Vector2(width, m_transform.sizeDelta.y);
            }

            public void Toggle()
            {
                bool state = !m_group.interactable;
                m_group.alpha = System.Convert.ToInt32(state);
                m_group.interactable = state;
                m_group.blocksRaycasts = state;
            }
            public void Toggle(bool state)
            {
                m_group.alpha = System.Convert.ToInt32(state);
                m_group.interactable = state;
                m_group.blocksRaycasts = state;
            }

            public string menuName
            {
                get
                {
                    return "EssentialDisplayBar";
                }
                set
                {
                    m_gameObject.name = value;
                }
            }

            public Vector2 position
            {
                get
                {
                    return m_transform.localPosition;
                }
                set
                {
                    m_transform.localPosition = value;
                }
            }

            public Vector2 windowSize
            {
                get
                {
                    return m_transform.sizeDelta;
                }
                set
                {
                    m_transform.sizeDelta = value;
                }
            }

            private MasterItemStack[] getEssentials
            {
                get
                {
                    return ManagerInstance.Get<SimulationManager>().avaiableEssentialsLeftThisTick;
                }
            }

            public bool displayNames
            {
                get
                {
                    throw new System.NotImplementedException();
                }
                set
                {
                    throw new System.NotImplementedException();
                }
            }

            public bool displayIcons
            {
                get
                {
                    throw new System.NotImplementedException();
                }
                set
                {
                    throw new System.NotImplementedException();
                }
            }

            public int maxItemsPerRow
            {
                get
                {
                    throw new System.NotImplementedException();
                }
                set
                {
                    throw new System.NotImplementedException();
                }
            }

            public int rows
            {
                get
                {
                    throw new System.NotImplementedException();
                }
                set
                {
                    throw new System.NotImplementedException();
                }
            }

            public RectTransform transform
            {
                get
                {
                    return m_gameObject.GetComponent<RectTransform>();
                }
            }
        }
        public class ItemContainerDisplay : IItemContainerDisplay
        {
            //dataholder
            private ItemContainer m_container;

            //Gameobjects
            private GameObject m_gameObject;
            private List<GameObject> m_displayElements;
            private GameObject m_elementPrefab;

            //UI
            private RectTransform m_transform;
            private RectTransform m_elementTransform;
            private CanvasGroup m_group;
            private GridLayoutGroup m_layout;

            private Vector2 m_gridSize;

            public ItemContainerDisplay(ItemContainer itemContainer)
            {
                this.itemContainer = itemContainer;

                m_gameObject = GameObject.Instantiate(Resources.Load("UI/ItemContainerDisplay") as GameObject, Vector2.zero, Quaternion.identity) as GameObject;
                m_gameObject.name = menuName;
                m_displayElements = new List<GameObject>();

                m_transform = m_gameObject.GetComponent<RectTransform>();
                m_group = m_gameObject.GetComponent<CanvasGroup>();
                m_layout = m_gameObject.GetComponent<GridLayoutGroup>();
                m_transform.SetParent(GameObject.FindGameObjectWithTag("UI").GetComponent<RectTransform>(), false);

                m_elementPrefab = Resources.Load("UI/ItemElement") as GameObject;
                m_elementTransform = m_elementPrefab.GetComponent<RectTransform>();

                m_gridSize = new Vector2(itemContainer.GetAllStacks().Length / 2, itemContainer.GetAllStacks().Length / 2);
            }

            public void UpdateUI() { }
            public void UpdateSlot(int index)
            {
                ItemStack stack = itemContainer.GetStackAt(index);
                if (stack != null)
                {
                    m_displayElements[index].transform.FindChild("Amount").GetComponent<Text>().text = "" + stack.amount;
                    m_displayElements[index].transform.FindChild("Image").GetComponent<Image>().sprite = stack.item.GetGraphics().icon;
                    m_displayElements[index].transform.FindChild("Image").GetComponent<Image>().enabled = true;
                }
                else
                {
                    m_displayElements[index].transform.FindChild("Amount").GetComponent<Text>().text = "";
                    m_displayElements[index].transform.FindChild("Image").GetComponent<Image>().enabled = false;
                }
            }

            public void BuildUI()
            {
                float xSize = ((m_elementTransform.rect.width + m_layout.padding.left) * gridSize.x) + m_layout.padding.right;
                float ySize = ((m_elementTransform.rect.height + m_layout.padding.top) * gridSize.y) + m_layout.padding.top;
                m_transform.sizeDelta = new Vector2(xSize, ySize);

                for (int i = 0; i < itemContainer.GetAllStacks().Length; i++)
                {
                    ItemStack stack = itemContainer.GetAllStacks()[i];
                    GameObject element = GameObject.Instantiate(m_elementPrefab, Vector3.zero, Quaternion.identity) as GameObject;
                    element.transform.SetParent(m_gameObject.transform, false);
                    if(stack != null)
                    {
                        element.transform.FindChild("Amount").GetComponent<Text>().text = "" + stack.amount;
                        element.transform.FindChild("Image").GetComponent<Image>().sprite = stack.item.GetGraphics().icon;
                    }
                    else
                    {
                        element.transform.FindChild("Amount").GetComponent<Text>().text = "";
                        element.transform.FindChild("Image").GetComponent<Image>().enabled = false;
                    }

                    m_displayElements.Add(element);
                }
            }

            public void Toggle()
            {
                bool state = !m_group.interactable;
                m_group.alpha = System.Convert.ToInt32(state);
                m_group.interactable = state;
                m_group.blocksRaycasts = state;
            }
            public void Toggle(bool state)
            {
                m_group.alpha = System.Convert.ToInt32(state);
                m_group.interactable = state;
                m_group.blocksRaycasts = state;
            }

            public ItemContainer itemContainer
            {
                get
                {
                    return m_container;
                }
                set
                {
                    if(m_container != null)
                        m_container.OnStackUpdate -= UpdateSlot;
                    m_container = value;
                    m_container.OnStackUpdate += UpdateSlot;
                }
            }
            public string menuName
            {
                get
                {
                    return "Inventory screen";
                }
                set
                {
                    throw new System.NotImplementedException();
                }
            }
            public Vector2 position
            {
                get
                {
                    return m_transform.anchoredPosition;
                }
                set
                {
                    m_transform.anchoredPosition = value;
                }
            }
            public Vector2 windowSize
            {
                get
                {
                    return m_transform.sizeDelta;
                }
                set
                {
                    m_transform.sizeDelta = value;
                }
            }
            public Vector2 gridSize
            {
                get
                {
                    return m_gridSize;
                }
                set
                {
                    m_gridSize = value;
                }
            }
            public RectTransform transform
            {
                get
                {
                    return m_gameObject.GetComponent<RectTransform>();
                }
            }
        }
        public class ActionMenuList : IActionMenu
        {
            private RectTransform m_transform;
            private CanvasGroup m_group;
            private List<ActionButton> m_buttons;
            private string m_menuName;
            private bool m_built;

            private Vector2 m_buttonSize;

            private GameObject m_buttonPrefab;
            private GameObject m_gameObject;

            public ActionMenuList()
            {
                m_gameObject = GameObject.Instantiate(Resources.Load("UI/ActionMenuList") as GameObject, Vector2.zero, Quaternion.identity) as GameObject;
                m_gameObject.name = "ActionMenuList";

                m_transform = m_gameObject.GetComponent<RectTransform>();
                m_group = m_gameObject.GetComponent<CanvasGroup>();
                m_transform.SetParent(GameObject.FindGameObjectWithTag("UI").GetComponent<RectTransform>(), false);

                m_buttons = new List<ActionButton>();
                m_buttonPrefab = Resources.Load("UI/ActionButton") as GameObject;
            }

            public virtual void BuildUI()
            {
                if (m_buttonSize == null)
                    m_buttonSize = Vector2.zero;

                RectOffset padding = m_gameObject.GetComponent<VerticalLayoutGroup>().padding;

                m_transform.sizeDelta = new Vector2(m_transform.sizeDelta.x, (padding.top + padding.bottom) + m_buttons.Count * m_buttonSize.y);

                for (int i = 0; i < m_buttons.Count; i++)
                {
                    GameObject button = GameObject.Instantiate(m_buttonPrefab, Vector3.zero, Quaternion.identity) as GameObject;
                    button.transform.SetParent(m_gameObject.transform, false);
                    button.transform.FindChild("Text").GetComponent<Text>().text = m_buttons[i].text;

                    UnityEngine.Events.UnityAction action = m_buttons[i].OnButtonPress;
                    button.GetComponent<Button>().onClick.AddListener(() => action());
                }

                m_built = true;
            }

            private void ClearUI()
            {
                for (int i = 0; i < m_gameObject.transform.childCount; i++)
                {
                    m_gameObject.transform.GetChild(i).gameObject.AddComponent<GameObjectDestroyer>().Destroy();
                }
            }

            public void Toggle()
            {
                bool state = !m_group.interactable;
                m_group.alpha = System.Convert.ToInt32(state);
                m_group.interactable = state;
                m_group.blocksRaycasts = state;
            }
            public void Toggle(bool state)
            {
                m_group.alpha = System.Convert.ToInt32(state);
                m_group.interactable = state;
                m_group.blocksRaycasts = state;
            }

            public void AddButton(ActionButton button)
            {
                m_buttons.Add(button);
                if(m_built)
                {
                    ClearUI();
                    BuildUI();
                }
            }
            public void RemoveButton(ActionButton button)
            {
                if (m_buttons.Contains(button))
                    m_buttons.Remove(button);
            }

            public ActionButton[] buttons
            {
                get { return m_buttons.ToArray(); }
            }
            public virtual string menuName
            {
                get { return m_menuName; }
                set
                {
                    m_menuName = value;
                    m_gameObject.name = m_menuName;
                }
            }

            public Vector2 position
            {
                get
                {
                    return m_transform.anchoredPosition;
                }
                set
                {
                    m_transform.anchoredPosition = value;
                }
            }
            public Vector2 windowSize
            {
                get
                {
                    return m_transform.sizeDelta;
                }
                set
                {
                    m_transform.sizeDelta = value;
                }
            }
            public Vector2 buttonSize
            {
                get
                {
                    return m_buttonSize;
                }
                set
                {
                    m_buttonSize = value;
                }
            }
            public RectTransform transform
            {
                get
                {
                    return m_gameObject.GetComponent<RectTransform>();
                }
            }
        }

        //groups
        public class EntityUIGroup
        {
            public List<IUI> m_uiElements;
            private List<bool> m_openOnSelect;

            public EntityUIGroup()
            {
                m_uiElements = new List<IUI>();
                m_openOnSelect = new List<bool>();
            }

            public void AddUIElement(IUI element)
            {
                m_uiElements.Add(element);
                m_openOnSelect.Add(true);
            }
            public void RemoveUIElement(IUI element)
            {
                m_uiElements.Remove(element);
                m_openOnSelect.RemoveAt(System.Array.IndexOf(m_uiElements.ToArray(), element));
            }

            public T GetElement<T>() where T : IUI
            {
                for (int i = 0; i < m_uiElements.Count; i++)
                {
                    if (m_uiElements[i].GetType() == typeof(T))
                        return (T)m_uiElements[i];
                }
                return default(T);
            }

            public void Toggle()
            {
                for (int i = 0; i < m_uiElements.Count; i++)
                {
                    if (!m_openOnSelect[i])
                        continue;

                    CanvasGroup CG = m_uiElements[i].transform.GetComponent<CanvasGroup>();
                    if(CG != null)
                    {
                        bool state = CG.interactable;
                        CG.alpha = System.Convert.ToInt32(state);
                        CG.interactable = state;
                        CG.blocksRaycasts = state;
                    }
                }
            }
            public void Toggle(bool state)
            {
                for (int i = 0; i < m_uiElements.Count; i++)
                {
                    if (!m_openOnSelect[i])
                        continue;

                    CanvasGroup CG = m_uiElements[i].transform.GetComponent<CanvasGroup>();
                    if (CG != null)
                    {
                        CG.alpha = System.Convert.ToInt32(state);
                        CG.interactable = state;
                        CG.blocksRaycasts = state;
                    }
                }
            }

            public void ChangeOpenOnSelect(IUI element, bool state)
            {
                for (int i = 0; i < m_uiElements.Count; i++)
                {
                    if (m_uiElements[i] == element)
                        m_openOnSelect[i] = state;
                    break;
                }
            }
            public void ChangeOpenOnSelect<T>(bool state) where T : IUI
            {
                T element = GetElement<T>();
                if (element != null)
                    m_openOnSelect[System.Array.IndexOf(m_uiElements.ToArray(), element)] = state;
            }
        }
    }
}