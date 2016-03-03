using System;
using System.Collections.Generic;

using UnityEngine;

using EndlessExpedition.Managers;
using EndlessExpedition.Graphics;
using EndlessExpedition.Terrain;
using EndlessExpedition.UI;

using DG.Tweening;
using EndlessExpedition.Entities.BehvaiourScripts;

namespace EndlessExpedition
{
    namespace Entities
    {
        public enum ActionType
        {
            None,
            Move,
            Transfer,
            Build,
            Interact,
            Attack
        }

        public class Actor : Entity
        {
            public const float TARGET_REACHED_MARGIN = 1.15f;

            //Graphic vars
            private ActorGraphics m_graphics;
            
            //Movement active vars
            private Vector3 m_targetPos;
            private Building m_targetBuilding;
            private float m_startZRotation;
            private float m_targetZRotation;
            private ActionType m_actionType;

            //Movement stats
            private float m_movementSpeed;
            private float m_rotationSpeed;

            //Delegates
            public delegate void LocationReachedEventHandler(Entity entity, Vector3 target);
            public delegate void BuildingReachedEventHandler(Entity entity, Building target);
            public delegate void LocationTargetSetEventHandler(Entity entity, Vector3 target);
            public delegate void BuildingTargetSetEventHandler(Entity entity, Building target);
            public delegate void ActorActionTypeChangeEventHandler(Actor actor, ActionType oldType, ActionType newType);

            //Event Handlers
            public LocationReachedEventHandler OnLocationTargetReached;
            public BuildingReachedEventHandler OnBuildingTargetReached;
            public LocationTargetSetEventHandler OnLocationTargetSet;
            public BuildingTargetSetEventHandler OnBuildingTargetSet;
            public ActorActionTypeChangeEventHandler OnActionTypeChange;

            //Tweeners
            private Tweener movementTweener;
            private Tweener rotationTweener;

            #region Engine events

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

                (GetGraphics() as ActorGraphics).InitializeActiveGraphics();
                
                OnBuildingTargetSet += EndTransferMode;
                OnLocationTargetSet += EndTransferMode;

                //Move Actors to top
                gameObject.transform.Translate(0, 0, -0.05f);

                EntityPropertyDisplay display = new EntityPropertyDisplay();
                display.properties = p_properties;
                display.displaySize = new Vector2(200, 15);
                display.AddDisplay("Name: ", "displayName");
                display.AddDisplay("Type: ", "actorType");
                display.AddDisplay("Item Slots: ", "itemContainerSlots");
                display.AddDisplay("Movement Speed: ", "movementSpeed");
                display.AddDisplay("Rotation Speed: ", "rotationSpeed");
                display.menuName = properties.Get<string>("displayName") + ": Propety Menu";
                display.BuildUI();
                display.Toggle(false);
                ManagerInstance.Get<UIManager>().AddUI(display);
                p_UIGroup.AddUIElement(display);
            }

            public override void OnTick()
            {
                base.OnTick();
            }

            public override void OnUpdate()
            {
                base.OnUpdate();

                UpdateMovement();
                UpdateLights();
            }

            #region UpdateMethods
            private void UpdateMovement()
            {
                Vector3 newPosition = gameObject.transform.position;

                //Movement specific
                if (m_targetPos != Vector3.zero && CurrentAction == ActionType.Move) //Move to tile
                {
                    newPosition = Vector3.MoveTowards(gameObject.transform.position, m_targetPos, m_movementSpeed * Time.deltaTime);
                    if (Vector2.Distance(gameObject.transform.position, m_targetPos) <= TARGET_REACHED_MARGIN)
                    {
                        if (OnLocationTargetReached != null)
                            OnLocationTargetReached(this, m_targetPos);
                    }
                }
                else if (m_targetBuilding != null && CurrentAction == ActionType.Move) //Move to building
                {
                    newPosition = Vector3.MoveTowards(gameObject.transform.position, m_targetBuilding.gameObject.transform.position, m_movementSpeed * Time.deltaTime);

                    if (Vector2.Distance(gameObject.transform.position, m_targetBuilding.gameObject.transform.position) <= TARGET_REACHED_MARGIN)
                    {
                        //destination reached
                        if (OnLocationTargetReached != null)
                            OnBuildingTargetReached(this, m_targetBuilding);
                    }
                }

                if (gameObject.transform.position != newPosition)
                {
                    (GetGraphics() as ActorGraphics).UpdateMovementLine();
                }

                gameObject.transform.position = newPosition;
            }
            private void UpdateLights()
            {
                int hours = ManagerInstance.Get<SimulationManager>().currentHour;
                if (hours > 7 && hours < 17)
                    light.enabled = false;
                else if (hours >= 17)
                    light.enabled = true;
            }
            #endregion

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

            #endregion
            
            #region Graphics

            public override GraphicsBase GetGraphics()
            {
                return m_graphics;
            }

            public override void SetGraphics(GraphicsBase graphics)
            {
                m_graphics = graphics as ActorGraphics;
            }

            #endregion

            #region Orders

            private void MoveToMousePos()
            {
                TerrainTile target = ManagerInstance.Get<InputManager>().currentMouseOverTile;

                if (target != null)
                {
                    if (ManagerInstance.Get<BuildManager>().GetBuildingAt(target.x, target.y) != null)
                    {
                        GoToBuilding(ManagerInstance.Get<BuildManager>().GetBuildingAt(target.x, target.y));
                    }
                    else
                    {
                        GoToGamePos(target.gamePosition.x, target.gamePosition.y, gameObject.transform.position.z);
                    }
                }
            }

            public override void GoToGamePos(float x, float y, float z, bool forceStatic = false)
            {
                base.GoToGamePos(x, y, z, forceStatic); //attempt static movement

                if (forceStatic)
                    return;

                CurrentAction = ActionType.Move;

                Vector3 targetPos = new Vector3(x, y, z);

                //Prepare some info for our 2D rotation
                Vector3 diff = gameObject.transform.position - targetPos;
                diff.Normalize();

                float zDeg = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg - 180;
                m_startZRotation = gameObject.transform.rotation.z;
                m_targetZRotation = zDeg;

                //Store target position and rotation
                Quaternion newRotation = Quaternion.Euler(0, 0, m_targetZRotation);

                //Kill current tweens and start next ones
                if(movementTweener != null)
                    movementTweener.Kill();
                movementTweener = gameObject.transform.DOMove(targetPos, Vector3.Distance(gameObject.transform.position, targetPos) / m_movementSpeed).SetEase(Ease.InOutSine);

                if (rotationTweener != null)
                    rotationTweener.Kill();
                rotationTweener = gameObject.transform.DORotate(newRotation.eulerAngles, 0.85f, RotateMode.Fast);

                if (OnLocationTargetSet != null)
                    OnLocationTargetSet(this, m_targetPos);

                m_targetPos = targetPos;
                m_targetBuilding = null;
            }

            public void GoToBuilding(Building building)
            {
                m_targetBuilding = building;

                //Prepare some info for our 2D rotation
                Vector3 diff = gameObject.transform.position - m_targetBuilding.gameObject.transform.position;
                diff.Normalize();

                float zDeg = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg - 180;
                m_startZRotation = gameObject.transform.rotation.z;
                m_targetZRotation = zDeg;

                //Store target position and rotation
                Quaternion newRotation = Quaternion.Euler(0, 0, m_targetZRotation);

                //Kill current tweens and start next ones
                gameObject.transform.DOKill();
                gameObject.transform.DOMove(
                    m_targetBuilding.gameObject.transform.position, 
                    Vector3.Distance(gameObject.transform.position, m_targetBuilding.gameObject.transform.position) / m_movementSpeed)
                    .SetEase(Ease.InOutSine);
                gameObject.transform.DORotate(newRotation.eulerAngles, 0.85f, RotateMode.Fast);

                //---------ACTIONS
                if (building.IsBuilt && building.itemContainer != null && itemContainer != null)
                {
                    OnBuildingTargetReached -= StartTransferMode;
                    OnBuildingTargetReached += StartTransferMode;
                }
                else if(!building.IsBuilt && GetBehaviourScript<SkybotBuilder>() != null)
                {
                    OnBuildingTargetReached -= StartBuildMode;
                    OnBuildingTargetReached += StartBuildMode;
                }
                //----------------
                CurrentAction = ActionType.Move;

                if (OnBuildingTargetSet != null)
                    OnBuildingTargetSet(this, building);

                m_targetPos = Vector3.zero;
            }

#endregion

            #region Transfer UI/Mode

            private void StartTransferMode(Entity entity, Building target)
            {
                CurrentAction = ActionType.Transfer;

                ItemContainerDisplay otherInv = target.uiGroup.GetElement<ItemContainerDisplay>();

                otherInv.Toggle(true);
                otherInv.position = new Vector2(Screen.width - otherInv.windowSize.x, p_UIGroup.GetElement<ItemContainerDisplay>().windowSize.y);

                if (movementTweener != null)
                    movementTweener.Kill();

                OnDeselectEvent += CloseAllOtherItemDisplayContainers;
                OnBuildingTargetReached -= StartTransferMode;
            }

            private void StartBuildMode(Entity entity, Building target)
            {
                CurrentAction = ActionType.Build;

                if (movementTweener != null)
                    movementTweener.Kill();

                OnBuildingTargetReached -= StartBuildMode;
            }

            private void EndTransferMode(Entity entity, Vector3 target)
            {
                if (m_targetBuilding == null)
                    return;

                EndTransferMode(entity, m_targetBuilding);
            }

            private void EndTransferMode(Entity entity, Building target)
            {
                if (target == null)
                    return;

                CloseAllOtherItemDisplayContainers(this, false);

                OnDeselectEvent -= CloseAllOtherItemDisplayContainers;
            }

            private void CloseAllOtherItemDisplayContainers(Entity entity, bool selectionState)
            {
                IUI[] displays = ManagerInstance.Get<UIManager>().FindAll<ItemContainerDisplay>();

                for (int i = 0; i < displays.Length; i++)
                {
                    ItemContainerDisplay ICD = displays[i] as ItemContainerDisplay;
                    if (ICD == p_UIGroup.GetElement<ItemContainerDisplay>())
                        continue;

                    ICD.position = ICD.initialPosition;
                    ICD.Toggle(false);
                }
            }

            #endregion

            #region Properties
            public Vector3 targetPosition
            {
                get
                {
                    if (m_targetBuilding != null)
                        return m_targetBuilding.unityPosition;
                    return m_targetPos;
                }
            }
            public ActionType CurrentAction
            {
                get
                {
                    return m_actionType;
                }
                set
                {
                    if(m_actionType != value)
                    {
                        if (OnActionTypeChange != null)
                            OnActionTypeChange(this, m_actionType, value);

                        m_actionType = value;
                    }
                }
            }
            #endregion
        }
    }
}