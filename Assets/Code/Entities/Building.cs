using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using FinalFrontier.Managers;
using FinalFrontier.Graphics;
using FinalFrontier.Items;
using FinalFrontier.UI;
using FinalFrontier.Entities.BuildingModules;

namespace FinalFrontier
{
    namespace Entities
    {
        public class Building : Entity
        {
            private const int LIGHT_HEIGHT = 2;
            private const float STATUS_ICON_SCALE = 0.24f;

            private BuildingGraphics m_graphics;
            private List<BuildingModule> m_modules;
            private ItemContainer m_itemContainer;

            //Building objects
            private Light m_light;

            //Alert UI
            private GameObject m_localCanvas;
            private Dictionary<string, GameObject> m_statusIcons;

            public override void OnStart()
            {
                base.OnStart();
                p_properties.Secure("identity", "unnamedBuilding");
                p_properties.Secure("entityType", "building");

                m_graphics = new BuildingGraphics(this);
                m_statusIcons = new Dictionary<string, GameObject>();
                m_modules = new List<BuildingModule>();

                if(properties.Has("produces"))
                {
                    AddModule(new ProductionModule(this));
                }
                if (properties.Has("itemContainerSlots"))
                {
                    AddItemContainerSpace(properties.Get<int>("itemContainerSlots"));
                }

                #region Building menus

                ActionMenuList debugMenu = new ActionMenuList();
                debugMenu.buttonSize = new Vector2(100, 30);
                debugMenu.AddButton(new ActionButton("Close", debugMenu.Toggle));
                debugMenu.menuName = properties.Get<string>("displayName") + ": Debug Menu";
                debugMenu.position = new Vector3(Screen.width - debugMenu.windowSize.x, 0);
                debugMenu.BuildUI();
                debugMenu.Toggle(false);
                ManagerInstance.Get<UIManager>().AddUI(debugMenu);
                p_UIGroup.AddUIElement(debugMenu);

                if (GetModule<ProductionModule>() != null)
                    debugMenu.AddButton(new ActionButton("Toggle Pause", GetModule<ProductionModule>().TogglePause));

                if (properties.Has("itemContainerSlots"))
                {
                    ItemContainerDisplay containerDisplay = new ItemContainerDisplay(itemContainer);
                    containerDisplay.gridSize = new Vector2(4, 2);
                    containerDisplay.BuildUI();
                    containerDisplay.Toggle(false);
                    ManagerInstance.Get<UIManager>().AddUI(containerDisplay);
                    p_UIGroup.AddUIElement(containerDisplay);
                    
                    containerDisplay.position = new Vector3(Screen.width -
                     containerDisplay.windowSize.x,
                     debugMenu.windowSize.y);
                }
                #endregion
                #region Status Icons
                m_localCanvas = new GameObject("Local Canvas");
                Canvas canvas = m_localCanvas.AddComponent<Canvas>();
                RectTransform transform = m_localCanvas.GetComponent<RectTransform>();

                canvas.renderMode = RenderMode.WorldSpace;

                transform.position = new Vector3(0, 0);
                transform.sizeDelta = new Vector2(1, 1);
                transform.SetParent(p_gameObject.transform, false);

                #endregion
                #region Lighting
                GameObject lightObj = new GameObject("Light");
                lightObj.transform.SetParent(p_gameObject.transform, false);

                m_light = lightObj.AddComponent<Light>();
                m_light.type = LightType.Point;
                if (properties.Has("lightStrength"))
                    m_light.intensity = properties.Get<int>("lightStrength");
                else
                    m_light.intensity = 3;
                if(properties.Has("lightSize"))
                {
                    m_light.range = properties.Get<int>("lightSize");
                }
                else
                {
                    m_light.range = (p_properties.Get<int>("tileWidth") * p_properties.Get<int>("tileHeight")) + LIGHT_HEIGHT;
                    if (m_light.range < 1)
                        m_light.range = 1 + LIGHT_HEIGHT;
                }

                lightObj.transform.Translate(new Vector3(0, 0, -m_light.intensity));

                if(properties.Has("lightColor"))
                {
                    string p = properties.Get<string>("lightColor");
                    string[] split = p.Split('/');
                    if(split.Length == 3)
                    {
                        int r = Int32.Parse(split[0]);
                        int g = Int32.Parse(split[1]);
                        int b = Int32.Parse(split[2]);
                        m_light.color = new Color(r, g, b);
                    }else if (split.Length == 4)
                    {
                        int r = Int32.Parse(split[0]);
                        int g = Int32.Parse(split[1]);
                        int b = Int32.Parse(split[2]);
                        float a = float.Parse(split[3]);
                        m_light.color = new Color(r, g, b, a);
                    }
                }
                #endregion
            }

            public override void OnTick()
            {
                base.OnTick();
            }

            public override void OnUpdate()
            {
                base.OnUpdate();
            }
            
            public override void SetGraphics(GraphicsBase graphics)
            {
                m_graphics = graphics as BuildingGraphics;
            }

            public override void OnSelect()
            {
                base.OnSelect();
            }

            public override void OnDeselect()
            {
                base.OnDeselect();
            }

            #region Modules
            public void RemoveModule(BuildingModule module)
            {
                if (m_modules.Contains(module))
                    m_modules.Remove(module);
            }
            public void AddModule(BuildingModule module)
            {
                m_modules.Add(module);
                module.OnStart();
            }
            public T GetModule<T>() where T : BuildingModule
            {
                for (int i = 0; i < m_modules.Count; i++)
                {
                    if (m_modules[i].GetType() == typeof(T))
                        return m_modules[i] as T;
                }
                return default(T);
            }

            public void AddItemContainerSpace(int slots)
            {
                if (m_itemContainer == null)
                    m_itemContainer = new ItemContainer(slots);
                else
                    m_itemContainer.AddSlots(slots);
            }
            #endregion

            #region Getters
            public override GraphicsBase GetGraphics()
            {
                return m_graphics;
            }
            public ItemContainer itemContainer
            {
                get
                {
                    return m_itemContainer;
                }
            }
            public Light light
            {
                get
                {
                    return m_light;
                }
            }
            #endregion

            #region Status icons methods
            public void AddStatusIcon(string statusIdentity, Sprite icon)
            {
                GameObject icObj = new GameObject(statusIdentity);
                icObj.transform.SetParent(m_localCanvas.transform, false);
                icObj.AddComponent<Image>().sprite = icon;

                Outline outline = icObj.AddComponent<Outline>();
                outline.effectDistance = new Vector2(0.1f, 0.1f);
                outline.effectColor = Color.black;

                RectTransform transform = icObj.GetComponent<RectTransform>();
                transform.sizeDelta = new Vector2(1, 1);
                transform.pivot = Vector2.zero;
                transform.anchorMin = Vector2.zero;
                transform.anchorMax = Vector2.zero;
                transform.localScale = new Vector3(STATUS_ICON_SCALE, STATUS_ICON_SCALE);

                m_statusIcons.Add(statusIdentity, icObj);
                RepositionStatusIcons();
            }

            public void RemoveStatusIcon(string alertIdentity)
            {
                if (m_statusIcons.ContainsKey(alertIdentity))
                {
                    m_statusIcons[alertIdentity].AddComponent<GameObjectDestroyer>().Destroy();
                    m_statusIcons.Remove(alertIdentity);
                    RepositionStatusIcons();
                }
            }

            private void RepositionStatusIcons()
            {
                int i = 0;
                foreach (var icon in m_statusIcons)
                {
                    icon.Value.GetComponent<RectTransform>().anchoredPosition = new Vector2(i * STATUS_ICON_SCALE, 0f);
                    i++;
                }
            }

            public bool HasStatusIcon(string statusIdentity)
            {
                return m_statusIcons.ContainsKey(statusIdentity);
            }
            #endregion
        }
    }
}