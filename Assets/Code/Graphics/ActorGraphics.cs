
using System.Collections.Generic;
using System.IO;

using UnityEngine;

using EndlessExpedition.Serialization;
using EndlessExpedition.Entities;

namespace EndlessExpedition
{
    namespace Graphics
    {
        public class ActorGraphics : GraphicsBase
        {
            private int m_pixWidth;
            private int m_pixHeight;
            private Actor m_actor;
            private LineRenderer m_lineRenderer;

            public ActorGraphics(Entity entity) : base(entity)
            {
                p_textureData = new List<UnityEngine.Color[]>();
                p_folder = "entities/actors";
                m_actor = entity as Actor;
                m_actor.OnLocationTargetReached += OnTargetReached;
                m_actor.OnBuildingTargetReached += OnTargetReached;
                m_actor.OnLocationTargetSet += OnTargetSet;
                m_actor.OnBuildingTargetSet += OnTargetSet;
            }

            public override void LoadFrom(string fileName, Properties properties)
            {
                string[] split = fileName.Split('.');
                fileName = split[0];

                string dataPath = EndlessExpedition.Serialization.Properties.dataRootPath + p_folder + "/" + fileName + ".png";

                m_pixWidth = properties.Get<int>("pixWidth");
                m_pixHeight = properties.Get<int>("pixHeight");

                Texture2D tex = new Texture2D(m_pixWidth, m_pixHeight);
                tex.LoadImage(File.ReadAllBytes(dataPath));

                p_textureData.Add(tex.GetPixels(0, 0, m_pixWidth, m_pixHeight));
            }

            public override Texture2D texture(int variant = 0)
            {
                Texture2D tex = new Texture2D(m_pixWidth, m_pixHeight);
                tex.filterMode = FilterMode.Point;
                tex.SetPixels(p_textureData[variant]);
                tex.Apply();
                return tex;
            }

            //LineRenderer
            public virtual void InitializeActiveGraphics()
            {
                Color lineColor = new Color(0.25f, 0.25f, 0.25f, 0.55f);
                m_lineRenderer = m_actor.gameObject.AddComponent<LineRenderer>();
                m_lineRenderer.material = Resources.Load<Material>("Materials/ActorLine");
                m_lineRenderer.SetVertexCount(2);
                m_lineRenderer.SetColors(lineColor, lineColor);
                m_lineRenderer.enabled = false;
                m_lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                m_lineRenderer.receiveShadows = false;
                m_lineRenderer.SetWidth(0.075f, 0.075f);
                m_actor.gameObject.GetComponent<Renderer>().sortingOrder += 1;
            }
            public void UpdateMovementLine()
            {
                if (m_lineRenderer == null)
                    return;
                
                if(m_actor.targetPosition != Vector3.zero)
                {
                    m_lineRenderer.SetPosition(0, m_actor.gameObject.transform.position);
                    m_lineRenderer.SetPosition(1, m_actor.targetPosition);
                }
            }
            public void ToggleMovementLine(bool state)
            {
                m_lineRenderer.enabled = state;
            }
            public void SetLineColor(Color color)
            {
                m_lineRenderer.SetColors(color, color);
            }
            public void SetLinePositions(Vector3[] positions)
            {
                m_lineRenderer.SetPositions(positions);
            }

            private void OnTargetSet(Entity entity, Vector3 target)
            {
                ToggleMovementLine(true);
            }
            private void OnTargetSet(Entity entity, Building target)
            {
                ToggleMovementLine(true);
            }
            private void OnTargetReached(Entity entity, Vector3 target)
            {
                ToggleMovementLine(false);
            }
            private void OnTargetReached(Entity entity, Building target)
            {
                ToggleMovementLine(false);
            }

            public LineRenderer LineRenderer
            {
                get
                {
                    return m_lineRenderer;
                }
            }
        }
    }
}
