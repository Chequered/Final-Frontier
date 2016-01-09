using System.Collections.Generic;

using UnityEngine;

using FinalFrontier.UI;
using FinalFrontier.Terrain;
using FinalFrontier.Managers.Base;

namespace FinalFrontier
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
                switch (type)
                {
                    case InputPressType.Down:
                        for (int i = 0; i < m_clickedDownEvents.Count; i++)
                        {
                            if (m_clickedDownEvents[i].Key == mouseButton)
                                if (m_clickedDownEvents[i].Value == eventHandler)
                                {
                                    m_clickedDownEvents.RemoveAt(i);
                                    return;
                                }
                        }
                        break;
                    case InputPressType.Up:
                        for (int i = 0; i < m_clickedUpEvents.Count; i++)
                        {
                            if (m_clickedUpEvents[i].Key == mouseButton)
                                if (m_clickedUpEvents[i].Value == eventHandler)
                                {
                                    m_clickedUpEvents.RemoveAt(i);
                                    return;
                                }
                        }
                        break;
                    case InputPressType.Held:
                        for (int i = 0; i < m_clickedHeldEvents.Count; i++)
                        {
                            if (m_clickedHeldEvents[i].Key == mouseButton)
                                if (m_clickedHeldEvents[i].Value == eventHandler)
                                {
                                    m_clickedHeldEvents.RemoveAt(i);
                                    return;
                                }
                        }
                        break;
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

                    if (Physics.Raycast(ray, out hit, LayerMask.GetMask("Terrain")))
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

            public static bool isMouseOverUI
            {
                get
                {
                    return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
                }
            }
        }
    }
}
