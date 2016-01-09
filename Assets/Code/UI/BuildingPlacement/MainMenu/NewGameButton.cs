using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class NewGameButton : MonoBehaviour {

    public InputField seedField;

    public void Submit()
    {
        GameObject saveData = GameObject.Find("SaveData");
        saveData.GetComponent<SaveDataContainer>().state = SaveDataState.New;
        saveData.GetComponent<SaveDataContainer>().newGameSeed = System.Convert.ToInt32(seedField.text);

        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}
