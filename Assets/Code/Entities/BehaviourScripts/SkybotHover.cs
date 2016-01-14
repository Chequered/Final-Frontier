using UnityEngine;

using EndlessExpedition.Managers;

namespace EndlessExpedition
{
    namespace Entities
    {
        namespace BehvaiourScripts
        {
            public class SkybotHover : EntityBehaviourScript
            {
                //Properties
                private float m_floatMargin = 0.25f;
                private float m_speed = 0.15f;
                private float m_heightinFight = 1f;

                private Vector3 m_centerPos;
                private int m_direction;
                private bool m_enabled;
                private float m_minHeight;
                private float m_maxHeight;

                //Lerp vars
                private float m_startTime;
                private float m_journeyLength;
                private Vector3 m_startMarker;
                private Vector3 m_destinationMarker;

                public SkybotHover() : base("skybotHover")
                {
                    m_direction = 1;
                }

                public override void OnStart(Entity entity)
                {
                    SetupNextHover(entity);
                    m_enabled = true;
                    m_centerPos = entity.gameObject.transform.position;

                    if (entity.GetType() == typeof(Actor))
                        ((Actor)entity).OnLocationTargetSet += StartFlight;
                }

                public override void OnTick(Entity entity)
                {

                }

                public override void OnUpdate(Entity entity)
                {
                    if (!m_enabled)
                        return;

                    SyncXYPosition(entity);

                    if (Vector3.Distance(entity.gameObject.transform.position, m_destinationMarker) <= 0)
                        SetupNextHover(entity);
                    
                        float distCovered = (Time.time - m_startTime) * m_speed;
                        float fracJourney = distCovered / m_journeyLength;
                        entity.gameObject.transform.position = Vector3.Lerp(m_startMarker, m_destinationMarker, fracJourney);
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

                private void StartFlight(Entity entity, Vector3 destination)
                {
                    m_maxHeight += m_heightinFight;
                }

                private void SyncXYPosition(Entity entity)
                {
                    float targetHeight = m_destinationMarker.z;
                    float startHeight = m_startMarker.z;

                    m_destinationMarker = new Vector3(entity.gameObject.transform.position.x, entity.gameObject.transform.position.y, targetHeight);
                    m_startMarker = new Vector3(entity.gameObject.transform.position.x, entity.gameObject.transform.position.y, startHeight);
                }
                private void SetupNextHover(Entity entity)
                {
                    m_direction *= -1; //we switch our direction

                    m_startTime = Time.time;
                    m_destinationMarker = entity.gameObject.transform.position + new Vector3(0, 0, m_floatMargin * m_direction);
                    m_startMarker = entity.gameObject.transform.position;
                    m_journeyLength = Vector3.Distance(m_startMarker, m_destinationMarker);

                    if(m_direction == 1)
                    {
                        //m_destinationMarker.z -= m_maxHeight;
                    }
                    else
                    {

                    }
                }
            }
        }
    }
}
