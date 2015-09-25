using System;

using FinalFrontier.Serialization;
using FinalFrontier.Managers;

namespace FinalFrontier
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
            private Properties _properties;

            public TerrainGenerationWorldInfo()
            {
                _properties = new Properties("saves");

                //Secure default properties
                _properties.Secure("seed", 0f);
                _properties.Secure("name", "Unamed Planet");
                _properties.Secure("type", "Dust");
                _properties.Secure("x", 0);
                _properties.Secure("y", 0);
                _properties.Secure("temperature", 12f); //celcius
                _properties.Secure("oxygenLevel", 0.5f);
                _properties.Secure("containsFlora", true);
            }

            public void GenerateTerrainProperties(float seed)
            {
                //trim the seed
                if (seed < MIN_SEED_VALUE) seed = MIN_SEED_VALUE;
                if (seed > MAX_SEED_VALUE) seed = MAX_SEED_VALUE;
                _properties.Set("seed", seed);
                                
                //Name
                //TODO: get name from perlinSample
                _properties.Set("name", randomName + " - " + Math.Round(seed));

                //Type
                string perTxt = seed + "";
                int lastDigit = (int)(perTxt[perTxt.Length - 1]);
                _properties.Set("type", "Dust");
                //TypeFromInt(lastDigit)

                //Position
                //TODO: make it so x & y are not always the same
                int root = (int)Math.Sqrt(Math.Round(seed));
                _properties.Set("x", root);
                _properties.Set("y", root);

                //Temperature
                if (seed < 0f) _properties.Set("temperature", seed * TEMP_SEED_SCALE_NEGATIVE);
                if (seed > 0f) _properties.Set("temperature", seed * TEMP_SEED_SCALE_POSITIVE);

                //Oxygen
                if (seed < 0f) _properties.Set("oxygenLevel", seed * OXYGEN_SEED_SCALE_NEGATIVE);
                if (seed > 0f) _properties.Set("oxygenLevel", seed * OXYGEN_SEED_SCALE_POSITIVE);

                //Flora
                if (lastDigit < 5) _properties.Set("containsFlora", false);

                //DEBUG
                ManagerInstance.Get<UIManager>().InspectPropeties(_properties);
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
                    return _properties;
                }
            }
        }
    }
}
