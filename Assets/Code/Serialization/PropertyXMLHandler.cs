using UnityEngine;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Text;

namespace FinalFrontier
{
    namespace Serialization
    {
        public static class PropertyXMLHandler
        {
            public static void Save(Properties properties)
            {
                if (!Directory.Exists(properties.dataPath))
                    Directory.CreateDirectory(properties.dataPath);

                XmlSerializer serializer = new XmlSerializer(typeof(List<Property<string, object>>));
                using (StreamWriter stream = new StreamWriter(properties.dataPath + "/" + properties.fileName, false, Encoding.GetEncoding("UTF-8")))
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
                        properties.SetAll(serializer.Deserialize(stream) as List<Property<string, object>>);
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
