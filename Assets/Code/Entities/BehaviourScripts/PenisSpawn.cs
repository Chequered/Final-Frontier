using UnityEngine;

using EndlessExpedition.Managers;

namespace EndlessExpedition
{
    namespace Entities
    {
        namespace BehvaiourScripts
        {
            public class PenisSpawn : EntityBehaviourScript
            {
                public PenisSpawn() : base("Penis Spawn")
                {

                }

                public override void OnStart(Entity entity)
                {
                    (entity as Actor).OnLocationTargetSet += SpawnWiener;
                }

                private void SpawnWiener(Entity entity, Vector3 loc)
                {
                    Debug.Log("hello");
                    Entity prefab = ManagerInstance.Get<EntityManager>().FindFromCache<Actor>(entity.Identity);
                    ManagerInstance.Get<EntityManager>().CreateEntity<Actor>(prefab, entity.tilePosition.x, entity.tilePosition.y);
                }

                public override void OnTick(Entity entity)
                {
                    throw new System.NotImplementedException();
                }

                public override void OnUpdate(Entity entity)
                {
                    throw new System.NotImplementedException();
                }

                public override void OnSelect(Entity entity, bool state)
                {
                    throw new System.NotImplementedException();
                }

                public override void OnDeselect(Entity entity, bool state)
                {
                    throw new System.NotImplementedException();
                }

                public override void OnDestroy(Entity entity)
                {
                    throw new System.NotImplementedException();
                }
            }
        }
    }
}
