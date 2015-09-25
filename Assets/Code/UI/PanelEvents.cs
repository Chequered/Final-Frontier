using UnityEngine;
using System.Collections;

using FinalFrontier.Managers;

public class PanelEvents : MonoBehaviour {

    private SaveLoadPanel _saveLoadPanel;

    private void Start()
    {
        _saveLoadPanel = GameObject.Find("Save/Load Panel").GetComponent<SaveLoadPanel>();
    }

	public void ShowWorldInfo()
    {
        ManagerInstance.Get<TerrainManager>().InspectWorldProperties();
    }

    public void SaveGame()
    {
        _saveLoadPanel.SetMode(SaveLoadMode.Save);
    }

    public void LoadGame()
    {
        _saveLoadPanel.SetMode(SaveLoadMode.Load);
    }
}
