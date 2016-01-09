using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using FinalFrontier.Serialization;
using FinalFrontier.Entities;
using FinalFrontier.Managers;

namespace FinalFrontier
{
    namespace UI
    {
        public enum InspectingType
        {
            None,
            Tile,
            Entity
        }

        public class PropertyInspector : MonoBehaviour
        {
            private Properties m_inspectingProperties;
            private Entity m_inspectingEntity;

            private CanvasGroup m_group;
            private Text[] m_propertiesTexts;
            private List<GameObject> m_texts;
            private GameObject m_textPrefab;
            private InspectingType m_inspectingType = InspectingType.Tile;

            public void OnStart()
            {
                m_group = GetComponent<CanvasGroup>();
                m_texts = new List<GameObject>();
                m_textPrefab = Resources.Load("UI/PropertyInspectorLine") as GameObject;
                ManagerInstance.Get<InputManager>().AddEventListener(InputPressType.Up, KeyCode.I, SwitchInspectMode);
            }
            
            public void SetInspectingEntity(Entity entity)
            {
                if (m_inspectingEntity == entity)
                    return;

                InspectProperties(entity.properties);

                m_inspectingEntity = entity;
                EnableGroup();
            }

            public void InspectProperties(Properties properties)
            {
                Properties oldRef = m_inspectingProperties;
                m_inspectingProperties = properties;

                EnableGroup();

                if (oldRef != m_inspectingProperties)
                    WriteProperties();

                if (m_inspectingProperties.Get<string>("type") == "terrainTile")
                    m_inspectingType = InspectingType.Tile;
                else if (m_inspectingProperties.Get<string>("type") == "prop")
                    m_inspectingType = InspectingType.Entity;
            }

            public void Close()
            {
                m_inspectingProperties = null;
                HideGroup();
            }

            private void WriteProperties()
            {
                for (int i = 0; i < m_texts.Count; i++)
                {
                    Destroy(m_texts[i]);
                }
                m_texts.Clear();

                float startX = 5;
                float startY = 50;
                int amount = m_inspectingProperties.amount;
                int height = 14;
                int margin = 4;

                for (int i = 0; i < amount; i++)
                {
                    GameObject p = GameObject.Instantiate(m_textPrefab);
                    p.GetComponent<RectTransform>().SetParent(this.transform);
                    p.GetComponent<RectTransform>().anchoredPosition = new Vector2(startX, (startY + (height + margin) * i) * -1);
                    p.GetComponent<RectTransform>().sizeDelta = new Vector2(GetComponent<RectTransform>().sizeDelta.x - startX * 2, 30);
                    p.GetComponent<Text>().text = "<b>" + m_inspectingProperties.GetAll()[i].Key + "</b> : " + m_inspectingProperties.GetAll()[i].Value;
                    m_texts.Add(p);
                }
            }

            private void EnableGroup()
            {
                //m_group.alpha = 1;
                //m_group.interactable = true;
                //m_group.blocksRaycasts = true;
            }

            private void HideGroup()
            {
                m_group.alpha = 0;
                m_group.interactable = true;
                m_group.blocksRaycasts = true;
            }

            private void SwitchInspectMode()
            {
                if (m_inspectingType == InspectingType.Tile)
                    m_inspectingType = InspectingType.Entity;
                else
                    m_inspectingType = InspectingType.Tile;
            }

            public bool isOpen
            {
                get
                {
                    if (m_group.alpha > 0)
                        return true;
                    return false;
                }
            }

            public InspectingType inspectingType
            {
                get
                {
                    return m_inspectingType;
                }
                set
                {
                    m_inspectingType = value;
                }
            }
        }
    }
}
