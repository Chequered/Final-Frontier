using System.Collections;

using UnityEngine;

using FinalFrontier.Managers;
using FinalFrontier.UI;
using FinalFrontier.Serialization;

public enum GameState
{
    Playing,
    Paused,
    MainMenu,
    Loading
}

public class Main : MonoBehaviour {

    private GameManager _gm;

	private void Start()
    {
        //Load Managers
        ManagerInstance.OnLoad();

        //Get References
        _gm = ManagerInstance.Get<GameManager>();

        //Start Managers
        ManagerInstance.OnStart();

        //DEBUG: move cam to middle of level
        int center = TerrainManager.WORLD_WIDTH * FinalFrontier.Terrain.TerrainChunk.SIZE / 2 - 8;
        Camera.main.transform.position = new Vector3(center, center, -10);
    }

    private void Update()
    {
        OnTick();
        if (_gm.gameState == GameState.Playing)
            OnUpdate();
    }

    //Custom events
    private void OnTick()
    {
        ManagerInstance.OnTick();
    }

    private void OnUpdate()
    {
        ManagerInstance.OnUpdate();
    }

    private void Save()
    {
        ManagerInstance.OnSave();
    }
}
