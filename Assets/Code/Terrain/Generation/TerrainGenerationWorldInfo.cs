using System;

using EndlessExpedition.Serialization;
using EndlessExpedition.Managers;

namespace EndlessExpedition
{
    namespace Terrain.Generation
    {
        public class TerrainGenerationWorldInfo
        {
            //Constants
                //SEED
            public const float MIN_SEED_VALUE = -20000f;
            public const float MAX_SEED_VALUE = 20000f;
                //TEMPERATURE
            public const float MIN_TEMP_VALUE = -55f;
            public const float MAX_TEMP_VALUE = 55f;
                //OXYGEN
            public const float MIN_OXYGEN_VALUE = 0f;
            public const float MAX_OXYGEN_VALUE = 1f;
                //SCALE
                    //TEMP
            public const float TEMP_SEED_SCALE_POSITIVE = MAX_TEMP_VALUE / MAX_SEED_VALUE;
            public const float TEMP_SEED_SCALE_NEGATIVE = MIN_TEMP_VALUE / MIN_SEED_VALUE;
                    //OXYGEN
            public const float OXYGEN_SEED_SCALE_POSITIVE = MAX_OXYGEN_VALUE / MAX_SEED_VALUE;
            public const float OXYGEN_SEED_SCALE_NEGATIVE = MIN_OXYGEN_VALUE / MIN_SEED_VALUE;
                //MISC
            public const int MAX_DECIMAL = 2;

            //Active
            private Properties m_properties;

            public TerrainGenerationWorldInfo()
            {
                m_properties = new Properties("world");

                //Secure default properties
                m_properties.Secure("identity", "worldInfo");
                m_properties.Secure("seed", 0f);
                m_properties.Secure("name", "Unamed Planet");
                m_properties.Secure("type", "Dust");
                m_properties.Secure("x", 0);
                m_properties.Secure("y", 0);
                m_properties.Secure("temperature", 12f); //celcius
                m_properties.Secure("oxygenLevel", 0.5f);
                m_properties.Secure("containsFlora", true);
            }

            public void GenerateTerrainProperties(float seed)
            {
                //trim the seed
                if (seed < MIN_SEED_VALUE) seed = MIN_SEED_VALUE;
                if (seed > MAX_SEED_VALUE) seed = MAX_SEED_VALUE;
                m_properties.Set("seed", seed);
                                
                //Name
                //TODO: get name from perlinSample
                m_properties.Set("name", randomName + " - " + Math.Round(seed));

                //Type
                string perTxt = seed + "";
                int lastDigit = (int)(perTxt[perTxt.Length - 1]);
                //TypeFromInt(lastDigit)

                m_properties.Set("type", "Dust");

                /*if (r < 4)
                    _properties.Set("type", "Dust");
                else if (r < 7)
                    _properties.Set("type", "Moon");
                else
                    _properties.Set("type", "Flora");*/

                //Position
                //TODO: make it so x & y are not always the same
                int root = (int)Math.Sqrt(Math.Round(seed));
                m_properties.Set("x", root);
                m_properties.Set("y", root);

                //Temperature
                if (seed < 0f) m_properties.Set("temperature", seed * TEMP_SEED_SCALE_NEGATIVE);
                if (seed > 0f) m_properties.Set("temperature", seed * TEMP_SEED_SCALE_POSITIVE);

                //Oxygen
                if (seed < 0f) m_properties.Set("oxygenLevel", seed * OXYGEN_SEED_SCALE_NEGATIVE);
                if (seed > 0f) m_properties.Set("oxygenLevel", seed * OXYGEN_SEED_SCALE_POSITIVE);

                //Flora
                if (lastDigit < 5) m_properties.Set("containsFlora", false);
            }
            
            //Property getters
            private string TypeFromInt(int num)
            {
                if(num < 3)
                    return "Earth";
                if (num < 6)
                    return "Moon";
                if (num < 10)
                    return "Dust";
                return "Dust";
            }

            private string randomName
            {
                get
                {
                    string[] names = { "Europa", "Jupiter", "Erasmus", "Rock"};
                    int r = UnityEngine.Random.Range(0, names.Length);
                    return names[r];
                }
            }

            public Properties properties
            {
                get
                {
                    return m_properties;
                }
                set
                {
                    m_properties = value;
                }
            }
        }
    }
}
