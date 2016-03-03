using System;
using UnityEngine;
using UnityEngine.UI;

namespace EndlessExpedition
{
    namespace Entities.Construction
    {
        public class ConstructionHeader
        {
            private ConstructionNode[,] m_nodes;
            private GameObject[,] m_GOs;
            private Building m_building;
            private GameObject m_gameObject;

            private bool m_initialized = false;
            private int m_width, m_height;

            public ConstructionHeader(Building building)
            {
                if (building == null)
                    throw new NullReferenceException();

                m_building = building;
                if (m_building.IsBuilt)
                {
                    //Fail
                    Destroy();
                    return;
                }
                //Succes
                Init();
            }

            private void Init()
            {
                m_initialized = true;
                m_width = m_building.properties.Get<int>("tileWidth");
                m_height = m_building.properties.Get<int>("tileHeight");
                m_nodes = new ConstructionNode[Width, Height];
                m_GOs = new GameObject[Width, Height];

                m_gameObject = new GameObject("ConstructionHeader: " + m_building.Identity);
                m_gameObject.transform.SetParent(m_building.LocalCanvas.transform);
                m_gameObject.transform.localPosition = Vector3.zero;

                GridLayoutGroup grid = m_gameObject.AddComponent<GridLayoutGroup>();
                grid.cellSize = new Vector2(1f, 1f);

                RectTransform rectTransform = m_gameObject.GetComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(m_width, m_height);

                for (int y = m_height - 1; y >= 0; y--)
                {
                    for (int x = 0; x < m_width; x++)
                    {
                    
                        GameObject node = new GameObject("node " + x + ", " + y);
                        node.transform.SetParent(m_gameObject.transform);
                        node.transform.localPosition = Vector3.zero;

                        m_nodes[x, y] = node.AddComponent<ConstructionNode>().Init(this, x, y);
                        m_nodes[x, y].OnConstructionFinishEvent += OnNodeFinish;

                        node.AddComponent<RawImage>().texture = m_nodes[x, y].OverlayTexture;
                        m_GOs[x, y] = node;
                    }
                }
            }

            private void Destroy()
            {

            }
            private void Complete()
            {
                for (int x = 0; x < m_width; x++)
                {
                    for (int y = 0; y < m_height; y++)
                    {
                        m_GOs[x, y].GetComponent<ConstructionNode>().DestroyTexture();
                        m_GOs[x, y].AddComponent<GameObjectDestroyer>().Destroy();
                    }
                }
                m_building.OnBuild();
                m_gameObject.AddComponent<GameObjectDestroyer>().Destroy();
            }

            private void OnNodeFinish(ConstructionNode node)
            {
                m_GOs[node.LocalX, node.LocalY].GetComponent<RawImage>().enabled = false;

                bool done = true;
                for (int x = 0; x < m_width; x++)
                {
                    for (int y = 0; y < m_height; y++)
                    {
                        if(!m_nodes[x, y].IsFinished)
                        {
                            done = false;
                            break;
                        }
                    }
                }
                if (done)
                    Complete();
            }

            public ConstructionNode GetClosestUnfishiedNode(Vector2 position)
            {
                ConstructionNode result = null;

                float d = float.MaxValue;

                if(m_nodes.Length > 0)
                {
                    for (int x = 0; x < m_width; x++)
                    {
                        for (int y = 0; y < m_height; y++)
                        {
                            if (m_nodes[x, y].IsFinished)
                                continue;
                            float ds = Vector2.Distance(position, m_nodes[x, y].WorldPosition);
                            if(ds < d)
                            {
                                d = ds;
                                result = m_nodes[x, y];
                            }
                        }
                    }
                }

                return result;
            }

            #region Properties
            public int Width
            {
                get
                {
                    return m_width;
                }
            }
            public int Height
            {
                get
                {
                    return m_height;
                }
            }
            public GameObject GameObject
            {
                get
                {
                    return m_gameObject;
                }
            }
            public Building Target
            {
                get
                {
                    return m_building;
                }
            }
            #endregion
        }
    }
}