using UnityEngine;

using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

using EndlessExpedition.Managers;

namespace EndlessExpedition
{
    namespace Serialization
    {
        [System.Serializable]
        public class Properties
        {
            public delegate void OnValueChangeEventHandler(int propertyIndex);
            public delegate void OnListChangeEventHandler();
            public OnValueChangeEventHandler OnValueChangeEvent;
            public OnListChangeEventHandler OnListChangeEvent;

            public static string dataRootPath = Application.dataPath + "/.." + "/Data/";
            public static string saveRootPath = Application.dataPath + "/.." + "/Saves/";

            private List<Property<string, object>> m_properties;
            
            private string m_fileName;

            public string dataPath;
            public string folder;

            public Properties(string folder)
            {
                this.folder = folder;
                dataPath = Application.dataPath + "/.." + "/Data/" + folder;
                m_properties = new List<Property<string, object>>();
            }

            /// <summary>
            /// Get A List of all the properties
            /// </summary>
            /// <returns></returns>
            public List<Property<string, object>> GetAll()
            {
                if(m_properties == null)
                    m_properties = new List<Property<string, object>>();
                return m_properties;
            }
            
            /// <summary>
            /// Clears all the properties and copies from given properties list.
            /// </summary>
            /// <param name="properties">properties to overwrite with</param>
            public void SetAll(Properties properties)
            {
                m_properties.Clear();
                m_properties.AddRange(properties.GetAll());

                if (OnListChangeEvent != null)
                    OnListChangeEvent();
            }

            public void SetAll(List<Property<string, object>> properties)
            {
                m_properties.Clear();
                m_properties.AddRange(properties);

                if (OnListChangeEvent != null)
                    OnListChangeEvent();
            }

            /// <summary>
            /// Copies from given properties list, keeps existing properties
            /// </summary>
            /// <param name="properties">properties to add</param>
            public void AddAll(List<Property<string, object>> properties)
            {
                m_properties.AddRange(properties);

                if (OnListChangeEvent != null)
                    OnListChangeEvent();
            }

            /// <summary>
            /// Get a specific property
            /// </summary>
            /// <typeparam name="T">The type of property</typeparam>
            /// <param name="key">The property name/key</param>
            /// <returns></returns>
            public T Get<T>(string key)
            {
                for (int i = 0; i < m_properties.Count; i++)
                {
                    if (m_properties[i].Key == key)
                    {
                        if (typeof(T) != m_properties[i].Value.GetType())
                        {
                            Debug.Log(typeof(T) + " - " + m_properties[i].Value.GetType());
                            Debug.Log(m_properties[i].Key + ": " + m_properties[i].Value);
                        }
                        return (T)m_properties[i].Value;
                    }
                }
                return default(T);
            }

            /// <summary>
            /// Set a property, if it already is set it will be overwritten
            /// Otherwise it will be added
            /// </summary>
            /// <param name="key">The property name/key</param>
            /// <param name="value">The value of the property</param>
            public void Set(string key, object value)
            {
                for (int i = 0; i < m_properties.Count; i++)
                {
                    if (m_properties[i].Key == key)
                    {
                        m_properties[i] = new Property<string, object>(key, value);

                        if (OnValueChangeEvent != null)
                            OnValueChangeEvent(i);
                        
                        return;
                    }
                }
                m_properties.Add(new Property<string, object>(key, value));

                if (OnValueChangeEvent != null)
                    OnValueChangeEvent(m_properties.Count - 1);
            }

            /// <summary>
            /// Checks if a property is set
            /// </summary>
            /// <param name="key">The property name/key</param>
            /// <returns></returns>
            public bool Has(string key)
            {
                bool result = false;
                for (int i = 0; i < m_properties.Count; i++)
                {
                    if (m_properties[i].Key == key)
                    {
                        result = true;
                        break;
                    }
                }
                return result;
            }

            /// <summary>
            /// Find the index of the property and returns it
            /// </summary>
            /// <param name="key">Key/Identity of the property</param>
            /// <returns>The index of the property in the internal list</returns>
            public int Index(string key)
            {
                for (int i = 0; i < m_properties.Count; i++)
                {
                    if (m_properties[i].Key == key)
                    {
                        return i;
                    }
                }
                return -1;
            }

            /// <summary>
            /// Only sets the property if it doesnt exist yet
            /// </summary>
            /// <param name="key">The property name/key</param>
            /// <param name="value">The value of the property</param>
            public void Secure(string key, object value)
            {
                for (int i = 0; i < m_properties.Count; i++)
                {
                    if (m_properties[i].Key == key)
                    {
                        return;
                    }
                }
                m_properties.Add(new Property<string, object>(key, value));

                if (OnValueChangeEvent != null)
                    OnValueChangeEvent(m_properties.Count - 1);
            }

            /// <summary>
            /// Amount of properties
            /// </summary>
            public int amount
            {
                get
                {
                    return m_properties.Count;
                }
            }

            /// <summary>
            /// Prints out all the properties in to the console
            /// </summary>
            public void LogAll()
            {
                string txt = "";
                for (int i = 0; i < m_properties.Count; i++)
                {
                    txt += m_properties[i].Key + ": " + m_properties[i].Value + " (" + m_properties[i].Value.GetType() + ")" + "\n";
                }
                CMD.Log(txt);
            }

            //Serialization
            public void Load()
            {
                PropertyXMLHandler.Load(this);
            }

            public void Load(string _fileName)
            {
                m_fileName = _fileName;
                PropertyXMLHandler.Load(this);
            }

            public void GenerateFileName()
            {
                if (Get<string>("identity") != null)
                {
                    m_fileName = Get<string>("identity") + ".xml";
                }
                else if (Get<string>("name") != null)
                {
                    m_fileName = Get<string>("name") + ".xml";
                }
                else
                {
                    m_fileName = "unnamed.xml";
                }
            }

            public string fileName
            {
                get
                {
                    return m_fileName;
                }
                set
                {
                    m_fileName = value;
                }
            }
        }

        [System.Serializable]
        [XmlType("Property")]
        public struct Property<K, V>
        {
            public Property(K key, V value)
            {
                Key = key;
                Value = value;
            }

            public K Key
            { get; set; }

            public V Value
            { get; set; }
        }
    }
}
