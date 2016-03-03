using UnityEngine;

using EndlessExpedition.Managers;

using DG.Tweening;

namespace EndlessExpedition
{
    namespace Entities
    {
        namespace BehvaiourScripts
        {
            public class SkybotHover : EntityBehaviourScript
            {
                //Properties
                private float m_hoverHeight = 0.45f;
                private float m_duration = 1.55f;

                private Tweener tweener;

                public SkybotHover() : base("skybotHover")
                {

                }

                public override void OnStart(Entity entity)
                {
                    if (entity.GetType() == typeof(Actor))
                        ((Actor)entity).OnLocationTargetSet += StartHover;

                    if (entity.GetType() == typeof(Actor))
                        ((Actor)entity).OnBuildingTargetSet += StartHover;
                }

                public override void OnTick(Entity entity)
                {

                }
                public override void OnUpdate(Entity entity)
                {
                }
                public override void OnSelect(Entity entity, bool state)
                {

                }
                public override void OnDeselect(Entity entity, bool state)
                {

                }
                public override void OnDestroy(Entity entity)
                {

                }

                private void StartHover(Entity entity, Building destination)
                {
                    StartHover(entity, destination.unityPosition);
                }
                private void StartHover(Entity entity, Vector3 destination)
                {
                    if (tweener != null)
                        tweener.Kill();

                    tweener = entity.gameObject.transform.DOMoveZ(-m_hoverHeight, m_duration).SetLoops(-1, LoopType.Yoyo);
                }
            }
        }
    }
}
