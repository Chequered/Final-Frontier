using System;
using System.Collections.Generic;
using UnityEngine;

using FinalFrontier.Managers;
using FinalFrontier.Terrain;
using FinalFrontier.Entities;

namespace FinalFrontier
{
    namespace UI
    {
        public abstract class TerrainOverlay
        {
            private GameObject m_overlayObject;
            private Texture2D m_texture;
            private bool m_built = false;
            private bool m_isOverlayVisable = false;

            public void BuildOverlay()
            {
                int worldSize = TerrainManager.worldSize;
                m_overlayObject = GameObject.Instantiate(Resources.Load("Terrain/TerrainOverlay") as GameObject,
                          new Vector3(worldSize / 2 - 8f, worldSize / 2 - 8f, 0),
                          Quaternion.identity) as GameObject;
                m_overlayObject.transform.name = overlayName;
                m_overlayObject.transform.localScale = new Vector3(worldSize, worldSize, 1);
                m_overlayObject.layer = 9; //orthographic layer

                //transform
                GameObject overlays = GameObject.Find("Overlay");
                if (overlays == null)
                    overlays = new GameObject("Overlays");
                overlays.transform.parent = GameObject.Find("World").transform;
                m_overlayObject.transform.parent = overlays.transform;

                m_texture = new Texture2D(worldSize, worldSize);
                m_texture.filterMode = FilterMode.Point;
                m_texture.wrapMode = TextureWrapMode.Clamp;
                
                m_overlayObject.GetComponent<Renderer>().sharedMaterial = new Material(m_overlayObject.GetComponent<Renderer>().material);
                m_overlayObject.GetComponent<Renderer>().sharedMaterial.color = new Color(1, 1, 1, 0.25f);
                m_overlayObject.GetComponent<Renderer>().sharedMaterial.mainTexture = m_texture;

                m_built = true;

                ToggleOverlay(false);
            }

            public void DestroyOverlay()
            {
                GameObject overlay = GameObject.Find(overlayName);
                if (overlay != null)
                    overlay.AddComponent<GameObjectDestroyer>().Destroy();
            }

            public void UpdateOverlayAt(int tileX, int tileY)
            {
                if (!m_built)
                    BuildOverlay();
                
                m_texture.SetPixel(tileX, tileY, ReturnTileColor(tileX, tileY));
            }
            
            public void ApplyUpdate()
            {
                m_texture.Apply();
                m_overlayObject.GetComponent<Renderer>().sharedMaterial.mainTexture = m_texture;
            }

            //display
            public void ToggleOverlay()
            {
                m_isOverlayVisable = !m_isOverlayVisable;
                m_overlayObject.SetActive(m_isOverlayVisable);
            }

            public void ToggleOverlay(bool state)
            {
                m_isOverlayVisable = state;
                m_overlayObject.SetActive(state);
            }

            protected abstract Color ReturnTileColor(int x, int y);
            public abstract void OnStart();
            public abstract string overlayName { get; }
        }
    }
}

//Base TerrainOverlays

namespace FinalFrontier
{
    namespace UI
    {
        public class BuildableTerrainOverlay : TerrainOverlay
        {
            protected override Color ReturnTileColor(int x, int y)
            {
                Color result = Color.green * 0.75f;
                
                if (ManagerInstance.Get<TerrainManager>().tiles[x, y].identity == "void")
                    result = Color.clear;
                else if (!ManagerInstance.Get<BuildManager>().CheckBuildLocation(x, y, 1, 1))
                    result = Color.red;

                //create checkered pattern
                if (x % 2 == 0)
                {
                    if(y % 2 == 0)
                    {
                        result *= 1.05f;
                    }
                }
                else
                {
                    if(y % 2 != 0)
                        result *= 1.05f;
                }

                return result;
            }

            public override string overlayName
            {
                get { return "BuildableTerrain"; }
            }

            public override void OnStart()
            {
                ManagerInstance.Get<UIManager>().availableBuildingList.OnVisiblityToggle += ToggleOverlay;
            }
        }
    }
}
