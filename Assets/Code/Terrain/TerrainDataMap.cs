using System;
using System.Collections.Generic;
using UnityEngine;

using EndlessExpedition.Serialization;
using EndlessExpedition.Managers;
using EndlessExpedition.UI;

namespace EndlessExpedition
{
    namespace Terrain
    {
        class TerrainDataMap<T>
        {
            private T[,] m_dataMap;
            private List<TerrainOverlay> m_terrainOverlays;

            public TerrainDataMap()
            {
                m_dataMap = new T[TerrainManager.worldSize, TerrainManager.worldSize];
                m_terrainOverlays = new List<TerrainOverlay>();
            }

            public void AddTerrainOverlay(TerrainOverlay terrainOverlay)
            {
                m_terrainOverlays.Add(terrainOverlay);
            }

            public void RemoveTerrainOverlay(TerrainOverlay terrainOverlay)
            {
                m_terrainOverlays.Remove(terrainOverlay);
            }

            public void SetDataAt(int x, int y, T newData, bool updateOverlayTextures = true)
            {
                m_dataMap[x, y] = newData;

                if(updateOverlayTextures)
                {
                    for (int i = 0; i < m_terrainOverlays.Count; i++)
                    {
                        m_terrainOverlays[i].UpdateOverlayAt(x, y);
                    }
                }
            }

            public void SetAllData(T data, bool updateOverlayTextures = true)
            {
                for (int x = 0; x < TerrainManager.worldSize; x++)
                {
                    for (int y = 0; y < TerrainManager.worldSize; y++)
                    {
                        SetDataAt(x, y, data, updateOverlayTextures);
                    }
                }
                ApplyAllOverlays();
            }

            public void SetAllDataSpecific(T[,] data, bool updateOverlayTextures = true)
            {
                for (int x = 0; x < TerrainManager.worldSize; x++)
                {
                    for (int y = 0; y < TerrainManager.worldSize; y++)
                    {
                        SetDataAt(x, y, data[x, y], updateOverlayTextures);
                    }
                }
                ApplyAllOverlays();
            }

            public void UpdateAllOverlays()
            {
                for (int i = 0; i < m_terrainOverlays.Count; i++)
                {
                    for (int x = 0; x < TerrainManager.worldSize; x++)
                    {
                        for (int y = 0; y < TerrainManager.worldSize; y++)
                        {
                            m_terrainOverlays[i].UpdateOverlayAt(x, y);
                        }
                    }
                }
            }

            public void ApplyAllOverlays()
            {
                for (int i = 0; i < m_terrainOverlays.Count; i++)
                {
                    m_terrainOverlays[i].ApplyUpdate();
                }
            }

            public T GetDataAt(int x, int y)
            {
                return m_dataMap[x, y];
            }
        }
    }
}
