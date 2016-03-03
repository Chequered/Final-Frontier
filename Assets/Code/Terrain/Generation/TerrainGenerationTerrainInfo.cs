using System;

using UnityEngine;

using EndlessExpedition.Serialization;
using EndlessExpedition.Managers;
using EndlessExpedition.Entities;

namespace EndlessExpedition
{
    namespace Terrain.Generation
    {
        public class TerrainGenerationTerrainInfo
        {
            //References
            private TerrainGenerationWorldInfo m_worldInfo;

            //Terrain arrays
            private float[,] m_perlinMap;
            private bool[,] m_planetTerrainShape;

            public TerrainGenerationTerrainInfo(TerrainGenerationWorldInfo worldInfo)
            {
                m_worldInfo = worldInfo;

                m_planetTerrainShape = ManagerInstance.Get<TerrainManager>().DataCircle(TerrainManager.worldSize / 2, TerrainManager.worldSize / 2, TerrainManager.worldSize / 2);
            }

            public void GenerateTerrain(TerrainTile[,] terrainTiles)
            {
                //TODO: use several octaves
                m_perlinMap = GeneratePerlinMap(
                    new float[]{5.5f, 5.5f, 6.5f, 5, 1, 25, 55}, //scale
                    new float[]{0f, 250f, 125f, 500f, 0, 125, 550}, //seed addition
                    new float[]{1f, 0.8f, 0.75f, 0.35f, 0.15f, 0.35f, 0.25f}); //strength of octave
                GenerateTerrain(terrainTiles, m_perlinMap, "primary");
                GenerateTerrain(terrainTiles, m_perlinMap, "secondary");
                GenerateTerrain(terrainTiles, m_perlinMap, "resource");

                //Now for the flora
                Prop[] flora = ManagerInstance.Get<EntityManager>().GetLoadedFlora(m_worldInfo.properties.Get<string>("type"));
                for (int x = 0; x < TerrainManager.worldSize; x++)
                {
                    for (int y = 0; y < TerrainManager.worldSize; y++)
                    {
                        float perlin = m_perlinMap[x, y];
                        perlin = Mathf.Clamp(perlin, 0f, 1f);
                        for (int i = 0; i < flora.Length; i++)
                        {
                            float min = flora[i].properties.Get<float>("minPerlinValue");
                            float max = flora[i].properties.Get<float>("maxPerlinValue");
                            if (perlin >= min && perlin <= max && terrainTiles[x, y].properties.Get<string>("identity") != "void")
                            {
                                string perTxt = perlin + "";
                                int lastDigit = (int)(perTxt[perTxt.Length - 1]);

                                if (lastDigit % 3 == 0)
                                    ManagerInstance.Get<EntityManager>().CreateEntity<Prop>(flora[i], x, y);
                            }
                        }
                    }
                }
            }

            public void LoadTerrain(TerrainTile[,] terrainTiles)
            {
                Properties[] tiles = GameManager.saveDataContainer.saveGame.terrainProperties.ToArray();
                for (int x = 0; x < TerrainManager.worldSize; x++)
                {
                    for (int y = 0; y < TerrainManager.worldSize; y++)
                    {
                        terrainTiles[x, y].SetTo(ManagerInstance.Get<TerrainManager>().FindTerrainTileCache(tiles[y * TerrainManager.worldSize + x].Get<string>("identity")));
                        terrainTiles[x, y].properties.SetAll(tiles[y * TerrainManager.worldSize + x]);
                    }
                }
            }

            public void GenerateTerrain(TerrainTile[,] terrainTiles, float[,] perlinMap, string tileType)
            {
                TerrainTileCache[] possibleTiles = ManagerInstance.Get<TerrainManager>().AvaivableTilesForPlanetType(m_worldInfo.properties.Get<string>("type"), tileType);

                for (int x = 0; x < TerrainManager.worldSize; x++)
                {
                    for (int y = 0; y < TerrainManager.worldSize; y++)
                    {
                        float perlin = perlinMap[x, y];
                        perlin = Mathf.Clamp(perlin, 0f, 1f);
                        for (int i = 0; i < possibleTiles.Length; i++)
                        {
                            float min = possibleTiles[i].properties.Get<float>("minPerlinValue");
                            float max = possibleTiles[i].properties.Get<float>("maxPerlinValue");
                            if (m_planetTerrainShape[x, y])
                            {
                                if (perlin >= min && perlin <= max)
                                    terrainTiles[x, y].SetTo(possibleTiles[i]);

                                terrainTiles[x, y].properties.Set("perlinNoiseValue", perlin);
                            }
                        }
                    }
                }
            }

            public Sprite perlinTexture
            {
                get
                {
                    Texture2D tex = new Texture2D(TerrainManager.worldSize, TerrainManager.worldSize);

                    for (int x = 0; x < TerrainManager.worldSize; x++)
                    {
                        for (int y = 0; y < TerrainManager.worldSize; y++)
                        {
                            float perlin = m_perlinMap[x, y];
                            tex.SetPixel(x, y, new Color(perlin, perlin, perlin, 1));
                        }
                    }

                    tex.Apply();

                    return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                }
            }

            public float[,] GeneratePerlinMap(float[] scales, float[] additions, float[] strengths)
            {
                int size = TerrainManager.worldSize;
                float[,] noise = new float[size, size];
                float totalStrength = 0f;
                for (int i = 0; i < strengths.Length; i++)
                {
                    totalStrength += strengths[i];
                }

                for (int i = 0; i < scales.Length; i++)
                {
                    float seed = m_worldInfo.properties.Get<float>("seed") + additions[i];

                    float y = 0.0F;
                    while (y < size)
                    {
                        float x = 0.0F;
                        while (x < size)
                        {
                            float xCoord = seed + x / size * scales[i];
                            float yCoord = seed + y / size * scales[i];
                            float sample = Mathf.PerlinNoise(xCoord, yCoord);
                            float final = sample * strengths[i];
                            noise[(int)x, (int)y] += final;
                            x++;
                        }
                        y++;
                    }
                }


                //bring it back to 0 - 1
                for (int x = 0; x < size; x++)
                {
                    for (int y = 0; y < size; y++)
                    {
                        noise[x, y] /= totalStrength;
                    }
                }

                return noise;
            }

            public bool IsInPlanetsShape(int x, int y)
            {
                return m_planetTerrainShape[x, y];
            }
        }
    }
    public static class TerrainExtensions
    {
        public static float[,] GeneratePerlinMap(int width, int height, float seed, float[] scales, float[] additions, float[] strengths)
        {
            float[,] dataMap = new float[width, height];
            float totalStrength = 0f;
            for (int i = 0; i < strengths.Length; i++)
            {
                totalStrength += strengths[i];
            }

            for (int i = 0; i < scales.Length; i++)
            {
                seed += additions[i];

                float y = 0.0F;
                while (y < height)
                {
                    float x = 0.0F;
                    while (x < width)
                    {
                        float xCoord = seed + x / width * scales[i];
                        float yCoord = seed + y / height * scales[i];
                        float sample = Mathf.PerlinNoise(xCoord, yCoord);
                        float final = sample * strengths[i];
                        dataMap[(int)x, (int)y] += final;
                        x++;
                    }
                    y++;
                }
            }


            //bring it back to 0 - 1
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    dataMap[x, y] /= totalStrength;
                }
            }
            return dataMap;
        }
        public static bool[,] DataCircle(int radius, int centerX, int centerY)
        {
            bool[,] result = new bool[radius * 2, radius * 2];

            int r = radius;
            int ox = centerX, oy = centerY;
            int size = radius * 2;

            for (int x = -r; x < r; x++)
            {
                int height = (int)Mathf.Sqrt(r * r - x * x);

                for (int y = -height; y < height; y++)
                {
                    if (x + ox >= 0 && x + ox < size && y + oy >= 0 && y + oy < size)
                        result[x + ox, y + oy] = true;
                }
            }
            return result;
        }
    }
}
