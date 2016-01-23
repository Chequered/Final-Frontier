using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using EndlessExpedition.Managers;
using EndlessExpedition.Graphics;
using EndlessExpedition.Items;
using EndlessExpedition.UI;
using EndlessExpedition.Entities.BuildingModules;

namespace EndlessExpedition
{
    namespace Entities
    {
        public class Building : Entity
        {
            private const float STATUS_ICON_SCALE = 0.24f;

            private BuildingGraphics m_graphics;
            private List<BuildingModule> m_modules;

            //Alert UI
            private GameObject m_localCanvas;
            private Dictionary<string, GameObject> m_statusIcons;

            public override void OnStart()
            {
                base.OnStart();
                p_properties.Secure("identity", "unnamedBuilding");
                p_properties.Secure("type", "building");

                m_graphics = new BuildingGraphics(this);
                m_statusIcons = new Dictionary<string, GameObject>();
                m_modules = new List<BuildingModule>();

                if(properties.Has("produces"))
                {
                    AddModule(new ProductionModule());
                }

                #region Building menus

                ActionMenuList debugMenu = new ActionMenuList();
                debugMenu.buttonSize = new Vector2(100, 30);
                debugMenu.AddButton(new ActionButton("Close", debugMenu.Toggle));
                debugMenu.menuName = properties.Get<string>("displayName") + ": Debug Menu";
                debugMenu.BuildUI();
                debugMenu.Toggle(false);
                ManagerInstance.Get<UIManager>().AddUI(debugMenu);
                p_UIGroup.AddUIElement(debugMenu);

                if (GetModule<ProductionModule>() != null)
                    debugMenu.AddButton(new ActionButton("Toggle Pause", GetModule<ProductionModule>().TogglePause));

                #endregion
                GenerateIconObject();
                GenerateLight();
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
            private void GenerateModules()
            {
                string moduleProperty = properties.Get<string>("buildingModules");
                if (moduleProperty == null)
                    return;

                string[] modules = moduleProperty.Split('/');

                for (int i = 0; i < modules.Length; i++)
                {
                    BuildingModule module = BuildingModule.FindModule(modules[i]);
                    if(module != null)
                    {
                        AddModule(module);
                        Debug.Log(module.identity);
                    }
                }
            }
            public void RemoveModule(BuildingModule module)
            {
                if (m_modules.Contains(module))
                    m_modules.Remove(module);
            }
            public void AddModule(BuildingModule module)
            {
                m_modules.Add(module);
                module.building = this;
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
            #endregion

            #region Getters
            public override GraphicsBase GetGraphics()
            {
                return m_graphics;
            }
            #endregion

            #region Status icons methods
            public void GenerateIconObject()
            {
                m_localCanvas = new GameObject("Local Canvas");
                Canvas canvas = m_localCanvas.AddComponent<Canvas>();
                RectTransform transform = m_localCanvas.GetComponent<RectTransform>();

                canvas.renderMode = RenderMode.WorldSpace;

                transform.position = new Vector3(0, 0);
                transform.sizeDelta = new Vector2(1, 1);
                transform.SetParent(p_gameObject.transform, false);
            }

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
                int m = properties.Get<int>("tileWidth");
                transform.localScale = new Vector3(STATUS_ICON_SCALE * m, STATUS_ICON_SCALE * m);

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