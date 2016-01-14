using UnityEngine;
using System.Collections;
using EndlessExpedition.Terrain;

namespace EndlessExpedition
{
    namespace Terrain
    {
        public class TerrainChunkCollision : MonoBehaviour, IEngineEvents
        {
            private TerrainChunk m_chunk;

            //MouseEvents
            private TerrainTile m_hoveringTile;

            public void OnStart()
            {

            }

            public void OnTick()
            {

            }

            public void OnUpdate()
            {

            }

            public void OnMouseEnter()
            {
                currentHoveringTile.OnMouseEnter();
            }

            public void OnMouseOver()
            {
                //Check if mouse is over UI
                //If so, do nothing
                if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                    return;

                if(m_hoveringTile != null)
                {
                    if (m_hoveringTile != currentHoveringTile)
                    {
                        m_hoveringTile.OnMouseEnter();
                    }
                }

                m_hoveringTile = currentHoveringTile;
                if(m_hoveringTile != null)
                    m_hoveringTile.OnMouseOver();
            
            }

            public void OnMouseExit()
            {
                if (m_hoveringTile != null)
                    m_hoveringTile.OnMouseExit();
            }

            public void SetTerrainChunk(TerrainChunk chunk)
            {
                m_chunk = chunk;
            }

            private TerrainTile currentHoveringTile
            {
                get
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;

                    if (Physics.Raycast(ray, out hit))
                    {
                        Vector3 mouseHit = hit.transform.InverseTransformPoint(hit.point);
                        int x = (int)Mathf.Floor(TerrainChunk.SIZE * (mouseHit.x + 0.5f));
                        int y = (int)Mathf.Floor(TerrainChunk.SIZE * (mouseHit.y + 0.5f));

                        if(x < TerrainChunk.SIZE && x >= 0 && y < TerrainChunk.SIZE && y >= 0)
                            return m_chunk.tiles[x, y];
                    }
                    return null;
                }
            }

            public TerrainChunk chunkRef
            {
                get
                {
                    return m_chunk;
                }
            }
        }
    }
}
