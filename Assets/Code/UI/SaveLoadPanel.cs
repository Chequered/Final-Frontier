using UnityEngine;
using UnityEngine.UI;

using FinalFrontier.Serialization;
using FinalFrontier.Managers;
using FinalFrontier.Terrain;

public enum SaveLoadMode
{
    Save,
    Load
}
namespace FinalFrontier
{
    namespace UI
    {
        public class SaveLoadPanel : MonoBehaviour
        {

            [HideInInspector]
            public SaveLoadMode mode;

            public InputField saveInputField;
            public Text titleText;
            public Text buttonText;

            private string m_worldName;
            private CanvasGroup m_canvasGroup;

            private void Start()
            {
                ((Text)saveInputField.placeholder).text = "Enter World Name..";
                m_canvasGroup = GetComponent<CanvasGroup>();
            }

            public void SetMode(SaveLoadMode mode)
            {
                this.mode = mode;
                switch (mode)
                {
                    case SaveLoadMode.Save:
                        titleText.text = "Save World";
                        buttonText.text = "Save";
                        break;
                    case SaveLoadMode.Load:
                        titleText.text = "Load World";
                        buttonText.text = "Load";
                        break;
                }
            }

            public void SetVisibility(bool state)
            {
                if (state)
                    m_canvasGroup.alpha = 1;
                else
                    m_canvasGroup.alpha = 0;

                m_canvasGroup.blocksRaycasts = state;
            }

            public void Submit()
            {
                switch (mode)
                {
                    case SaveLoadMode.Save:
                        Save();
                        break;
                    case SaveLoadMode.Load:
                        Load();
                        break;
                }
                SetVisibility(false);
            }

            public void Save()
            {
                //Properties worldProperties = ManagerInstance.Get<TerrainManager>().worldProperties;
                //worldProperties.Save();
            }

            public void Load()
            {
                Properties worldProperties = new Properties("saves");
                worldProperties.Load(saveInputField.text + ".xml");

                Destroy(GameObject.Find("World"));

                ManagerInstance.Get<TerrainManager>().SetWorldProperties(worldProperties);
                //ManagerInstance.Get<TerrainManager>().OnStart();
            }
        }
    }
}
