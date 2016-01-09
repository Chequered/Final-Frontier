using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using FinalFrontier.Terrain;
using FinalFrontier.Managers;
using FinalFrontier.Graphics;

namespace FinalFrontier
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
                        terrainTex.SetPixels(tileDrawQueue[i].x * res, tileDrawQueue[i].y * res, res, res, cache);
                        cached++;
                    }
                    else
                    {
                        //No cache found, drawing new one
                        TerrainTileGraphics graphics = ManagerInstance.Get<TerrainManager>().FindTerrainTileCache(tileDrawQueue[i].identity).graphics;
                        Color[] tex = graphics.GetTextureData(UnityEngine.Random.Range(0, graphics.variants));
                        terrainTex.SetPixels(tileDrawQueue[i].x * res, tileDrawQueue[i].y * res, res, res, tex);

                        //Caching new Texture
                        TerrainTileGraphicsCached graphicsCache = new TerrainTileGraphicsCached();
                        graphicsCache.CopyNeighborInfo(tileDrawQueue[i].neighborInfo);
                        graphicsCache.textureData = tex;
                        ManagerInstance.Get<TerrainManager>().CacheTexture(graphicsCache);
                    }
                    if (i % 16 == 0)
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
        }
    }
}
