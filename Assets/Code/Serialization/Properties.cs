using UnityEngine;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace FinalFrontier
{
    namespace Serialization
    {
        public class Properties
        {
            public static string dataRootPath = Application.dataPath + "/.." + "/data/"; 

            private List<Property<string, object>> _properties;

            public string dataPath;
            public string fileName;

            public Properties(string folder = "unspecified")
            {
                dataPath = Application.dataPath + "/.." + "/data/" + folder;
                _properties = new List<Property<string, object>>();
            }

            /// <summary>
            /// Get A List of all the properties
            /// </summary>
            /// <returns></returns>
            public List<Property<string, object>> GetAll()
            {
                if(_properties == null)
                    _properties = new List<Property<string, object>>();
                return _properties;
            }
            
            /// <summary>
            /// Clears all the properties and copies from given properties list.
            /// </summary>
            /// <param name="properties">properties to overwrite with</param>
            public void SetAll(Properties properties)
            {
                _properties.Clear();
                _properties.AddRange(properties.GetAll());
            }

            public void SetAll(List<Property<string, object>> properties)
            {
                _properties.Clear();
                _properties.AddRange(properties);
            }

            /// <summary>
            /// Copies from given properties list, keeps existing properties
            /// </summary>
            /// <param name="properties">properties to add</param>
            public void AddAll(List<Property<string, object>> properties)
            {
                for (int i = 0; i < _properties.Count; i++)
                {
                    for (int p = 0; p < properties.Count; p++)
                    {
                        if(_properties[i].Key != properties[p].Key)
                        {
                            Set(properties[p].Key, properties[p].Value);
                        }
                    }
                }
            }

            /// <summary>
            /// Get a specific property
            /// </summary>
            /// <typeparam name="T">The type of property</typeparam>
            /// <param name="key">The property name/key</param>
            /// <returns></returns>
            public T Get<T>(string key)
            {

                for (int i = 0; i < _properties.Count; i++)
                {
                    if (_properties[i].Key == key)
                    {
                        return (T)_properties[i].Value;
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
                for (int i = 0; i < _properties.Count; i++)
                {
                    if (_properties[i].Key == key)
                    {
                        _properties[i] = new Property<string, object>(key, value);
                        return;
                    }
                }
                _properties.Add(new Property<string, object>(key, value));
            }

            /// <summary>
            /// Checks if a property is set
            /// </summary>
            /// <param name="key">The property name/key</param>
            /// <returns></returns>
            public bool Has(string key)
            {
                bool result = false;
                for (int i = 0; i < _properties.Count; i++)
                {
                    if (_properties[i].Key == key)
                    {
                        result = true;
                        break;
                    }
                }
                return result;
            }

            /// <summary>
            /// Only sets the property if it doesnt exist yet
            /// </summary>
            /// <param name="key">The property name/key</param>
            /// <param name="value">The value of the property</param>
            public void Secure(string key, object value)
            {
                for (int i = 0; i < _properties.Count; i++)
                {
                    if (_properties[i].Key == key)
                    {
                        return;
                    }
                }
                _properties.Add(new Property<string, object>(key, value));
            }

            /// <summary>
            /// Amount of properties
            /// </summary>
            public int amount
            {
                get
                {
                    return _properties.Count;
                }
            }

            /// <summary>
            /// Save the properties to an XML file
            /// </summary>
            public void Save()
            {
                if(Get<string>("identity") != null)
                {
                    fileName = Get<string>("identity") + ".xml";
                }else if(Get<string>("name") != null)
                {
                    fileName = Get<string>("name") + ".xml";
                }
                else
                {
                    fileName = "unnamed.xml";
                }
                PropertyXMLHandler.Save(this);
            }

            /// <summary>
            /// Load properties from specified file
            /// </summary>
            /// <param name="fileName">file to load properties from, do not include path</param>
            public void Load(string fileName)
            {
                this.fileName = fileName;
                PropertyXMLHandler.Load(this);
            }

            /// <summary>
            /// Prints out all the properties in the Unity debugger
            /// </summary>
            public void LogAll()
            {
                string txt = "";
                for (int i = 0; i < _properties.Count; i++)
                {
                    txt += _properties[i].Key + ": " + _properties[i].Value + "\n";
                }
                Debug.Log(txt);
            }

            public string folder
            {
                set
                {
                    dataPath = Application.dataPath + "/.." + "/data/" + value;
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
