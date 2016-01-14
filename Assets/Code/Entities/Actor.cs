using System;
using System.Collections.Generic;

using UnityEngine;

using EndlessExpedition.Managers;
using EndlessExpedition.Graphics;
using EndlessExpedition.Terrain;

namespace EndlessExpedition
{
    namespace Entities
    {
        public class Actor : Entity
        {
            public const float TARGET_REACHED_MARGIN = 0.45f;

            private ActorGraphics m_graphics;

            private Vector3 m_targetPos;
            private Building m_targetBuilding;
            private float m_startZRotation;
            private float m_targetZRotation;

            private float m_movementSpeed;
            private float m_rotationSpeed;

            public delegate void LocationReachedEventHandler(Entity entity, Vector3 target);
            public delegate void BuildingReachedEventHandler(Entity entity, Building target);
            public delegate void LocationTargetSetEventHandler(Entity entity, Vector3 target);
            public delegate void BuildingTargetSetEventHandler(Entity entity, Building target);

            public LocationReachedEventHandler OnLocationTargetReached;
            public BuildingReachedEventHandler OnBuildingTargetReached;
            public LocationTargetSetEventHandler OnLocationTargetSet;
            public BuildingTargetSetEventHandler OnBuildingTargetSet;

            public override void OnStart()
            {
                base.OnStart();
                p_properties.Secure("identity", "unnamedActor");
                p_properties.Secure("type", "actor");

                #region Read properties
                if (p_properties.Has("movementSpeed"))
                    m_movementSpeed = p_properties.Get<float>("movementSpeed");
                else
                    m_movementSpeed = 5f;

                if (p_properties.Has("rotationSpeed"))
                    m_rotationSpeed = p_properties.Get<float>("rotationSpeed");
                else
                    m_rotationSpeed = 25f;
                #endregion

                m_graphics = new ActorGraphics(this);

                if (properties.Has("hasLight"))
                    if (properties.Get<bool>("hasLight"))
                        GenerateLight();

                (GetGraphics() as ActorGraphics).InitializeMovementLine();
            }

            public override void OnTick()
            {
                base.OnTick();
            }

            public override void OnUpdate()
            {
                base.OnUpdate();

                Quaternion newRotation = gameObject.transform.rotation;
                Vector3 newPosition = gameObject.transform.position;

                //Rotate our actor towars our target;
                newRotation = Quaternion.Euler(0, 0, m_targetZRotation);

                //Movement specific
                if (m_targetPos != Vector3.zero)
                {
                    newPosition = Vector3.MoveTowards(gameObject.transform.position, m_targetPos, m_movementSpeed * Time.deltaTime);
                    if (Vector2.Distance(gameObject.transform.position, m_targetPos) <= TARGET_REACHED_MARGIN)
                    {
                        m_targetPos = Vector3.zero;

                        if (OnLocationTargetReached != null)
                            OnLocationTargetReached(this, m_targetPos);
                    }
                }else if(m_targetBuilding != null)
                {
                    newPosition = Vector3.MoveTowards(gameObject.transform.position, m_targetBuilding.gameObject.transform.position, m_movementSpeed * Time.deltaTime);

                    if (Vector2.Distance(gameObject.transform.position, m_targetBuilding.gameObject.transform.position) <= TARGET_REACHED_MARGIN)
                    {
                        //destination reached
                        m_targetBuilding = null;

                        if (OnLocationTargetReached != null)
                            OnBuildingTargetReached(this, m_targetBuilding);
                    }
                }

                if(gameObject.transform.position != newPosition)
                {
                    gameObject.transform.rotation = newRotation;
                    (GetGraphics() as ActorGraphics).UpdateMovementLine();
                }

                gameObject.transform.position = newPosition;
            }

            public override void OnSelect()
            {
                base.OnSelect();
                ManagerInstance.Get<InputManager>().AddEventListener(InputPressType.Up, 1, MoveToMousePos);

                if(targetPosition != Vector3.zero)
                    (GetGraphics() as ActorGraphics).ToggleMovementLine(true);
            }

            public override void OnDeselect()
            {
                base.OnDeselect();
                ManagerInstance.Get<InputManager>().RemoveEventListener(InputPressType.Up, 1, MoveToMousePos);

                if (targetPosition != Vector3.zero)
                    (GetGraphics() as ActorGraphics).ToggleMovementLine(false);
            }

            private void MoveToMousePos()
            {
                TerrainTile target = ManagerInstance.Get<InputManager>().currentMouseOverTile;

                if(target != null)
                    GoToGamePos(target.gamePosition.x, target.gamePosition.y, gameObject.transform.position.z);
            }

            public override GraphicsBase GetGraphics()
            {
                return m_graphics;
            }

            public override void SetGraphics(GraphicsBase graphics)
            {
                m_graphics = graphics as ActorGraphics;
            }

            public override void GoToGamePos(float x, float y, float z, bool forceStatic = false)
            {
                base.GoToGamePos(x, y, z, forceStatic); //attempt static movement

                if (forceStatic)
                    return;

                m_targetBuilding = null;
                m_targetPos = new Vector3(x, y, z);

                Vector3 diff = gameObject.transform.position - m_targetPos;
                diff.Normalize();

                float zDeg = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
                m_startZRotation = gameObject.transform.rotation.z;
                m_targetZRotation = zDeg - 180;

                if (OnLocationTargetSet != null)
                    OnLocationTargetSet(this, m_targetPos);
            }

            public void GoToBuilding(Building building)
            {
                m_targetBuilding = building;
                m_targetPos = Vector3.zero;

                Vector3 diff = gameObject.transform.position - m_targetBuilding.gameObject.transform.position;
                diff.Normalize();

                float zDeg = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
                m_startZRotation = gameObject.transform.rotation.z;
                m_targetZRotation = zDeg;

                if (OnBuildingTargetSet != null)
                    OnBuildingTargetSet(this, building);
            }

            //Getters
            public Vector3 targetPosition
            {
                get
                {
                    if (m_targetBuilding != null)
                        return m_targetBuilding.unityPosition;
                    return m_targetPos;
                }
            }
        }
    }
}