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
                private ParticleSystem.EmissionModule m_emitter;

                public SkybotTrailParticles() : base("skybotParticles")
                {

                }

                public override void OnStart(Entity entity)
                {
                    GameObject prefab = Resources.Load<GameObject>("Particles/SkybotTrail");
                    Quaternion rotation = prefab.transform.localRotation;
                    GameObject particleObj = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
                    particleObj.transform.SetParent(entity.gameObject.transform);
                    particleObj.transform.localPosition = Vector3.zero;
                    particleObj.transform.localRotation = new Quaternion(0, 270, 0, 0);

                    m_particleSystem = particleObj.GetComponent<ParticleSystem>();
                    m_emitter = m_particleSystem.emission;

                    (entity as Actor).OnLocationTargetSet += OnTargetSet;
                    (entity as Actor).OnBuildingTargetSet += OnTargetSet;
                    (entity as Actor).OnLocationTargetReached += OnTargetReached;
                    (entity as Actor).OnBuildingTargetReached += OnTargetReached;
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

                private void OnTargetSet(Entity entity, Building target)
                {
                    StartEmmision();
                }
                private void OnTargetSet(Entity entity, Vector3 target)
                {
                    StartEmmision();
                }
                private void OnTargetReached(Entity entity, Vector3 target)
                {
                    StopEmission();
                }
                private void OnTargetReached(Entity entity, Building target)
                {
                    StopEmission();
                }

                private void StartEmmision()
                {
                    m_emitter.enabled = true;
                }
                private void StopEmission()
                {
                    m_emitter.enabled = false;
                }
            }
        }
    }
}
