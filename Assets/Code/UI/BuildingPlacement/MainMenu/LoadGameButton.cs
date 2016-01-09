using UnityEngine;
using System.Collections;

using FinalFrontier.Serialization;

public class LoadGameButton : MonoBehaviour {

    public string folder;

	public void Submit()
    {
        Savegame save = new Savegame();
        save.LoadSaveFolder(folder);

        GameObject.Find("SaveData").GetComponent<SaveDataContainer>().saveGame = save;
        GameObject.Find("SaveData").GetComponent<SaveDataContainer>().state = SaveDataState.Load;

        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        Application.LoadLevel(0);
    }
}