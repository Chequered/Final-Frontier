using System;
using System.Collections.Generic;

using UnityEngine;

using EndlessExpedition.Managers;

namespace EndlessExpedition
{
    namespace Entities
    {
        public abstract class EntityBehaviourScript
        {
            //behaviour vars
            private bool m_enabled = true;
            private List<Entity> m_attachedTo;

            //abstract vars
            protected string p_behaviourName = "Unnamed Behaviour";

            public EntityBehaviourScript(string behaviourName)
            {
                p_behaviourName = behaviourName;
                m_attachedTo = new List<Entity>();

                //register our script
                ManagerInstance.Get<EntityManager>().RegisterEntityBehaviourScript(this);
            }

            public void AttachToEntity(Entity entity)
            {
                entity.OnTickEvent += OnTick;
                entity.OnUpdateEvent += OnUpdate;
                entity.OnSelectEvent += OnSelect;
                entity.OnDeselectEvent += OnDeselect;
                entity.OnDestroyEvent += OnDestroy;
                m_attachedTo.Add(entity);

                if (entity.properties.Has("behaviourScripts"))
                {
                    string scripts = entity.properties.Get<string>("behaviourScripts");
                    scripts += "\n " + p_behaviourName;
                    entity.properties.Set("behaviourScripts", scripts);
                }
                else
                {
                    entity.properties.Set("behaviourScripts", "\n" + p_behaviourName);
                }

                OnStart(entity);
            }

            public void RemoveFromEntity(Entity entity)
            {
                if (m_attachedTo.Contains(entity))
                {
                    entity.OnStartEvent -= OnStart;
                    entity.OnTickEvent -= OnTick;
                    entity.OnUpdateEvent -= OnUpdate;
                    entity.OnSelectEvent -= OnSelect;
                    entity.OnDeselectEvent -= OnDeselect;
                    entity.OnDestroyEvent -= OnDestroy;
                    m_attachedTo.Remove(entity);

                    string[] splitScripts = entity.properties.Get<string>("behaviourScripts").Split(new char[] {'\n'});
                    string newScriptsText = "";
                    for (int i = 0; i < splitScripts.Length; i++)
                    {
                        if (splitScripts[i] != p_behaviourName)
                            newScriptsText += "\n" + splitScripts[i];
                    }

                    entity.properties.Set("behaviourScripts", newScriptsText);
                }
                else
                {
                    Debug.LogError("You're trying to remove a behaviour script from an object that it is not attached to.");
                }
            }

            //abstract methods
            public abstract void OnStart(Entity entity);
            public abstract void OnTick(Entity entity);
            public abstract void OnUpdate(Entity entity);
            public abstract void OnSelect(Entity entity, bool state);
            public abstract void OnDeselect(Entity entity, bool state);
            public abstract void OnDestroy(Entity entity);

            //setters
            public void SetActive(bool state)
            {
                m_enabled = state;
            }

            //getters
            public string behaviourName
            {
                get
                {
                    return p_behaviourName;
                }
            }

            public bool isEnabled
            {
                get
                {
                    return m_enabled;
                }
            }
        }
    }
}