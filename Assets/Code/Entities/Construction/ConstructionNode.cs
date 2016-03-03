using UnityEngine;

using EndlessExpedition.Graphics;

namespace EndlessExpedition
{
    namespace Entities.Construction
    {
        public class ConstructionNode : MonoBehaviour
        {
            public delegate void OnConstructionNodeFinishEventHandler(ConstructionNode node);
            public OnConstructionNodeFinishEventHandler OnConstructionFinishEvent;

            private ConstructionHeader m_header;
            private float m_maxProgress;
            private float m_currentProgress;
            private Texture2D m_texture;
            private int m_localX, m_localY;

            public ConstructionNode Init(ConstructionHeader header, int x, int y)
            {
                m_header = header;
                m_localX = x;
                m_localY = y;

                m_currentProgress = 0f;
                m_maxProgress = 100f;

                m_texture = new Texture2D(GraphicsBase.TILE_TEXTURE_RESOLUTION, 
                                          GraphicsBase.TILE_TEXTURE_RESOLUTION);

                for (int tX = 0; tX < m_texture.width; tX++)
                {
                    for (int tY = 0; tY < m_texture.height; tY++)
                    {
                        m_texture.SetPixel(tX, tY, new Color(0.75f, 0.75f, 0.75f, 0.55f));
                    }
                }
                m_texture.Apply();

                return this;
            }

            public void ProgressConstruction(float progression)
            {
                m_currentProgress += progression;
                
                if(m_currentProgress > 0)
                {
                    Color[] pixels = m_texture.GetPixels();
                    int total = pixels.Length;
                    int texProgression = (int)(m_currentProgress / (m_maxProgress / 100f));
                    int targetIndex = (int)Mathf.Floor(total / 100f * texProgression);

                    for (int i = 0; i < pixels.Length; i++)
                    {
                        if (i <= targetIndex)
                            pixels[i] = Color.clear;
                        else
                            break;
                    }
                    m_texture.SetPixels(pixels);
                    m_texture.Apply();
                }
                if (m_currentProgress >= m_maxProgress)
                    Complete();
            }

            public void DestroyTexture()
            {
                DestroyImmediate(m_texture);
            }
            private void Complete()
            {
                if (OnConstructionFinishEvent != null)
                    OnConstructionFinishEvent(this);
            }

            #region Properties
            public Vector2 WorldPosition
            {
                get
                {
                    CMD.Log(string.Format("{0} {1} {2}", m_header.Target.gameObject.transform.position, new Vector3(m_header.Width / 2, m_header.Height / 2), new Vector3(m_localX, m_localY)));
                    return m_header.Target.gameObject.transform.position - new Vector3(m_header.Width / 2, m_header.Height / 2) + new Vector3(m_localX, m_localY);
                }
            }
            public int LocalX
            {
                get
                {
                    return m_localX;
                }
            }
            public int LocalY
            {
                get
                {
                    return m_localY;
                }
            }
            public bool IsFinished
            {
                get
                {
                    return m_currentProgress >= m_maxProgress ? true : false;
                }
            }
            public Texture2D OverlayTexture
            {
                get
                {
                    return m_texture;
                }
            }
            #endregion
        }
    }
}