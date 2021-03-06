﻿using UnityEngine;
using UnityEngine.UI;

using System.Collections.Generic;

using EndlessExpedition.Managers;
using EndlessExpedition.Items;
using EndlessExpedition.Entities;

using DG.Tweening;
using DG.Tweening.Plugins;
using EndlessExpedition.Serialization;
using System;

namespace EndlessExpedition
{
    namespace UI
    {
        public interface IUI
        {
            void BuildUI();
            void Toggle();
            void Toggle(bool state);
            string menuName { get; }
            Vector2 position { get; set; }
            Vector2 windowSize { get; set; }
            RectTransform transform { get; }
            Vector2 scale { get; set; }
        }

        public interface IInventoryDisplay : IUI
        {
            void UpdateUI();
        }

        //interfaces
        public interface IActionMenu : IUI
        {
            void AddButton(ActionButton button);
            void RemoveButton(ActionButton button);
            ActionButton[] buttons { get; }
            Vector2 buttonSize { get; set; }
        }
        public interface IPropertyDisplay : IUI
        {
            void AddDisplay(string displayTitle, string propertyIdentity);
            void RemoveDisplay(string displayTitle, string propertyIdentity);
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
                m_gameObject.SetActive(state);
            }
            public void Toggle(bool state)
            {
                m_group.alpha = System.Convert.ToInt32(state);
                m_group.interactable = state;
                m_group.blocksRaycasts = state;
                m_gameObject.SetActive(state);
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
            public Vector2 scale
            {
                get
                {
                    return m_transform.localScale;
                }
                set
                {
                    m_transform.localScale = value;
                }
            }
        }

        public class ItemContainerDisplayElement : MonoBehaviour
        {
            private ItemContainerDisplay m_display;
            private bool m_moving;
            private int m_elementIndex;

            private GameObject m_amount, m_icon;
            private GameObject m_preview;

            private Color m_defaultColor;

            public void SetInfo(ItemContainerDisplay display, int index)
            {
                m_display = display;
                m_elementIndex = index;

                m_amount = gameObject.transform.FindChild("Amount").gameObject;
                m_icon = gameObject.transform.FindChild("Image").gameObject;
                m_defaultColor = transform.FindChild("Background").GetComponent<Image>().color;
            }

            public void OnMouseOver()
            {
                transform.FindChild("Background").GetComponent<Image>().color = new Color(115, 115, 115);
                InputManager.isMouseOverItemSlot = true;

                if(m_display.state)
                {
                    if (Input.GetMouseButtonDown(0) && ManagerInstance.Get<InputManager>().currentMovingItemstack == null && stack != null)
                    {
                        ManagerInstance.Get<InputManager>().currentMovingItemstack = new ItemStackTransferInfo(stack, m_display.itemContainer, m_elementIndex);
                        m_moving = true;
                    }
                    if(Input.GetMouseButtonUp(0))
                    {
                        TransferStack();
                    }
                }
            }

            private void TransferStack()
            {
                ItemStackTransferInfo info = ManagerInstance.Get<InputManager>().currentMovingItemstack;
                if (info != null)
                {
                    ItemStack stack = info.itemContainer.TakeStackAt(info.containerIndex);

                    if (stack == null)
                        Debug.Log("stack = null");

                    if (!m_display.itemContainer.AddStackAt(stack, m_elementIndex))
                    {
                        FailTrainsfer();
                        return;
                    }
                }
                m_moving = false;
                GameObject[] previews = GameObject.FindGameObjectsWithTag("ItemTransferPreview");
                for (int i = 0; i < previews.Length; i++)
                {
                    Destroy(previews[i]);
                }
                ManagerInstance.Get<InputManager>().currentMovingItemstack = null;
            }

            private void OnMouseExit()
            {
                transform.FindChild("Background").GetComponent<Image>().color = m_defaultColor;
                InputManager.isMouseOverItemSlot = false;
            }

            private void Update()
            {
                if (stack == null)
                    return;

                if(m_moving)
                {
                    if (m_preview != null)
                    {
                        m_preview.GetComponent<RectTransform>().anchoredPosition = Input.mousePosition;
                        m_amount.GetComponent<Text>().text = "";
                        m_icon.GetComponent<Image>().enabled = false;
                    }
                    else if (ManagerInstance.Get<InputManager>().currentMovingItemstack != null)
                    {
                        CreatePreviewObject();
                    }

                    if (Input.GetMouseButtonUp(0) && !InputManager.isMouseOverItemSlot)
                    {
                        FailTrainsfer();
                    }
                }
            }

            private void FailTrainsfer()
            {
                m_display.UpdateSlots(m_elementIndex);
                //ManagerInstance.Get<InputManager>().currentMovingItemstack.itemContainer.OnStackUpdate(ManagerInstance.Get<InputManager>().currentMovingItemstack.containerIndex);
                m_moving = false;
                Destroy(m_preview);
                ManagerInstance.Get<InputManager>().currentMovingItemstack = null;
            }

            private void CreatePreviewObject()
            {
                m_preview = GameObject.Instantiate(Resources.Load("UI/ItemElement") as GameObject, Vector3.zero, Quaternion.identity) as GameObject;
                m_preview.transform.tag = "ItemTransferPreview";

                m_preview.transform.FindChild("Amount").GetComponent<Text>().text = "" + stack.amount;
                m_preview.transform.FindChild("Image").GetComponent<Image>().sprite = stack.item.GetGraphics().icon;
                m_preview.transform.FindChild("Image").GetComponent<Image>().enabled = true;
                m_preview.transform.FindChild("Background").GetComponent<Image>().raycastTarget = false;

                m_preview.transform.SetParent(GameObject.FindGameObjectWithTag("UI").transform, false);
                m_preview.transform.localScale = new Vector3(1, 1, 1);
            }

            private ItemStack stack
            {
                get
                {
                    return m_display.itemContainer.GetStackAt(m_elementIndex);
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

            //Initial
            private bool m_initialSet;
            private Vector2 m_initialPosition;

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
            }

            public void UpdateUI() { }
            public void UpdateSlots(int index)
            {
                ItemStack stack = itemContainer.GetStackAt(index);
                if (stack != null)
                {
                    m_displayElements[index].transform.FindChild("Amount").GetComponent<Text>().text = "" + stack.amount;
                    m_displayElements[index].transform.FindChild("Image").GetComponent<Image>().sprite = stack.item.GetGraphics().icon;
                    m_displayElements[index].transform.FindChild("Image").GetComponent<Image>().enabled = true;
                    m_displayElements[index].transform.FindChild("Image").transform.DOScale(new Vector3(1, 1, 1), 1f);


                    m_displayElements[index].transform.FindChild("Amount").GetComponent<Text>().DOColor(Color.white, 0.15f).OnComplete(
                        () => m_displayElements[index].transform.FindChild("Amount").GetComponent<Text>().DOColor(new Color(0.88f, 0.88f, 0.88f), 0.45f));
                }
                else
                {
                    m_displayElements[index].transform.FindChild("Amount").GetComponent<Text>().text = "";
                    m_displayElements[index].transform.FindChild("Image").transform.DOScale(new Vector3(0, 0, 1), 1f);
                }
            }

            public void BuildUI()
            {
                int items = itemContainer.GetAllStacks().Length;
                int rows = Mathf.CeilToInt((float)items / (float)5);

                float xSize = 0;
                float ySize = 0;

                if (rows > 1)
                    xSize = ((m_elementTransform.rect.width + m_layout.padding.left) * 5) + m_layout.padding.right;
                else
                    xSize = ((m_elementTransform.rect.width + m_layout.padding.left) * itemContainer.GetAllStacks().Length) + m_layout.padding.right;
                ySize = ((m_elementTransform.rect.height + m_layout.padding.top) * rows) + m_layout.padding.top;
                m_transform.sizeDelta = new Vector2(xSize, ySize);

                for (int i = 0; i < itemContainer.GetAllStacks().Length; i++)
                {
                    ItemStack stack = itemContainer.GetAllStacks()[i];
                    GameObject element = GameObject.Instantiate(m_elementPrefab, Vector3.zero, Quaternion.identity) as GameObject;
                    element.transform.SetParent(m_gameObject.transform, false);
                    element.transform.localScale = new Vector3(1, 1, 1);
                    element.AddComponent<ItemContainerDisplayElement>().SetInfo(this, i);

                    if(stack != null)
                    {
                        element.transform.FindChild("Amount").GetComponent<Text>().text = "" + stack.amount;
                        element.transform.FindChild("Image").GetComponent<Image>().sprite = stack.item.GetGraphics().icon;
                        element.transform.FindChild("Image").GetComponent<Image>().transform.localScale = new Vector3(0, 0, 1);
                        element.transform.FindChild("Image").GetComponent<Image>().transform.DOScale(new Vector3(1, 1, 1), 1f);
                    }
                    else
                    {
                        element.transform.FindChild("Amount").GetComponent<Text>().text = "";
                        element.transform.FindChild("Image").GetComponent<Image>().enabled = false;
                        element.transform.FindChild("Image").GetComponent<Image>().transform.localScale = new Vector3(0, 0, 1);
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
                for (int i = 0; i < transform.childCount; i++)
                {
                    if (state)
                        transform.GetChild(i).GetComponent<BoxCollider2D>().size = m_elementTransform.rect.size;
                    else
                        transform.GetChild(i).GetComponent<BoxCollider2D>().size = Vector2.zero;
                }
                m_gameObject.SetActive(state);
            }
            public void Toggle(bool state)
            {
                m_group.alpha = System.Convert.ToInt32(state);
                m_group.interactable = state;
                m_group.blocksRaycasts = state;
                for (int i = 0; i < transform.childCount; i++)
                {
                    if (state)
                        transform.GetChild(i).GetComponent<BoxCollider2D>().size = m_elementTransform.rect.size;
                    else
                        transform.GetChild(i).GetComponent<BoxCollider2D>().size = Vector2.zero;
                }
                m_gameObject.SetActive(state);
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
                        m_container.OnStackUpdate -= UpdateSlots;
                    m_container = value;
                    m_container.OnStackUpdate += UpdateSlots;
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
                    if(!m_initialSet)
                    {
                        m_initialPosition = value;
                        m_initialSet = true;
                    }
                }
            }
            public Vector2 initialPosition
            {
                get
                {
                    return m_initialPosition;
                }
            }
            public Vector2 windowSize
            {
                get
                {
                    Vector2 actualSize = new Vector2();
                    actualSize.x = m_transform.sizeDelta.x * m_transform.localScale.x;
                    actualSize.y = m_transform.sizeDelta.y * m_transform.localScale.y;
                    return actualSize;
                }
                set
                {
                    m_transform.sizeDelta = value;
                }
            }
            public RectTransform transform
            {
                get
                {
                    return m_transform;
                }
            }
            public Vector2 scale
            {
                get
                {
                    return m_transform.localScale;
                }
                set
                {
                    m_transform.localScale = new Vector3(value.x, value.y, 1);
                }
            }
            public bool state
            {
                get
                {
                    return System.Convert.ToBoolean(m_group.alpha);
                }
            }
            public GameObject GetUIElementAt(int index)
            {
                if (index < m_displayElements.Count)
                    return m_displayElements[index];
                return null;
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
                m_gameObject.SetActive(state);
            }
            public void Toggle(bool state)
            {
                m_group.alpha = System.Convert.ToInt32(state);
                m_group.interactable = state;
                m_group.blocksRaycasts = state;
                m_gameObject.SetActive(state);
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
            public Vector2 scale
            {
                get
                {
                    return m_transform.localScale;
                }
                set
                {
                    m_transform.localScale = value;
                }
            }
        }

        public class EntityPropertyDisplay : IPropertyDisplay
        {
            private RectTransform m_transform;
            private CanvasGroup m_group;
            private Properties m_properties;
            private List<string> m_propertyTitles;
            private List<int> m_propertiesToDisplay;
            private List<GameObject> m_displays;
            private string m_menuName;
            private bool m_built;

            private Vector2 m_displaySize;

            private GameObject m_displayPrefab;
            private GameObject m_gameObject;

            public EntityPropertyDisplay()
            {
                m_gameObject = GameObject.Instantiate(Resources.Load("UI/ActionMenuList") as GameObject, Vector2.zero, Quaternion.identity) as GameObject;
                m_gameObject.name = "ActionMenuList";

                m_transform = m_gameObject.GetComponent<RectTransform>();
                m_group = m_gameObject.GetComponent<CanvasGroup>();
                m_transform.SetParent(GameObject.FindGameObjectWithTag("UI").GetComponent<RectTransform>(), false);

                m_propertyTitles = new List<string>();
                m_propertiesToDisplay = new List<int>();
                m_displays = new List<GameObject>();

                m_displayPrefab = Resources.Load("UI/PropertyDisplay") as GameObject;
            }

            public Properties properties
            {
                get
                {
                    return m_properties;
                }
                set
                {
                    m_properties = value;
                    m_properties.OnValueChangeEvent -= RefreshUI;
                    m_properties.OnValueChangeEvent += RefreshUI;
                }
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
            public Vector2 displaySize
            {
                set
                {
                    m_displaySize = value;
                }
                get
                {
                    return m_displaySize;
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
            public RectTransform transform
            {
                get
                {
                    return m_gameObject.GetComponent<RectTransform>();
                }
            }
            public Vector2 scale
            {
                get
                {
                    return m_transform.localScale;
                }
                set
                {
                    m_transform.localScale = value;
                }
            }

            public void AddDisplay(string displayTitle, string propertyIdentity)
            {
                m_propertyTitles.Add(displayTitle);

                int index = m_properties.Index(propertyIdentity);
                if(index >= 0)
                {
                    m_propertiesToDisplay.Add(index);
                }
            }

            public void BuildUI()
            {
                if (m_displaySize == null)
                    m_displaySize = Vector2.zero;

                RectOffset padding = m_gameObject.GetComponent<VerticalLayoutGroup>().padding;

                m_transform.sizeDelta = new Vector2(padding.left + padding.right + m_displaySize.x, (padding.top + padding.bottom) + m_propertiesToDisplay.Count * m_displaySize.y);

                for (int i = 0; i < m_propertiesToDisplay.Count; i++)
                {
                    GameObject display = GameObject.Instantiate(m_displayPrefab, Vector3.zero, Quaternion.identity) as GameObject;
                    display.transform.SetParent(m_gameObject.transform, false);
                    display.transform.FindChild("Title").GetComponent<Text>().text = m_propertyTitles[i];
                    display.transform.FindChild("Value").GetComponent<Text>().text = m_properties.GetAll()[m_propertiesToDisplay[i]].Value.ToString();

                    m_displays.Add(display);
                }

                m_built = true;
            }

            //TODO: Use propertyIndex
            public void RefreshUI(int propertyIndex)
            {
                if (!m_built)
                    return;

                //Destroy old displays
                for (int i = 0; i < m_displays.Count; i++)
                {
                    m_displays[i].AddComponent<GameObjectDestroyer>().Destroy();
                }

                m_displays.Clear();

                RectOffset padding = m_gameObject.GetComponent<VerticalLayoutGroup>().padding;

                m_transform.sizeDelta = new Vector2(m_transform.sizeDelta.x, (padding.top + padding.bottom) + m_propertiesToDisplay.Count * m_displaySize.y);

                //Build new
                for (int i = 0; i < m_propertiesToDisplay.Count; i++)
                {
                    GameObject display = GameObject.Instantiate(m_displayPrefab, Vector3.zero, Quaternion.identity) as GameObject;
                    display.transform.SetParent(m_gameObject.transform, false);
                    display.transform.FindChild("Title").GetComponent<Text>().text = m_propertyTitles[i];
                    display.transform.FindChild("Value").GetComponent<Text>().text = m_properties.GetAll()[m_propertiesToDisplay[i]].ToString();

                    m_displays.Add(display);
                }
            }

            public void RemoveDisplay(string displayTitle, string propertyIdentity)
            {
                throw new NotImplementedException();
            }

            public void Toggle()
            {
                bool state = !m_group.interactable;
                m_group.alpha = Convert.ToInt32(state);
                m_group.interactable = state;
                m_group.blocksRaycasts = state;
                m_gameObject.SetActive(state);
            }
            public void Toggle(bool state)
            {
                m_group.alpha = Convert.ToInt32(state);
                m_group.interactable = state;
                m_group.blocksRaycasts = state;
                m_gameObject.SetActive(state);
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

                    m_uiElements[i].Toggle();
                }
            }
            public void Toggle(bool state)
            {
                for (int i = 0; i < m_uiElements.Count; i++)
                {
                    if (!m_openOnSelect[i])
                        continue;

                    m_uiElements[i].Toggle(state);
                }
            }

            public void ChangeOpenOnSelect(IUI element, bool state)
            {
                for (int i = 0; i < m_uiElements.Count; i++)
			    {
                    if (m_uiElements[i] == element)
                    {
                        m_openOnSelect[i] = state;
                        break;
                    }
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