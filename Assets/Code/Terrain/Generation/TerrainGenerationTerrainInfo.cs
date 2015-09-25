using System;

using UnityEngine;

using FinalFrontier.Serialization;
using FinalFrontier.Managers;
using FinalFrontier.Entities;

namespace FinalFrontier
{
    namespace Terrain.Generation
    {
        public class TerrainGenerationTerrainInfo
        {
            //References
            private TerrainGenerationWorldInfo _worldInfo;

            //Terrain arrays
            private float[,] _perlinMap;
            private bool[,] _planetTerrainShape;

            public TerrainGenerationTerrainInfo(TerrainGenerationWorldInfo worldInfo)
            {
                _worldInfo = worldInfo;

                _planetTerrainShape = ManagerInstance.Get<TerrainManager>().DataCircle(TerrainManager.worldSize / 2, TerrainManager.worldSize / 2, TerrainManager.worldSize / 2);
            }

            public void GenerateTerrain(TerrainTile[,] terrainTiles)
            {
                //TODO: use several octaves
                _perlinMap = GeneratePerlinMap(
                    new float[]{5.5f, 5.5f, 6.5f, 5, 1}, //scale
                    new float[]{0f, 250f, 125f, 500f, 0}, //seed addition
                    new float[]{1f, 0.8f, 0.75f, 0.35f, 0.15f}); //strength of octave
                GenerateTerrain(terrainTiles, _perlinMap, "primary");
                GenerateTerrain(terrainTiles, _perlinMap, "secondary");
                GenerateTerrain(terrainTiles, _perlinMap, "resource");

                //Now for the flora
                Prop[] flora = ManagerInstance.Get<EntityManager>().GetLoadedFlora(_worldInfo.properties.Get<string>("type"));
                for (int x = 0; x < TerrainManager.worldSize; x++)
                {
                    for (int y = 0; y < TerrainManager.worldSize; y++)
                    {
                        float perlin = _perlinMap[x, y];
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
                                    ManagerInstance.Get<EntityManager>().CreateEntityFrom<Prop>(flora[i], x, y);
                            }
                        }
                    }
                }
            }

            public void GenerateTerrain(TerrainTile[,] terrainTiles, float[,] perlinMap, string tileType)
            {
                TerrainTileCache voidTile = ManagerInstance.Get<TerrainManager>().FindFromCache("void");
                TerrainTileCache[] possibleTiles = ManagerInstance.Get<TerrainManager>().AvaivableTilesForPlanetType(_worldInfo.properties.Get<string>("type"), tileType);

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
                            if (_planetTerrainShape[x, y])
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
                            float perlin = _perlinMap[x, y];
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
                    float seed = _worldInfo.properties.Get<float>("seed") + additions[i];

                    float y = 0.0F;
                    while (y < size)
                    {
                        float x = 0.0F;
                        while (x < size)
                        {
                            float xCoord = seed + x / size * scales[i];
                            float yCoord = seed + y / size * scales[i];
                            float sample = Mathf.PerlinNoise(xCoord, yCoord);
                            noise[(int)x, (int)y] += sample * strengths[i];
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
        }
    }
}
