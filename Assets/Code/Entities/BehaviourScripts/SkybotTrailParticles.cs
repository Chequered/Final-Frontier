using UnityEngine;

using EndlessExpedition.Managers;

namespace EndlessExpedition
{
    namespace Entities
    {
        namespace BehvaiourScripts
        {
            public class SkybotTrailParticles : EntityBehaviourScript
            {
                private ParticleSystem m_particleSystem;


                public SkybotTrailParticles() : base("skybotParticles")
                {

                }

                public override void OnStart(Entity entity)
                {
                    GameObject particleObj = GameObject.Instantiate(Resources.Load("Particles/SkybotTrail")) as GameObject;
                    particleObj.transform.SetParent(entity.gameObject.transform);
                    particleObj.transform.localPosition = Vector3.zero;

                    m_particleSystem = particleObj.GetComponent<ParticleSystem>();
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
            }
        }
    }
}
