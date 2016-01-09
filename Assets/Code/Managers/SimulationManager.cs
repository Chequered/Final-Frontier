using System.Collections.Generic;

using UnityEngine;

using FinalFrontier.UI;
using FinalFrontier.Entities;
using FinalFrontier.Managers.Base;
using FinalFrontier.Items;
using FinalFrontier.Entities.BuildingModules;

namespace FinalFrontier
{
    namespace Managers
    {
        public enum ProductionType
        {
            Item,
            Essential,
            Currency,
            Flora
        }

        public class SimulationManager : ManagerBase
        {
            public delegate void SimulationTickStateEventHandler();
            public SimulationTickStateEventHandler OnSimulationBegin;
            public SimulationTickStateEventHandler OnSimulationEnd;

            private const float SIMULATION_DELTATIME_PER_TICK = 5f;
            private float m_deltaTimeLeftToNextTick = SIMULATION_DELTATIME_PER_TICK;

            private List<ProductionModule> m_registeredProductionModules;

            private Dictionary<Item, int> m_predictedEssentialsThisThick;
            private Dictionary<Item, int> m_availableEssentialsThisTick;

            private int m_simulationTicks;

            public override void OnStart()
            {

            }

            public override void OnTick()
            {

            }

            public override void OnUpdate()
            {
                m_deltaTimeLeftToNextTick -= Time.deltaTime;

                //predict the produces eseentials this tick.
                if(m_deltaTimeLeftToNextTick <= 0)
                {
                    SimulationTick();
                    m_deltaTimeLeftToNextTick = SIMULATION_DELTATIME_PER_TICK;
                }
            }

            public override void OnLoad()
            {
                m_registeredProductionModules = new List<ProductionModule>();
                m_predictedEssentialsThisThick = new Dictionary<Item, int>();
                m_availableEssentialsThisTick = new Dictionary<Item, int>();
            }

            public override void OnExit()
            {
                throw new System.NotImplementedException();
            }

            public void SimulationTick()
            {
                if (OnSimulationBegin != null)
                    OnSimulationBegin();

                //Reset our predicted and avaiable essentials
                m_predictedEssentialsThisThick.Clear();
                m_availableEssentialsThisTick.Clear();

                //predict the essential production this tick
                for (int m = 0; m < m_registeredProductionModules.Count; m++)
                {
                    KeyValuePair<Item, int>[] predictions = m_registeredProductionModules[m].predictedEssentials;
                    for (int i = 0; i < predictions.Length; i++)
                    {
                        KeyValuePair<Item, int> prediction = predictions[i];
                        if (m_predictedEssentialsThisThick.ContainsKey(prediction.Key))
                            m_predictedEssentialsThisThick[prediction.Key] += prediction.Value;
                        else
                            m_predictedEssentialsThisThick.Add(prediction.Key, prediction.Value);
                    }
                }

                //we now have our predicted essentials this tick
                //Let's see if we have all we need
                List<ProductionModule> allowedModules = new List<ProductionModule>();
                List<ProductionModule> disallowedModules = new List<ProductionModule>();

                for (int m = 0; m < m_registeredProductionModules.Count; m++)
                {
                    if (m_registeredProductionModules[m].ValidateProduction())
                    {
                        //we can produce, lets consume the items required for production
                        allowedModules.Add(m_registeredProductionModules[m]);
                        foreach (var requirement in m_registeredProductionModules[m].requiredForProduction)
                        {
                            switch (requirement.Key.type)
                            {
                                case ItemType.Item:
                                    m_registeredProductionModules[m].itemContainer.TakeItems(requirement.Key, requirement.Value);
                                    break;
                                case ItemType.Currency:
                                    ManagerInstance.Get<ItemManager>().TakeCurrency(requirement.Key, requirement.Value);
                                    break;
                                case ItemType.Essential:
                                    m_predictedEssentialsThisThick[requirement.Key] -= requirement.Value;
                                    break;
                            }
                        }
                    }
                    else
                        disallowedModules.Add(m_registeredProductionModules[m]);
                }

                //we now have a list of buildigns that can produce and those who can't
                //TODO: display warning for those who can't
                //Let's produce!
                for (int m = 0; m < allowedModules.Count; m++)
                {
                    KeyValuePair<Item, int>[] products = allowedModules[m].ProduceProducts();
                    foreach (var product in products)
                    {
                        switch (product.Key.type)
                        {
                            case ItemType.Item:
                                if (allowedModules[m].building.itemContainer != null)
                                {
                                    MasterItemStack masterStack = new MasterItemStack(product.Key, product.Value);
                                    allowedModules[m].building.itemContainer.AddFromProduction(masterStack);
                                }
                                else
                                    Debug.LogError("This Building Needs an ItemContainer!: " + allowedModules[m].building);
                                break;
                            case ItemType.Currency:
                                ManagerInstance.Get<ItemManager>().AddCurrency(product.Key, product.Value);
                                break;
                            case ItemType.Essential:
                                if (m_availableEssentialsThisTick.ContainsKey(product.Key))
                                    m_availableEssentialsThisTick[product.Key] += product.Value;
                                else
                                    m_availableEssentialsThisTick.Add(product.Key, product.Value);
                                break;
                        }
                    }
                }
                m_simulationTicks++;

                if (OnSimulationEnd != null)
                    OnSimulationEnd();
            }

            public void RegisterProductionModule(ProductionModule module)
            {
                m_registeredProductionModules.Add(module);
            }

            public void UnregisterProductionModule(ProductionModule module)
            {
                if(m_registeredProductionModules.Contains(module))
                    m_registeredProductionModules.Remove(module);
            }

            public int PredictedEssentialsLeftThisTick(Item essential)
            {
                foreach (var pre in m_predictedEssentialsThisThick)
                {
                    if (pre.Key == essential)
                        return pre.Value;
                }
                return 0;
            }

            public int AvaiableEssentialsLeftThisTick(Item essential)
            {
                foreach (var ava in m_availableEssentialsThisTick)
                {
                    if (ava.Key == essential)
                        return ava.Value;
                }
                return 0;
            }

            public MasterItemStack[] avaiableEssentialsLeftThisTick
            {
                get
                {
                    MasterItemStack[] result = new MasterItemStack[m_availableEssentialsThisTick.Count];
                    int i = 0;
                    foreach (var essential in m_availableEssentialsThisTick)
                    {
                        result[i] = new MasterItemStack(essential.Key, essential.Value);
                        i++;
                    }
                    return result;
                }
            }

            public float timeLeftUntilNextTick
            {
                get
                {
                    return m_deltaTimeLeftToNextTick;
                }
            }

            public int simulationTicksElapsed
            {
                get
                {
                    return m_simulationTicks;
                }
            }

            private void LogAllEssentials()
            {
                Debug.Log("+---------------+ essentials this tick +---------------+ ");
                foreach (var essential in m_availableEssentialsThisTick)
                {
                    Debug.Log(essential.Key.displayName + ": " + essential.Value);
                }
                foreach (var currency in ManagerInstance.Get<ItemManager>().GetAllCurrencies)
                {
                    Debug.Log(currency.Key.displayName + ": " + currency.Value);
                }
                Debug.Log("+-------------------------------------------------------+ ");
            }
        }
    }
}
