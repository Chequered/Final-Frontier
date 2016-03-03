using System.Collections.Generic;

using UnityEngine;

using EndlessExpedition.UI;
using EndlessExpedition.Terrain;
using EndlessExpedition.Managers.Base;
using EndlessExpedition.Items;
using EndlessExpedition.Entities;

namespace EndlessExpedition
{
    namespace Managers
    {
        public enum InputPressType
        {
            Down,
            Up,
            Held,
            Double
        }

        public class InputManager : ManagerBase
        {
            public static float doubleClickTimeout = 0.4f;
            public static bool isTypingInInputField;

            //key
            public delegate void OnKeyPressedHandler();
            private List<KeyValuePair<KeyCode, OnKeyPressedHandler>> m_keyPressDownEvents;
            private List<KeyValuePair<KeyCode, OnKeyPressedHandler>> m_keyPressUpEvents;
            private List<KeyValuePair<KeyCode, OnKeyPressedHandler>> m_keyPressHeldEvents;

            //click
            public delegate void OnClickPressHandler();
            private List<KeyValuePair<int, OnClickPressHandler>> m_clickedDownEvents;
            private List<KeyValuePair<int, OnClickPressHandler>> m_clickedUpEvents;
            private List<KeyValuePair<int, OnClickPressHandler>> m_clickedHeldEvents;
            private List<KeyValuePair<int, OnClickPressHandler>> m_clickedDoubleEvents;

            private List<float> m_clickTimers;

            //items
            private ItemStackTransferInfo m_currentMovingItemStack;
            private static bool m_isMouseOverItemSlot;

            //Entities
            private Entity m_selectedEntity;

            public void AddEventListener(InputPressType type, KeyCode keyCode, OnKeyPressedHandler eventHandler)
            {
                KeyValuePair<KeyCode, OnKeyPressedHandler> pair = new KeyValuePair<KeyCode,OnKeyPressedHandler>(keyCode, eventHandler);
                switch (type)
                {
                    case InputPressType.Down:
                        m_keyPressDownEvents.Add(pair);
                        break;
                    case InputPressType.Up:
                        m_keyPressUpEvents.Add(pair);
                        break;
                    case InputPressType.Held:
                        m_keyPressHeldEvents.Add(pair);
                        break;
                }
            }
            public void AddEventListener(InputPressType type, int mouseButton, OnClickPressHandler eventHandler)
            {
                KeyValuePair<int, OnClickPressHandler> pair = new KeyValuePair<int, OnClickPressHandler>(mouseButton, eventHandler);
                switch (type)
                {
                    case InputPressType.Down:
                        m_clickedDownEvents.Add(pair);
                        break;
                    case InputPressType.Up:
                        m_clickedUpEvents.Add(pair);
                        break;
                    case InputPressType.Held:
                        m_clickedHeldEvents.Add(pair);
                        break;
                    case InputPressType.Double:
                        m_clickedDoubleEvents.Add(pair);
                        m_clickTimers.Add(0);
                        break;
                }
            }

            public void RemoveEventListener(InputPressType type, KeyCode keyCode, OnKeyPressedHandler eventHandler)
            {
                switch (type)
                {
                    case InputPressType.Down:
                        for (int i = 0; i < m_keyPressDownEvents.Count; i++)
                        {
                            if (m_keyPressDownEvents[i].Key == keyCode)
                                if (m_keyPressDownEvents[i].Value == eventHandler)
                                {
                                    m_keyPressDownEvents.RemoveAt(i);
                                    return;
                                }
                        }
                        break;
                    case InputPressType.Up:
                        for (int i = 0; i < m_keyPressUpEvents.Count; i++)
                        {
                            if (m_keyPressUpEvents[i].Key == keyCode)
                                if (m_keyPressUpEvents[i].Value == eventHandler)
                                {
                                    m_keyPressUpEvents.RemoveAt(i);
                                    return;
                                }
                        }
                        break;
                    case InputPressType.Held:
                        for (int i = 0; i < m_keyPressHeldEvents.Count; i++)
                        {
                            if (m_keyPressHeldEvents[i].Key == keyCode)
                                if (m_keyPressHeldEvents[i].Value == eventHandler)
                                {
                                    m_keyPressHeldEvents.RemoveAt(i);
                                    return;
                                }
                        }
                        break;
                }
            }
            public void RemoveEventListener(InputPressType type, int mouseButton, OnClickPressHandler eventHandler)
            {
                List<KeyValuePair<int, OnClickPressHandler>> list = null;
                switch (type)
                {
                    case InputPressType.Down:
                        list = m_clickedDownEvents;
                        break;
                    case InputPressType.Up:
                        list = m_clickedUpEvents;
                        break;
                    case InputPressType.Held:
                        list = m_clickedHeldEvents;
                        break;
                }
                if(list != null)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (list[i].Key == mouseButton)
                        {
                            if (list[i].Value == eventHandler)
                            {
                                list.RemoveAt(i);
                                return;
                            }
                        }
                    }
                }
            }

            public override void OnStart()
            {

            }

            public override void OnTick()
            {
                foreach (KeyValuePair<KeyCode, OnKeyPressedHandler> keyEvent in m_keyPressDownEvents)
                {
                    if(Input.GetKeyDown(keyEvent.Key))
                    {
                        keyEvent.Value();
                    }
                }
                foreach (KeyValuePair<KeyCode, OnKeyPressedHandler> keyEvent in m_keyPressUpEvents)
                {
                    if (Input.GetKeyUp(keyEvent.Key))
                    {
                        keyEvent.Value();
                    }
                }
                foreach (KeyValuePair<KeyCode, OnKeyPressedHandler> keyEvent in m_keyPressHeldEvents)
                {
                    if (Input.GetKey(keyEvent.Key))
                    {
                        keyEvent.Value();
                    }
                }

                foreach (KeyValuePair<int, OnClickPressHandler> keyEvent in m_clickedDownEvents)
                {
                    if (Input.GetMouseButtonDown(keyEvent.Key))
                    {
                        keyEvent.Value();
                    }
                }
                foreach (KeyValuePair<int, OnClickPressHandler> keyEvent in m_clickedUpEvents)
                {
                    if (Input.GetMouseButtonUp(keyEvent.Key))
                    {
                        keyEvent.Value();
                    }
                }
                foreach (KeyValuePair<int, OnClickPressHandler> keyEvent in m_clickedHeldEvents)
                {
                    if (Input.GetMouseButton(keyEvent.Key))
                    {
                        keyEvent.Value();
                    }
                }

                for (int i = 0; i < m_clickedDoubleEvents.Count; i++)
                {
                    if (Input.GetMouseButtonUp(m_clickedDoubleEvents[i].Key))
                    {
                        if (m_clickTimers[i] != 0)
                            m_clickTimers[i] = Time.time;
                        else if (m_clickTimers[i] + doubleClickTimeout < Time.time)
                        {
                            m_clickedDoubleEvents[i].Value();
                            m_clickTimers[i] = 0;
                        }
                        else
                        {
                            m_clickTimers[i] = 0;
                        }
                    }
                }
            }

            public override void OnUpdate()
            {
                if (Input.GetMouseButtonUp(0) && GameManager.gameState == GameState.Playing && !isMouseOverUI)
                {
                    Transform hitTransform = null;

                    RaycastHit hit;
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    
                    if (Physics.Raycast(ray, out hit, 500, LayerMask.GetMask("Actor")))
                    {
                        hitTransform = hit.transform;
                    }
                    else if (Physics.Raycast(ray, out hit, 500, LayerMask.GetMask("Building")))
                    {
                        hitTransform = hit.transform;
                    }
                    else if (Physics.Raycast(ray, out hit, 500, LayerMask.GetMask("Prop")))
                    {
                        hitTransform = hit.transform;
                    }

                    if (hitTransform != null)
                    {
                        if (m_selectedEntity != null)
                            m_selectedEntity.OnDeselect();
                        hitTransform.GetComponent<EntityCollision>().OnClick();
                        m_selectedEntity = hitTransform.GetComponent<EntityCollision>().entity;
                    }
                    else if (m_selectedEntity != null)
                    {
                        m_selectedEntity.OnDeselect();
                        m_selectedEntity = null;
                    }
                }
            }

            public override void OnLoad()
            {
                m_keyPressDownEvents = new List<KeyValuePair<KeyCode, OnKeyPressedHandler>>();
                m_keyPressUpEvents = new List<KeyValuePair<KeyCode, OnKeyPressedHandler>>();
                m_keyPressHeldEvents = new List<KeyValuePair<KeyCode, OnKeyPressedHandler>>();

                m_clickedDownEvents = new List<KeyValuePair<int, OnClickPressHandler>>();
                m_clickedUpEvents = new List<KeyValuePair<int, OnClickPressHandler>>();
                m_clickedHeldEvents = new List<KeyValuePair<int, OnClickPressHandler>>();
                m_clickedDoubleEvents = new List<KeyValuePair<int, OnClickPressHandler>>();
                m_clickTimers = new List<float>();
            }

            public override void OnExit()
            {

            }

            public TerrainTile currentMouseOverTile
            {
                get
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;
                    LayerMask mask = LayerMask.GetMask("Terrain");

                    if (Physics.Raycast(ray, out hit, 500, mask))
                    {
                        Vector3 mouseHit = hit.transform.InverseTransformPoint(hit.point);
                        int x = (int)Mathf.Floor(TerrainChunk.SIZE * (mouseHit.x + 0.5f));
                        int y = (int)Mathf.Floor(TerrainChunk.SIZE * (mouseHit.y + 0.5f));

                        if (Managers.TerrainManager.isLocationValid(x, y) && hit.transform.GetComponent<TerrainChunkCollision>() != null)
                            return hit.transform.GetComponent<TerrainChunkCollision>().chunkRef.tiles[x, y];
                    }
                    return null;
                }
            }

            public ItemStackTransferInfo currentMovingItemstack
            {
                get
                {
                    return m_currentMovingItemStack;
                }
                set
                {
                    m_currentMovingItemStack = value;
                }
            }

            public static bool isMouseOverUI
            {
                get
                {
                    return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
                }
            }
            public static bool isMouseOverItemSlot
            {
                get
                {
                    return m_isMouseOverItemSlot;
                }
                set
                {
                    m_isMouseOverItemSlot = value;
                }
            }
        }
    }
}
