using System.Collections.Generic;

using UnityEngine;

using EndlessExpedition.UI;
using EndlessExpedition.Entities;
using EndlessExpedition.Managers.Base;
using EndlessExpedition.Items;
using EndlessExpedition.Entities.BuildingModules;

namespace EndlessExpedition
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
            private const int TIME_MAX_SECONDS = 60;
            private const int TIME_MAX_MINUTES = 24;

            public delegate void SimulationTickStateEventHandler();
            public SimulationTickStateEventHandler OnSimulationBegin;
            public SimulationTickStateEventHandler OnSimulationEnd;

            private const float SIMULATION_DELTATIME_PER_TICK = 5f;
            private float m_deltaTimeLeftToNextTick = SIMULATION_DELTATIME_PER_TICK;

            private List<ProductionModule> m_registeredProductionModules;

            private Dictionary<Item, int> m_predictedEssentialsThisThick;
            private Dictionary<Item, int> m_availableEssentialsThisTick;

            private int m_simulationTicks;

            private GameObject m_dirLight;
            private float m_lastTimeCheck;
            private int m_timeSeconds;
            private int m_timeMinutes;

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

                if (Time.time >= m_lastTimeCheck + 0.85f)
                {
                    m_lastTimeCheck = Time.time;
                    m_timeSeconds++;
                    m_dirLight.transform.localEulerAngles = new Vector3(0, ((360f / (24f * 60f)) * (m_timeMinutes * 60 + m_timeSeconds)) + 180f, 0);
                    if (m_timeSeconds >= TIME_MAX_SECONDS)
                    {
                        m_timeSeconds = 0;
                        m_timeMinutes++;
                    }

                    if (m_timeMinutes >= TIME_MAX_MINUTES)
                    {
                        m_timeMinutes = 0;
                    }
                }
            }

            public override void OnLoad()
            {
                m_registeredProductionModules = new List<ProductionModule>();
                m_predictedEssentialsThisThick = new Dictionary<Item, int>();
                m_availableEssentialsThisTick = new Dictionary<Item, int>();

                m_dirLight = GameObject.FindGameObjectWithTag("Sun");
                m_dirLight.transform.localEulerAngles = new Vector3(0, ((360f / (24f * 60f)) * (m_timeMinutes * m_timeSeconds)) + 180f, 0);
                m_timeSeconds = 0;
                m_timeMinutes = 10;
            }

            public override void OnExit()
            {
                throw new System.NotImplementedException();
            }

            [ConsoleCommand("Forces a simulation tick to be processed")]
            public static void CMDSimulationTick()
            {
                ManagerInstance.Get<SimulationManager>().SimulationTick();
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
                                    m_registeredProductionModules[m].ItemContainer.TakeItems(requirement.Key, requirement.Value);
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
                                if (allowedModules[m].Building.itemContainer != null)
                                {
                                    MasterItemStack masterStack = new MasterItemStack(product.Key, product.Value);
                                    allowedModules[m].Building.itemContainer.AddFromProduction(masterStack);
                                }
                                else
                                    Debug.LogError("This Building Needs an ItemContainer!: " + allowedModules[m].Building);
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

            //Time
            public void AdvanceOneHour()
            {
                SetTime(m_timeMinutes + 1, m_timeSeconds);
            }
            public void SetTime(int hours, int minutes)
            {
                if (hours > 24)
                    hours = 24;
                if (hours < 0)
                    hours = 0;
                if (minutes > 59)
                    minutes = 59;
                if (minutes < 0)
                    minutes = 0;

                m_timeMinutes = hours;
                m_timeSeconds = minutes;
            }
            public bool isNightTime
            {
                get
                {
                    if (currentHour > 17 || currentHour < 7)
                        return true;
                    return false;
                }
            }
            public int maxTime
            {
                get
                {
                    return 24 * 60;
                }
            }
            public int currentTime
            {
                get
                {
                    return m_timeMinutes * 60 + m_timeSeconds;
                }
            }
            public int currentHour
            {
                get
                {
                    return m_timeMinutes;
                }
            }
            public string timeAsString
            {
                get
                {
                    string h = "" + m_timeMinutes;
                    string m = "" + m_timeSeconds;

                    if (m_timeMinutes < 10)
                        h = "0" + m_timeMinutes;
                    if (m_timeSeconds < 10)
                        m = "0" + m_timeSeconds;

                    return h + ":" + m;
                }
            }
        }
    }
}
