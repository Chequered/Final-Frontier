using UnityEngine;
using System.Collections;
using FinalFrontier.Terrain;

namespace FinalFrontier
{
    namespace Terrain
    {
        public class TerrainChunkCollision : MonoBehaviour, IEngineEvents
        {
            private TerrainChunk _chunk;

            //MouseEvents
            private TerrainTile _hoveringTile;

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

                if(_hoveringTile != null)
                {
                    if (_hoveringTile != currentHoveringTile)
                    {
                        _hoveringTile.OnMouseEnter();
                    }
                }

                _hoveringTile = currentHoveringTile;
                if(_hoveringTile != null)
                    _hoveringTile.OnMouseOver();
            
            }

            public void OnMouseExit()
            {
                if (_hoveringTile != null)
                    _hoveringTile.OnMouseExit();
            }

            public void SetTerrainChunk(TerrainChunk chunk)
            {
                _chunk = chunk;
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

                        return _chunk.tiles[x, y];
                    }
                    return null;
                }
            }
        }
    }
}
