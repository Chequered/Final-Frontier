using UnityEngine;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Text;

namespace EndlessExpedition
{
    namespace Serialization
    {
        public static class PropertyXMLHandler
        {
            public static void Save(Properties properties, string savePath)
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<Property<string, object>>));
                string finalPath = savePath + "/" + properties.folder + "/"  + properties.fileName;

                using (StreamWriter stream = new StreamWriter(finalPath))
                {
                    try
                    {
                        serializer.Serialize(stream, properties.GetAll());
                    }catch(XmlException e)
                    {
                        Debug.LogError(e);
                    }
                }
            }

            public static void Load(Properties properties)
            {
                string path = properties.dataPath;
                string fileName = properties.fileName;
                XmlSerializer serializer = new XmlSerializer(typeof(List<Property<string, object>>));
                
                using (FileStream stream = new FileStream(path + "/" + fileName, FileMode.Open))
                {
                    try
                    {
                        properties.AddAll(serializer.Deserialize(stream) as List<Property<string, object>>);
                    }
                    catch (XmlException e)
                    {
                        Debug.LogError(e);
                    }
                }
            }
        }
    }
}
