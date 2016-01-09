using System.IO;

using UnityEngine;
using UnityEngine.UI;

using FinalFrontier.Serialization;

namespace FinalFrontier
{
    namespace UI.MainMenu
    {
        public class SaveGameList : MonoBehaviour
        {
            public GameObject loadParent;

            private void Start()
            {
                string[] saveGames = Directory.GetDirectories(Properties.saveRootPath);

                for (int i = 0; i < saveGames.Length; i++)
                {
                    GameObject button = GameObject.Instantiate(Resources.Load("UI/LoadGameButton") as GameObject);
                    button.transform.SetParent(loadParent.transform);

                    string[] splitFolder = saveGames[i].Split('/');
                    string saveFolder = splitFolder[splitFolder.Length - 1];

                    button.GetComponent<LoadGameButton>().folder = saveFolder;

                    RectTransform rect = button.GetComponent<RectTransform>();
                    rect.pivot = Vector2.zero;
                    rect.anchoredPosition = new Vector2(0, -30 + i * -30);
                    rect.sizeDelta = new Vector2(800, 30);

                    button.transform.FindChild("Text").GetComponent<Text>().text = saveGames[i];

                    string[] files = Directory.GetFiles(saveGames[i], "*.savegame");
                    bool hasTerrain = false, hasWorld = false, hasEntities = false;

                    for (int file = 0; file < files.Length; file++)
                    {
                        string[] split = files[file].Split('\\');
                        string dataFile = split[split.Length - 1];

                        if (dataFile == Savegame.SAVEGAME_WORLD)
                            hasWorld = true;
                        if (dataFile == Savegame.SAVEGAME_TERRAIN)
                            hasTerrain = true;
                        if (dataFile == Savegame.SAVEGAME_ENTITIES)
                            hasEntities = true;
                    }
                    
                    if (!hasWorld || !hasTerrain || !hasEntities)
                    {
                        //savegame is corrupt/missing data
                        button.transform.FindChild("Text").GetComponent<Text>().color = Color.red;
                        button.transform.FindChild("Text").GetComponent<Text>().text += " *CORRUPT*";
                    }
                }
            }
        }
    }
}
