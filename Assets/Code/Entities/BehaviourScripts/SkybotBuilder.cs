using UnityEngine;

using DG.Tweening;
using EndlessExpedition.Entities.Construction;
using EndlessExpedition.Graphics;

namespace EndlessExpedition
{
    namespace Entities
    {
        namespace BehvaiourScripts
        {
            public class SkybotBuilder : EntityBehaviourScript
            {
                private Actor m_skybot;
                private Building m_target;
                private ConstructionNode m_targetNode;

                private Vector3 m_lineOriginalPosition;
                private Vector3 m_lineTargetPosition;
                private float m_lerpProgress;

                private Timer m_buildTimer;
                private float m_buildSpeed;

                public SkybotBuilder() : base("skybotBuilder")
                {

                }

                public override void OnDeselect(Entity entity, bool state)
                {

                }

                public override void OnDestroy(Entity entity)
                {

                }

                public override void OnSelect(Entity entity, bool state)
                {

                }

                public override void OnStart(Entity entity)
                {
                    if (entity.GetType() == typeof(Actor))
                        m_skybot = entity as Actor;
                    
                    m_skybot.OnBuildingTargetReached += OnBuildingReached;

                    if (m_skybot != null)
                    {
                        if (m_skybot.properties.Has("buildSpeed"))
                        {
                            m_buildTimer = new Timer(m_skybot.properties.Get<float>("buildSpeed"));
                        }
                        else
                        {
                            m_buildTimer = new Timer(0.05f);
                        }
                        if (m_skybot.properties.Has("buildStrength"))
                        {
                            m_buildSpeed = m_skybot.properties.Get<float>("buildStrength");
                        }
                        else
                        {
                            m_buildSpeed = 1f;
                        }
                    }
                }

                public override void OnTick(Entity entity)
                {

                }

                public override void OnUpdate(Entity entity)
                {
                    if (m_target == null)
                        return;
                    if (m_target.IsBuilt)
                        return;
                    
                    if(m_skybot.CurrentAction == ActionType.Build)
                    {
                        if (m_buildTimer.IsDone)
                        {
                            if (m_targetNode.IsFinished)
                            {
                                if (!FindNewTargetNode())
                                {
                                    m_buildTimer.Stop();
                                    m_target = null;
                                    (m_skybot.GetGraphics() as ActorGraphics).ToggleMovementLine(false);
                                    (m_skybot.GetGraphics() as ActorGraphics).LineRenderer.DOKill();
                                    (m_skybot.GetGraphics() as ActorGraphics).SetLineColor(Color.white);
                                    return;
                                }
                            }
                            m_targetNode.ProgressConstruction(m_buildSpeed);
                            m_buildTimer.Start();

                            if (m_lineTargetPosition == Vector3.zero)
                                m_lineOriginalPosition = entity.unityPosition;
                            else
                                m_lineOriginalPosition = m_lineTargetPosition;

                            m_lineTargetPosition = m_targetNode.WorldPosition + new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f)) + new Vector2(0.5f, 0.5f);
                            m_lerpProgress = 0;

                            DOTween.To(SetLerpProgress, 0f, 1f, m_buildTimer.Duration);

                            (m_skybot.GetGraphics() as ActorGraphics).ToggleMovementLine(true);
                            (m_skybot.GetGraphics() as ActorGraphics).LineRenderer.DOColor(new Color2(Color.red, Color.red * 1.15f), new Color2(Color.red * 0.85f, Color.red * 1.25f), m_buildSpeed);
                            (m_skybot.GetGraphics() as ActorGraphics).SetLinePositions(new Vector3[] { m_skybot.unityPosition, m_lineTargetPosition });
                        }
                        else
                        {
                            (m_skybot.GetGraphics() as ActorGraphics).SetLinePositions(new Vector3[] { m_skybot.unityPosition, Vector3.Lerp(m_lineOriginalPosition, m_lineTargetPosition, m_lerpProgress) });
                        }
                    }
                }

                private void SetLerpProgress(float progress)
                {
                    m_lerpProgress = progress;
                }

                private bool FindNewTargetNode()
                {
                    if (m_target.ConstructionHeader != null)
                    {
                        ConstructionNode node = m_target.ConstructionHeader.GetClosestUnfishiedNode(m_skybot.unityPosition);
                        if (node != null)
                        {
                            m_targetNode = node;
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    return false;
                }
                private void OnBuildingReached(Entity entity, Building target)
                {
                    if (target.IsBuilt)
                        return;

                    m_buildTimer.Start();

                    m_target = target;
                    if (m_target.ConstructionHeader != null)
                        m_targetNode = m_target.ConstructionHeader.GetClosestUnfishiedNode(m_skybot.unityPosition);
                }
            }
        }
    }
}
