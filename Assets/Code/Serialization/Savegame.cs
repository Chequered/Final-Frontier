using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using UnityEngine;

using EndlessExpedition.Managers;

namespace EndlessExpedition
{
    namespace Serialization
    {
        public class Savegame
        {
            public const string SAVEGAME_TERRAIN = "terrain.savegame";
            public const string SAVEGAME_WORLD = "world.savegame";
            public const string SAVEGAME_ENTITIES = "entities.savegame";

            private Properties m_worldProperties;
            private List<Properties> m_terrainProperties;
            private List<Properties> m_entityProperties;

            public string savePath
            {
                get
                {
                    return Properties.saveRootPath + string.Format("save-{0:yyyy-MM-dd_hh-mm-ss-tt}", DateTime.Now) + "/";
                }
            }

            public void Save(string name = null)
            {
                string path = savePath;
                if (name != null)
                    path = Properties.saveRootPath + name + "/";

                List<Properties> properties = new List<Properties>();
                properties.AddRange(ManagerInstance.Get<TerrainManager>().terrainTilesProperties);

                float realTime = Time.realtimeSinceStartup;
                BinaryFormatter bf = new BinaryFormatter();

                Directory.CreateDirectory(path);

                try
                {
                    {
                        //world
                        FileStream file = File.Create(path + SAVEGAME_WORLD);

                        bf.Serialize(file, ManagerInstance.Get<TerrainManager>().worldProperties);
                        file.Close();
                    }
                    {
                        //terrain
                        FileStream file = File.Create(path + SAVEGAME_TERRAIN);

                        bf.Serialize(file, properties);
                        file.Close();
                    }
                    {
                        //entities
                        FileStream file = File.Create(path + SAVEGAME_ENTITIES);

                        bf.Serialize(file, ManagerInstance.Get<EntityManager>().allEntityProperties);
                        file.Close();
                    }
                }
                catch(IOException error)
                {
                    Debug.Log(error);
                }

                Debug.Log("Saved " + path + " in " + ((Time.realtimeSinceStartup - realTime) * 1000) + " ms");
            }

            public void LoadSaveFolder(string folder)
            {
                try
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    {
                        //world
                        FileStream file = File.Open(Properties.saveRootPath + folder + "/" + SAVEGAME_WORLD, FileMode.Open);

                        m_worldProperties = bf.Deserialize(file) as Properties;
                        file.Close();
                    }
                    {
                        //terrain
                        FileStream file = File.Open(Properties.saveRootPath + folder + "/" + SAVEGAME_TERRAIN, FileMode.Open);

                        m_terrainProperties = bf.Deserialize(file) as List<Properties>;
                        file.Close();
                    }
                    {
                        //entities
                        FileStream file = File.Open(Properties.saveRootPath + folder + "/" + SAVEGAME_ENTITIES, FileMode.Open);

                        m_entityProperties = bf.Deserialize(file) as List<Properties>;

                        //Debug.LogWarning(m_entityProperties.Count);

                        file.Close();
                    }

                }
                catch (IOException error)
                {
                    Debug.LogError(error);
                }
            }

            public Properties worldProperties
            {
                get
                {
                    return m_worldProperties;
                }
            }

            public List<Properties> terrainProperties
            {
                get
                {
                    return m_terrainProperties;
                }
            }

            public List<Properties> entityProperties
            {
                get
                {
                    return m_entityProperties;
                }
            }
        }
    }
}
