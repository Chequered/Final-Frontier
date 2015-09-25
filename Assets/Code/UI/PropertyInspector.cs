using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using FinalFrontier.Serialization;

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
            private Properties _inspectingProperties;
            private CanvasGroup _group;
            private Text[] _propertiesTexts;
            private List<GameObject> _texts;
            private GameObject _textPrefab;
            private InspectingType _inspectingType;

            public void OnStart()
            {
                _group = GetComponent<CanvasGroup>();
                _texts = new List<GameObject>();
                _textPrefab = Resources.Load("UI/PropertyInspectorLine") as GameObject;
            }

            public void SetInspectingProperty(Properties properties)
            {
                Properties oldRef = _inspectingProperties;
                _inspectingProperties = properties;
                EnableGroup();
                if (oldRef != properties)
                    WriteProperties();

                if (properties.Get<string>("type") == "terrainTile")
                    _inspectingType = InspectingType.Tile;
                else if (properties.Get<string>("type") == "prop")
                    _inspectingType = InspectingType.Entity;
            }

            public void Close()
            {
                _inspectingProperties = null;
                HideGroup();
            }

            private void WriteProperties()
            {
                for (int i = 0; i < _texts.Count; i++)
                {
                    Destroy(_texts[i]);
                }
                _texts.Clear();

                float startX = 5;
                float startY = 50;
                int amount = _inspectingProperties.amount;
                int height = 14;
                int margin = 4;

                for (int i = 0; i < amount; i++)
                {
                    GameObject p = GameObject.Instantiate(_textPrefab);
                    p.GetComponent<RectTransform>().SetParent(this.transform);
                    p.GetComponent<RectTransform>().anchoredPosition = new Vector2(startX, (startY + (height + margin) * i) * -1);
                    p.GetComponent<RectTransform>().sizeDelta = new Vector2(GetComponent<RectTransform>().sizeDelta.x - startX * 2, 30);
                    p.GetComponent<Text>().text = "<b>" + _inspectingProperties.GetAll()[i].Key + "</b> : " + _inspectingProperties.GetAll()[i].Value;
                    _texts.Add(p);
                }
            }

            private void EnableGroup()
            {
                _group.alpha = 1;
                _group.interactable = true;
                _group.blocksRaycasts = true;
            }

            private void HideGroup()
            {
                _group.alpha = 0;
                _group.interactable = true;
                _group.blocksRaycasts = true;
            }

            public bool isOpen
            {
                get
                {
                    if (_group.alpha > 0)
                        return true;
                    return false;
                }
            }

            public InspectingType inspectingType
            {
                get
                {
                    return _inspectingType;
                }
            }
        }
    }
}
