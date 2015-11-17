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

                //we draw a circle to represent the shape of the planet
                _planetTerrainShape = ManagerInstance.Get<TerrainManager>().DataCircle(TerrainManager.worldSize / 2, TerrainManager.worldSize / 2, TerrainManager.worldSize / 2);
            }

            public void GenerateTerrain(TerrainTile[,] terrainTiles)
            {
                _perlinMap = GeneratePerlinMap(
                    new float[]{5.5f, 5.5f, 6.5f, 5, 1}, //scale
                    new float[]{0f, 250f, 125f, 500f, 0}, //seed addition
                    new float[]{1f, 0.8f, 0.75f, 0.35f, 0.15f}); //strength 
                GenerateTerrain(terrainTiles, _perlinMap, "primary"); //our primary tiles
                GenerateTerrain(terrainTiles, _perlinMap, "secondary"); //our secondary tiles
                GenerateTerrain(terrainTiles, _perlinMap, "resource"); //the planet's resources

                //Now for the flora
                Prop[] flora = ManagerInstance.Get<EntityManager>().GetLoadedFlora(_worldInfo.properties.Get<string>("type")); //all the flora that can grow on our planet
                for (int x = 0; x < TerrainManager.worldSize; x++)
                {
                    for (int y = 0; y < TerrainManager.worldSize; y++)
                    {
                        float perlin = _perlinMap[x, y];
                        perlin = Mathf.Clamp(perlin, 0f, 1f);
                        for (int i = 0; i < flora.Length; i++)
                        {
                            float min = flora[i].properties.Get<float>("minPerlinValue"); //the max perlin value for the plant
                            float max = flora[i].properties.Get<float>("maxPerlinValue"); //the min perlin value for the plant
                            
                            //if the terrain's perlin value is between the mix & max and the tile is within the round shape of the planet
                            if (perlin >= min && perlin <= max && terrainTiles[x, y].properties.Get<string>("identity") != "void")
                            {
                                string perTxt = perlin + ""; //we cast the perlin float to a string
                                int lastDigit = (int)(perTxt[perTxt.Length - 1]); //we get the last decimal of our casted floar
                                
                                //this is to sort of randomzie the look of flora, even though it's still procedural
                                //only if the last decial is 3 we will place a plant
                                if (lastDigit % 3 == 0) 
                                    ManagerInstance.Get<EntityManager>().CreateEntityFrom<Prop>(flora[i], x, y); //register our entity
                            }
                        }
                    }
                }
            }

            public void GenerateTerrain(TerrainTile[,] terrainTiles, float[,] perlinMap, string tileType)
            {
                TerrainTileCache voidTile = ManagerInstance.Get<TerrainManager>().FindFromCache("void"); //reference to the "void" tile, a tile that represents empty space
                
                //all the tiles that are avaiable for our planet type
                TerrainTileCache[] possibleTiles = ManagerInstance.Get<TerrainManager>().AvaivableTilesForPlanetType(_worldInfo.properties.Get<string>("type"), tileType);

                for (int x = 0; x < TerrainManager.worldSize; x++)
                {
                    for (int y = 0; y < TerrainManager.worldSize; y++)
                    {
                        float perlin = perlinMap[x, y];
                        perlin = Mathf.Clamp(perlin, 0f, 1f);
                        for (int i = 0; i < possibleTiles.Length; i++)
                        {
                            float min = possibleTiles[i].properties.Get<float>("minPerlinValue"); //mix perlin value of our tile
                            float max = possibleTiles[i].properties.Get<float>("maxPerlinValue"); //max perlin value of our tile
                            if (_planetTerrainShape[x, y]) //if the tile is within the planet's round shape
                            {
                                if (perlin >= min && perlin <= max) //and if the perlin value is between our min & max.
                                    terrainTiles[x, y].SetTo(possibleTiles[i]); 

                                terrainTiles[x, y].properties.Set("perlinNoiseValue", perlin); //we save the tile's perlin value to future use
                            }
                        }
                    }
                }
            }

            //here we generate our actual perlin map, the result is a 2 dimensional array of floats
            //each float wll be between 0.0 and 1.0 and represents the terrain's height
            public float[,] GeneratePerlinMap(float[] scales, float[] additions, float[] strengths)
            {
                int size = TerrainManager.worldSize; //the diameter of the planet
                float[,] noise = new float[size, size]; //the data holder
                float totalStrength = 0f; //the perlin map's total strength, this is for later use
                for (int i = 0; i < strengths.Length; i++)
                {
                    totalStrength += strengths[i];
                }

                for (int i = 0; i < scales.Length; i++)
                {
                    float seed = _worldInfo.properties.Get<float>("seed") + additions[i]; //we get our seed and add the octave's addition

                    float y = 0.0F;
                    while (y < size)
                    {
                        float x = 0.0F;
                        while (x < size)
                        {
                            float xCoord = seed + x / size * scales[i]; //the horizontal position of the perlin map
                            float yCoord = seed + y / size * scales[i]; //the vertical position of the perlin map
                            float sample = Mathf.PerlinNoise(xCoord, yCoord); //we get our perlin value
                            noise[(int)x, (int)y] += sample * strengths[i]; //store it
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
