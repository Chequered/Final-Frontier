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
            private int _maxProgress = 100;
            private int _currentProgress = 0;
            private bool _showGUI = false;
            
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
                _maxProgress = tileDrawQueue.Count;
                _showGUI = true;

                //texture resolution
                int res = TerrainTileGraphics.TEXTURE_RESOLUTION;

                int cached = 0;

                //Go through the chunks internal drawQueue
                for (int i = 0; i < tileDrawQueue.Count; i++)
                {
                    _currentProgress = i;

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
                        TerrainTileGraphics graphics = ManagerInstance.Get<TerrainManager>().FindFromCache(tileDrawQueue[i].identity).graphics;
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
                _showGUI = false;
                yield return new WaitForEndOfFrame();
            }

            #endregion

            #region GUI
            private void OnGUI()
            {
                if(_showGUI)
                {
                    GUI.Label(new Rect(Screen.width / 2 - 50, 25, 100, 25), "Drawing Terrain");
                    GUI.Label(new Rect(Screen.width / 2 - 15, 50, 25, 25), "" + _currentProgress);
                    GUI.Label(new Rect(Screen.width / 2 + 15, 50, 25, 25), "/" + _maxProgress);
                }
            }
            #endregion
        }
    }
}
