using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using EndlessExpedition.Terrain;
using EndlessExpedition.Managers;
using EndlessExpedition.Graphics;

namespace EndlessExpedition
{
    namespace Terrain
    {
        public class TerrainOperationHandler : MonoBehaviour
        {
            //Progress GUI
            private int m_maxProgress = 100;
            private int m_currentProgress = 0;
            private bool m_showGUI = false;
            
            #region Draw
            public void Draw(TerrainChunk chunk)
            {
                StartCoroutine(DrawOperation(chunk));
            }

            private IEnumerator DrawOperation(TerrainChunk chunk)
            {
                //Get some references
                Texture2D terrainTex = chunk.texture;
                List<TerrainTile> tileDrawQueue = chunk.terrainTileDrawQueue;

                //Setup Progress GUI
                m_maxProgress = tileDrawQueue.Count;
                m_showGUI = true;

                //texture resolution
                int res = TerrainTileGraphics.TILE_TEXTURE_RESOLUTION;

                int cached = 0;

                //Go through the chunks internal drawQueue
                for (int i = 0; i < tileDrawQueue.Count; i++)
                {
                    m_currentProgress = i;

                    //Search for cached version
                    Color[] cache = ManagerInstance.Get<TerrainManager>().GetCachedTexture(tileDrawQueue[i]);
                    if (cache != null)
                    {
                        //Cache found
                        terrainTex.SetPixels(tileDrawQueue[i].localX * res, tileDrawQueue[i].localY * res, res, res, cache);
                        cached++;
                    }
                    else
                    {
                        //No cache found, drawing new one
                        TerrainTileGraphics graphics = ManagerInstance.Get<TerrainManager>().FindTerrainTileCache(tileDrawQueue[i].identity).graphics;
                        Color[] tex = new Color[TerrainTileGraphics.TILE_TEXTURE_RESOLUTION * TerrainTileGraphics.TILE_TEXTURE_RESOLUTION];

                        int variant = UnityEngine.Random.Range(0, graphics.variants);
                        for (int pixel = 0; pixel < tex.Length; pixel++)
                        {
                            tex[pixel] = graphics.GetTextureData(variant)[pixel];
                        }

                        TileTexture(tex, tileDrawQueue[i].x, tileDrawQueue[i].y);
                        terrainTex.SetPixels(tileDrawQueue[i].localX * res, tileDrawQueue[i].localY * res, res, res, tex);

                        //Caching new Texture
                        TerrainTileGraphicsCached graphicsCache = new TerrainTileGraphicsCached();
                        graphicsCache.CopyNeighborInfo(tileDrawQueue[i].neighborInfo);
                        graphicsCache.textureData = tex;
                        ManagerInstance.Get<TerrainManager>().CacheTexture(graphicsCache);
                    }
                    if (i % TerrainChunk.SIZE / 2 == 0)
                        yield return new WaitForEndOfFrame();
                }
                tileDrawQueue.Clear();
                terrainTex.Apply();

                yield return new WaitForEndOfFrame();

                //hide progress GUI
                m_showGUI = false;
                yield return new WaitForEndOfFrame();
            }

            #endregion

            #region GUI
            private void OnGUI()
            {
                if(m_showGUI)
                {
                    GUI.Label(new Rect(Screen.width / 2 - 50, 25, 100, 25), "Drawing Terrain");
                    GUI.Label(new Rect(Screen.width / 2 - 15, 50, 25, 25), "" + m_currentProgress);
                    GUI.Label(new Rect(Screen.width / 2 + 15, 50, 25, 25), "/" + m_maxProgress);
                }
            }
            #endregion

            private void TileTexture(Color[] pix, int x, int y)
            {
                int TILE_RESOLUTION = TerrainTileGraphics.TILE_TEXTURE_RESOLUTION;
                TerrainTile tile = ManagerInstance.Get<TerrainManager>().tiles[x, y];
                TerrainTileCache cached = ManagerInstance.Get<TerrainManager>().FindTerrainTileCache(tile.identity);

                if (TerrainManager.isLocationValid(x, y))
                {
                    if (x - 1 >= 0 && tile.neighborInfo.left != "void" && tile.identity != "void")
                        if (tile.neighborInfo.left != tile.identity)
                        {
                            TerrainTileCache neighborCache = ManagerInstance.Get<TerrainManager>().FindTerrainTileCache(tile.neighborInfo.left);
                            if (cached.loadID > neighborCache.loadID) //Load ID deciced who gets the gradient
                            {
                                Color[] neightborTex = neighborCache.graphics.texture().GetPixels();
                                for (int i = 0; i < pix.Length; i++)
                                {
                                    Gradient gradient = new Gradient();

                                    GradientColorKey[] gck = new GradientColorKey[2];
                                    gck[0].color = pix[i];
                                    gck[0].time = 0.0f;
                                    gck[1].color = neightborTex[i];
                                    gck[1].time = 1.0f;

                                    // Populate the alpha  keys at relative time 0 and 1  (0 and 100%)
                                    GradientAlphaKey[] gak = new GradientAlphaKey[2];
                                    gak[0].alpha = 1.0f;
                                    gak[0].time = 0.0f;
                                    gak[1].alpha = 1f;
                                    gak[1].time = 1.0f;

                                    gradient.SetKeys(gck, gak);

                                    if (i % TILE_RESOLUTION == 0)
                                    {
                                        pix[i] = gradient.Evaluate(1f);
                                    }
                                    if ((i - 1) % TILE_RESOLUTION == 0)
                                    {
                                        pix[i] = gradient.Evaluate(0.8f);
                                    }
                                    if ((i - 2) % TILE_RESOLUTION == 0)
                                    {
                                        pix[i] = gradient.Evaluate(0.6f);
                                    }
                                    if ((i - 3) % TILE_RESOLUTION == 0)
                                    {
                                        pix[i] = gradient.Evaluate(0.4f);
                                    }
                                    if ((i - 4) % TILE_RESOLUTION == 0)
                                    {
                                        pix[i] = gradient.Evaluate(0.2f);
                                    }
                                }
                            }
                        }
                    if (y + 1 < TerrainManager.worldSize && tile.neighborInfo.top != "void" && tile.identity != "void")
                        if (tile.neighborInfo.top != "void" && tile.neighborInfo.top != tile.identity)
                        {
                            TerrainTileCache neighborCache = ManagerInstance.Get<TerrainManager>().FindTerrainTileCache(tile.neighborInfo.top);
                            if (cached.loadID > neighborCache.loadID) //Load ID deciced who gets the gradient
                            {
                                Color[] neightborTex = neighborCache.graphics.texture().GetPixels();

                                for (int i = 0; i < pix.Length; i++)
                                {
                                    Gradient gradient = new Gradient();

                                    GradientColorKey[] gck = new GradientColorKey[2];
                                    gck[0].color = pix[i];
                                    gck[0].time = 0.0f;
                                    gck[1].color = neightborTex[i];
                                    gck[1].time = 1.0f;

                                    // Populate the alpha  keys at relative time 0 and 1  (0 and 100%)
                                    GradientAlphaKey[] gak = new GradientAlphaKey[2];
                                    gak[0].alpha = 1.0f;
                                    gak[0].time = 0.0f;
                                    gak[1].alpha = 1f;
                                    gak[1].time = 1.0f;

                                    gradient.SetKeys(gck, gak);

                                    if (i > pix.Length - (TILE_RESOLUTION + 1))
                                    {
                                        pix[i] = gradient.Evaluate(1f);
                                    }
                                    if (i > pix.Length - (TILE_RESOLUTION + 1) * 2 && i < pix.Length - (TILE_RESOLUTION))
                                    {
                                        pix[i] = gradient.Evaluate(0.8f);
                                    }
                                    if (i > pix.Length - (TILE_RESOLUTION + 1) * 3 && i < pix.Length - (TILE_RESOLUTION) * 2)
                                    {
                                        pix[i] = gradient.Evaluate(0.6f);
                                    }
                                    if (i > pix.Length - (TILE_RESOLUTION + 1) * 4 && i < pix.Length - (TILE_RESOLUTION) * 3)
                                    {
                                        pix[i] = gradient.Evaluate(0.4f);
                                    }
                                    if (i > pix.Length - (TILE_RESOLUTION + 1) * 5 && i < pix.Length - (TILE_RESOLUTION) * 4)
                                    {
                                        pix[i] = gradient.Evaluate(0.2f);
                                    }
                                }
                            }
                        }
                    if (x + 1 < TerrainManager.worldSize && tile.neighborInfo.right != "void" && tile.identity != "void")
                        if (tile.neighborInfo.right != "void" && tile.neighborInfo.right != tile.identity)
                        {
                            TerrainTileCache neighborCache = ManagerInstance.Get<TerrainManager>().FindTerrainTileCache(tile.neighborInfo.right);
                            if (cached.loadID > neighborCache.loadID) //Load ID deciced who gets the gradient
                            {
                                Color[] neightborTex = neighborCache.graphics.texture().GetPixels();

                                for (int i = 0; i < pix.Length; i++)
                                {
                                    Gradient gradient = new Gradient();

                                    GradientColorKey[] gck = new GradientColorKey[2];
                                    gck[0].color = pix[i];
                                    gck[0].time = 0.0f;
                                    gck[1].color = neightborTex[i];
                                    gck[1].time = 1.0f;

                                    // Populate the alpha  keys at relative time 0 and 1  (0 and 100%)
                                    GradientAlphaKey[] gak = new GradientAlphaKey[2];
                                    gak[0].alpha = 1.0f;
                                    gak[0].time = 0.0f;
                                    gak[1].alpha = 1f;
                                    gak[1].time = 1.0f;

                                    gradient.SetKeys(gck, gak);

                                    if ((i + 1) % TILE_RESOLUTION == 0)
                                    {
                                        pix[i] = gradient.Evaluate(1);
                                    }
                                    if ((i + 2) % TILE_RESOLUTION == 0)
                                    {
                                        pix[i] = gradient.Evaluate(0.8f);
                                    }
                                    if ((i + 3) % TILE_RESOLUTION == 0)
                                    {
                                        pix[i] = gradient.Evaluate(0.6f);
                                    }
                                    if ((i + 4) % TILE_RESOLUTION == 0)
                                    {
                                        pix[i] = gradient.Evaluate(0.4f);
                                    }
                                    if ((i + 5) % TILE_RESOLUTION == 0)
                                    {
                                        pix[i] = gradient.Evaluate(0.2f);
                                    }
                                }
                            }
                        }
                    if (y - 1 >= 0 && tile.neighborInfo.bottom != "void" && tile.identity != "void")
                        if (tile.neighborInfo.bottom != "void" && tile.neighborInfo.bottom != tile.identity)
                        {
                            TerrainTileCache neighborCache = ManagerInstance.Get<TerrainManager>().FindTerrainTileCache(tile.neighborInfo.bottom);
                            if (cached.loadID > neighborCache.loadID) //Load ID deciced who gets the gradient
                            {
                                Color[] neightborTex = neighborCache.graphics.texture().GetPixels();

                                for (int i = 0; i < pix.Length; i++)
                                {
                                    Gradient gradient = new Gradient();

                                    GradientColorKey[] gck = new GradientColorKey[2];
                                    gck[0].color = pix[i];
                                    gck[0].time = 0.0f;
                                    gck[1].color = neightborTex[i];
                                    gck[1].time = 1.0f;

                                    // Populate the alpha  keys at relative time 0 and 1  (0 and 100%)
                                    GradientAlphaKey[] gak = new GradientAlphaKey[2];
                                    gak[0].alpha = 1.0f;
                                    gak[0].time = 0.0f;
                                    gak[1].alpha = 1f;
                                    gak[1].time = 1.0f;

                                    gradient.SetKeys(gck, gak);

                                    if (i <= TILE_RESOLUTION)
                                    {
                                        pix[i] = gradient.Evaluate(1f);
                                    }
                                    if (i <= TILE_RESOLUTION * 2 && i > TILE_RESOLUTION)
                                    {
                                        pix[i] = gradient.Evaluate(0.8f);
                                    }
                                    if (i <= TILE_RESOLUTION * 3 && i > TILE_RESOLUTION * 2)
                                    {
                                        pix[i] = gradient.Evaluate(0.6f);
                                    }
                                    if (i <= TILE_RESOLUTION * 4 && i > TILE_RESOLUTION * 3)
                                    {
                                        pix[i] = gradient.Evaluate(0.4f);
                                    }
                                    if (i <= TILE_RESOLUTION * 5 && i > TILE_RESOLUTION * 4)
                                    {
                                        pix[i] = gradient.Evaluate(0.2f);
                                    }
                                }
                            }
                        }
                }
            }
        }
    }
}
