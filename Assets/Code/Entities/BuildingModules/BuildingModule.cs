using System.Collections.Generic;
using System.Reflection;
using System;

using UnityEngine;

using EndlessExpedition.Items;
using EndlessExpedition.Serialization;
using EndlessExpedition.Managers;
using EndlessExpedition.Graphics;
using EndlessExpedition.Entities.BehvaiourScripts;

namespace EndlessExpedition
{
    namespace Entities.BuildingModules
    {
        public abstract class BuildingModule : IComparable
        {
            private static List<Type> m_moduleTypes;
            private bool m_paused;

            public static void SearchModules()
            {
                m_moduleTypes = Extensions.GetListOfType<BuildingModule>();
            }
            public static BuildingModule FindModule(string moduleName)
            {
                BuildingModule result = null;

                Type t = null;
                for (int i = 0; i < m_moduleTypes.Count; i++)
                {
                    if (moduleName == m_moduleTypes[i].Name.ToString())
                    {
                        t = m_moduleTypes[i];
                        break;
                    }
                }

                if(t == null)
                {
                    return null;
                    throw new Exception("Building module not found");
                }

                result = Activator.CreateInstance(t) as BuildingModule;

                return result;
            }

            protected Building p_building;
            public abstract void OnStart();
            public abstract void OnTick();
            public abstract void OnUpdate();
            public abstract void OnModuleRemove();

            public virtual void TogglePause()
            {
                m_paused = !m_paused;
            }
            public virtual void TogglePause(bool state)
            {
                m_paused = state;
            }
            public int CompareTo(object obj)
            {
                if (obj == null)
                    return 1;

                BuildingModule BM = obj as BuildingModule;
                return BM.Identity.CompareTo(this.Identity);
            }

            public abstract string Identity
            {
                get;
            }
            public Building Building
            {
                get
                {
                    return p_building;
                }
                set
                {
                    p_building = value;
                }
            }
            public bool Paused
            {
                get
                {
                    return m_paused;
                }
            }
        }

        public class ProductionModule : BuildingModule
        {
            private List<KeyValuePair<Item, int>> m_productionResult;
            private Dictionary<Item, int> m_productionRequirements;

            private int m_currentProductionIndex = 0;
            private bool m_productionPaused = false;

            public ProductionModule()
            {
                m_productionResult = new List<KeyValuePair<Item, int>>();
                m_productionRequirements = new Dictionary<Item, int>();
            }

            public override void OnStart()
            {
                Properties p = Building.properties;
                if(p.Has("produces"))
                {
                    string[] produces = p.Get<string>("produces").Split('/');
                    for (int i = 0; i < produces.Length; i++)
                    {
                        string[] split = produces[i].Split(':');
                        if(split[0] != "null")
                        {
                            try
                            {
                                m_productionResult.Add(new KeyValuePair<Item, int>(ManagerInstance.Get<ItemManager>().FindItem(split[0]), System.Convert.ToInt32(split[1])));
                            }
                            catch (InvalidCastException e)
                            {
                                CMD.Error(e);
                            }
                        }
                    }
                }
                else
                {
                    OnModuleRemove();
                }
                if(p.Has("productionRequiredItems"))
                {
                    string[] requiredItems = p.Get<string>("productionRequiredItems").Split('/');
                    for (int i = 0; i < requiredItems.Length; i++)
                    {
                        string[] split = requiredItems[i].Split(':');
                        try
                        {
                            m_productionRequirements.Add(ManagerInstance.Get<ItemManager>().FindItem(split[0]), System.Convert.ToInt32(split[1]));
                        }catch(InvalidCastException e)
                        {
                            Debug.LogError(e);
                        }
                    }
                }
                else
                {
                    OnModuleRemove();
                }

                if (m_productionResult.Count >= 1)
                    ManagerInstance.Get<SimulationManager>().RegisterProductionModule(this);
            }

            public override void OnTick()
            {

            }

            public override void OnUpdate()
            {

            }

            public bool ValidateProduction()
            {
                bool result = true;
                List<KeyValuePair<Item, int>> missing = new List<KeyValuePair<Item, int>>(); 
                foreach (var requirement in m_productionRequirements)
                {
                    switch (requirement.Key.type)
                    {
                        case ItemType.Item:
                            if (!p_building.itemContainer.ContainsItems(requirement.Key, requirement.Value))
                            {
                                result = false;
                                missing.Add(requirement);
                                break;
                            }
                            break;
                        case ItemType.Currency:
                            if(ManagerInstance.Get<ItemManager>().GetCurrencyAmount(requirement.Key) < requirement.Value)
                            {
                                result = false;
                                missing.Add(requirement);
                            }
                            break;
                        case ItemType.Essential:
                            if (ManagerInstance.Get<SimulationManager>().PredictedEssentialsLeftThisTick(requirement.Key) < requirement.Value)
                            {
                                result = false;
                                missing.Add(requirement);
                            }
                            break;
                        default:
                            break;
                    }
                }
                if (!result)
                {
                    //production failed
                    Building.light.enabled = false;
                    for (int i = 0; i < missing.Count; i++)
                    {
                        string identity = "missingRequirement:" + missing[i].Key.identity;
                        if(!Building.HasStatusIcon(identity))
                        {
                            Building.AddStatusIcon(identity, (missing[i].Key.GetGraphics() as ItemGraphics).GetIcon(ItemIconType.Deficit));
                        }
                    }
                }
                else
                {
                    if(ManagerInstance.Get<SimulationManager>().isNightTime)
                        Building.light.enabled = true;
                    //production succes
                    foreach (var requirement in m_productionRequirements)
                    {
                        string identity = "missingRequirement:" + requirement.Key.identity;
                        if(Building.HasStatusIcon(identity))
                        {
                            Building.RemoveStatusIcon(identity);
                        }
                    }
                }

                return result;
            }

            public override void OnModuleRemove()
            {
                //TODO: Tell sim manager
                Building.RemoveModule(this);
            }

            public KeyValuePair<Item, int>[] ProduceProducts()
            {
                List<KeyValuePair<Item, int>> result = new List<KeyValuePair<Item,int>>();

                for (int i = 0; i < m_productionResult.Count; i++)
                {
                    result.Add(m_productionResult[i]);
                }

                return result.ToArray();
            }

            public KeyValuePair<Item, int>[] predictedEssentials
            {
                get
                {
                    List<KeyValuePair<Item, int>> result = new List<KeyValuePair<Item, int>>();
                    for (int i = 0; i < m_productionResult.Count; i++)
                    {
                        if (m_productionResult[i].Key.type == ItemType.Essential)
                            result.Add(m_productionResult[i]);
                    }
                    return result.ToArray();
                }
            }

            public Dictionary<Item, int> requiredForProduction
            {
                get
                {
                    return m_productionRequirements;
                }
            }

            public ItemContainer ItemContainer
            {
                get
                {
                    return p_building.itemContainer;
                }
            }

            public override void TogglePause()
            {
                base.TogglePause();
                m_productionPaused = !m_productionPaused;
                if (m_productionPaused)
                {
                    ManagerInstance.Get<SimulationManager>().UnregisterProductionModule(this);
                    Building.AddStatusIcon("productionPaused", Resources.Load<Sprite>("UI/icon_pause"));
                }
                else
                {
                    ManagerInstance.Get<SimulationManager>().RegisterProductionModule(this);
                    Building.RemoveStatusIcon("productionPaused");
                }
            }
            public override void TogglePause(bool state)
            {
                base.TogglePause(state);
                m_productionPaused = state;
                if (m_productionPaused)
                {
                    ManagerInstance.Get<SimulationManager>().UnregisterProductionModule(this);
                    Building.AddStatusIcon("productionPaused", Resources.Load<Sprite>("UI/icon_pause"));
                }
                else
                {
                    ManagerInstance.Get<SimulationManager>().RegisterProductionModule(this);
                    Building.RemoveStatusIcon("productionPaused");
                }
            }

            public override string Identity
            {
                get { return "productionModule"; }
            }
        }
    }
}