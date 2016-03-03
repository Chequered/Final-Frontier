using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using EndlessExpedition.Managers;
using EndlessExpedition.Graphics;
using EndlessExpedition.Entities.Construction;
using EndlessExpedition.UI;
using EndlessExpedition.Entities.BuildingModules;

namespace EndlessExpedition
{
    namespace Entities
    {
        public class Building : Entity
        {
            private const float STATUS_ICON_SCALE = 0.16f;

            private BuildingGraphics m_graphics;
            private ConstructionHeader m_constructionHeader;
            private List<BuildingModule> m_modules;

            private bool m_isBuilt;

            //Alert UI
            private GameObject m_localCanvas;
            private Dictionary<string, GameObject> m_statusIcons;

            //Light vars
            private float m_maxLightStrength;

            public override void OnStart()
            {
                base.OnStart();
                p_properties.Secure("identity", "unnamedBuilding");
                p_properties.Secure("type", "building");

                m_graphics = new BuildingGraphics(this);
                m_statusIcons = new Dictionary<string, GameObject>();
                m_modules = new List<BuildingModule>();

                GenerateIconObject();
                GenerateLight();
                GenerateModules();
                
                Color c = gameObject.GetComponent<Renderer>().material.color;
                c.a = 0.25f;
                gameObject.GetComponent<Renderer>().material.color = c;

                if (properties.Has("produces"))
                {
                    ProductionModule PM = new ProductionModule();
                    AddModule(PM);
                    PM.TogglePause(false);
                }

                if (properties.Has("buildMode"))
                    if (properties.Get<string>("buildMode") == "construction")
                        m_constructionHeader = new ConstructionHeader(this);

                #region Building menus

                ActionMenuList debugMenu = new ActionMenuList();
                debugMenu.buttonSize = new Vector2(100, 30);
                debugMenu.AddButton(new ActionButton("Close", debugMenu.Toggle));
                debugMenu.menuName = properties.Get<string>("displayName") + ": Debug Menu";
                debugMenu.BuildUI();
                debugMenu.Toggle(false);
                ManagerInstance.Get<UIManager>().AddUI(debugMenu);
                p_UIGroup.AddUIElement(debugMenu);

                #endregion
            }

            public override void OnTick()
            {
                base.OnTick();
            }

            public override void OnUpdate()
            {
                base.OnUpdate();
                
                if (!m_isBuilt)
                    return;

                UpdateLights();
            }

            #region UpdateMethods
            private void UpdateLights()
            {
                int hours = ManagerInstance.Get<SimulationManager>().currentHour;
                if (hours > 7 && hours < 17)
                    light.enabled = false;
                else if (hours >= 17)
                    light.enabled = true;
            }
            #endregion

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

            public void OnBuild()
            {
                m_isBuilt = true;
                
                Color c = gameObject.GetComponent<Renderer>().material.color;
                c.a = 1f;
                gameObject.GetComponent<Renderer>().material.color = c;

                if (GetModule<ProductionModule>() != null)
                    p_UIGroup.GetElement<ActionMenuList>().AddButton(new ActionButton("Toggle Pause", GetModule<ProductionModule>().TogglePause));

                if (GameManager.gameState == GameState.LoadingSave || GameManager.gameState == GameState.StartingSave)
                    return;

                #region Spawn children
                if (properties.Has("spawnOnBuild"))
                {
                    string[] split = properties.Get<string>("spawnOnBuild").Split('/');
                    for (int i = 0; i < split.Length; i++)
                    {
                        string type = split[i].Split(':')[0];
                        string id = split[i].Split(':')[1];

                        Entity prefab = null;
                        Entity entity = null;

                        switch (type)
                        {
                            case "Actor":
                                prefab = ManagerInstance.Get<EntityManager>().FindFromCache<Actor>(id);
                                if (prefab != null)
                                {
                                    entity = ManagerInstance.Get<EntityManager>().CreateEntity<Actor>(prefab, tilePosition.x, tilePosition.y);
                                }
                                break;
                            case "Prop":
                                prefab = ManagerInstance.Get<EntityManager>().FindFromCache<Prop>(id);
                                if (prefab != null)
                                {
                                    entity = ManagerInstance.Get<EntityManager>().CreateEntity<Prop>(prefab, tilePosition.x, tilePosition.y);
                                }
                                break;
                            case "Building":
                                prefab = ManagerInstance.Get<EntityManager>().FindFromCache<Building>(id);
                                if (prefab != null)
                                {
                                    entity = ManagerInstance.Get<EntityManager>().CreateEntity<Building>(prefab, tilePosition.x, tilePosition.y);
                                }
                                break;
                            default:
                                break;
                        }
                        if (entity != null)
                        {
                            p_spawnedEntities.Add(entity);
                        }
                    }
                }
                #endregion
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
                module.Building = this;
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
            public bool IsBuilt
            {
                get
                {
                    return m_isBuilt;
                }
            }
            public Canvas LocalCanvas
            {
                get
                {
                    return m_localCanvas.GetComponent<Canvas>();
                }
            }
            public ConstructionHeader ConstructionHeader
            {
                get
                {
                    return m_constructionHeader;
                }
            }
            #endregion

            #region Status icons methods
            public void GenerateIconObject()
            {
                m_localCanvas = new GameObject("Local Canvas");

                Canvas canvas = m_localCanvas.AddComponent<Canvas>();
                RectTransform transform = m_localCanvas.GetComponent<RectTransform>();

                canvas.renderMode = RenderMode.WorldSpace;
                canvas.sortingOrder = 1;

                transform.localPosition = new Vector3(0, 0, -0.05f);
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