using UnityEngine;

using EndlessExpedition.Managers;

namespace EndlessExpedition
{
    namespace Entities
    {
        namespace BehvaiourScripts
        {
            public class BuildingLanding : EntityBehaviourScript
            {
                private float m_startTime;
                private float m_journeyLength;
                private float m_speed = 50.5f;
                private Vector3 m_startMarker;
                private Vector3 m_destinationMarker;
                private float m_particleLifeTime;
                private GameObject m_particleObject;
                private bool m_landed = false;

                public BuildingLanding() : base("buildingLanding")
                {

                }

                public override void OnStart(Entity entity)
                {
                    m_startTime = Time.time;
                    m_destinationMarker = entity.gameObject.transform.position;
                    m_startMarker = entity.gameObject.transform.position - new Vector3(0, 0, 100f);
                    m_journeyLength = Vector3.Distance(m_startMarker, m_destinationMarker);

                    entity.gameObject.transform.Translate(new Vector3(0, 0, -25));
                    entity.gameObject.GetComponent<Renderer>().sortingOrder += 50;
                }

                public override void OnTick(Entity entity)
                {

                }

                public override void OnUpdate(Entity entity)
                {
                    if (entity.gameObject.transform.position.z < -0.15f)
                    {
                        if(!m_landed)
                        {
                            float distCovered = (Time.time - m_startTime) * m_speed;
                            float fracJourney = distCovered / m_journeyLength;
                            entity.gameObject.transform.position = Vector3.Lerp(m_startMarker, m_destinationMarker, fracJourney);
                        }
                    }
                    else
                    {
                        if(!m_landed)
                        {
                            m_landed = true;
                            m_particleObject = GameObject.Instantiate(Resources.Load("Particles/Landing") as GameObject,
                                entity.gameObject.transform.position,
                                Quaternion.identity) as GameObject;
                            m_particleLifeTime = m_particleObject.GetComponent<ParticleSystem>().duration + Time.time;
                            m_particleObject.layer = ManagerInstance.Get<EntityManager>().LayerForEntityType(entity.properties.Get<string>("type"));
                            entity.gameObject.GetComponent<Renderer>().sortingOrder -= 50;
                            (entity as Building).OnBuild();
                        }
                        if (Time.time > m_particleLifeTime)
                        {
                            RemoveFromEntity(entity);
                            ManagerInstance.Get<EntityManager>().UnregisterEntityBehaviourScript(this);
                            m_particleObject.AddComponent<GameObjectDestroyer>().Destroy();
                        }
                    }
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
