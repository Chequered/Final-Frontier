using System.Collections.Generic;
using System.IO;

using UnityEngine;

using FinalFrontier.UI;
using FinalFrontier.Entities;
using FinalFrontier.Graphics;
using FinalFrontier.Managers.Base;
using FinalFrontier.Items;
using FinalFrontier.Serialization;

namespace FinalFrontier
{
    namespace Managers
    {
        public class ItemManager : ManagerBase
        {

            private List<Item> m_itemCache = new List<Item>();

            //active items
            private Dictionary<Item, int> m_activeCurrencies;


            public override void OnStart()
            {
                //SimulationManager.OnSimulationTickStart += ResetEssentialsNextTick;
                //SimulationManager.OnSimulationTickEnd += PrintCurrentEssentials;
            }

            public override void OnTick()
            {

            }

            public override void OnUpdate()
            {

            }

            public override void OnLoad()
            {
                m_activeCurrencies = new Dictionary<Item, int>();
                LoadItems("essentials");
                LoadItems("currency");
                LoadItems("items");
            }

            public override void OnExit()
            {

            }

            public void LoadItems(string folder)
            {
                string[] folders = Directory.GetDirectories(Properties.dataRootPath + "items/" + folder);

                for (int i = 0; i < folders.Length; i++)
                {
                    string[] split = folders[i].Split('\\');
                    folders[i] = split[split.Length - 1];

                    string itemFolder = Properties.dataRootPath + "items/" + folder + "/" + folders[i];
                    string[] propertyFiles = Directory.GetFiles(itemFolder, "*.xml");

                    Properties p = new Properties("items/" + folder);

                    for (int file = 0; file < propertyFiles.Length; file++)
                    {
                        string[] splitFile = propertyFiles[file].Split('\\');
                        string propFile = splitFile[splitFile.Length - 1];

                        p.Load(folders[i] + "/" + propFile);
                    }

                    Item item = new Item(p);
                    ItemGraphics graphics = new ItemGraphics();
                    graphics.LoadFrom(folder + "/" + folders[i] + "/" + folders[i], p);
                    item.SetGraphics(graphics);

                    m_itemCache.Add(item);
                }
            }

            public Item FindItem(string identity)
            {
                for (int i = 0; i < m_itemCache.Count; i++)
                {
                    if (m_itemCache[i].identity == identity)
                        return m_itemCache[i];
                }
                return null;
            }

            public int GetCurrencyAmount(Item currency)
            {
                foreach (var c in m_activeCurrencies)
                {
                    if (c.Key == currency)
                        return c.Value;
                }
                return 0;
            }

            public bool TakeCurrency(Item currency, int amount)
            {
                if(m_activeCurrencies.ContainsKey(currency))
                {
                    m_activeCurrencies[currency] -= amount;
                    return true;
                }
                return false;
            }

            public int AddCurrency(Item currency, int amount)
            {
                if (m_activeCurrencies.ContainsKey(currency))
                    m_activeCurrencies[currency] += amount;
                else
                    m_activeCurrencies.Add(currency, amount);

                return m_activeCurrencies[currency];
            }

            public Dictionary<Item, int> GetAllCurrencies
            {
                get
                {
                    return m_activeCurrencies;
                }
            }
        }
    }
}
