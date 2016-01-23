using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System;

using UnityEngine;

using EndlessExpedition.Managers;
using EndlessExpedition.Serialization;

public enum GameState
{
    Booting,
    Playing,
    Paused,
    MainMenu,
    LoadingNew,
    LoadingSave,
    StartingNew,
    StartingSave
}

public class Main : MonoBehaviour {

    //DebugOnly //save-2016-01-02_01-29-59-PM
    public string saveGameName = "null"; 

	private void Awake()
    {
        if (saveGameName != "null")
        {
            Savegame save = new Savegame();
            save.LoadSaveFolder(saveGameName);
            GameManager.saveDataContainer.saveGame = save;
            GameManager.saveDataContainer.state = SaveDataState.Load;
        }

        if (GameManager.saveDataContainer.state == SaveDataState.Load)
            GameManager.gameState = GameState.LoadingSave;
        else
            GameManager.gameState = GameState.LoadingNew;

        //Setup mod refs
        EndlessExpedition.Entities.BuildingModules.BuildingModule.SearchModules();

        //Load Managers
        ManagerInstance.OnLoad();

        //Start Managers
        ManagerInstance.OnStart();

        //DEBUG: move cam to middle of level
        int center = TerrainManager.WORLD_WIDTH * EndlessExpedition.Terrain.TerrainChunk.SIZE / 2 - 8;
        Camera.main.transform.position = new Vector3(center, center, -20);
    }

    private void Update()
    {
        OnTick();
        if (GameManager.gameState == GameState.Playing)
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

    public static Main instance
    {
        get
        {
            return GameObject.Find("Main").GetComponent<Main>();
        }
    }
}

public static class Extensions
{
    public static bool IsDefault<T>(this T value) where T : struct
    {
        bool isDefault = value.Equals(default(T));

        return isDefault;
    }

    public static List<Type> GetListOfType<T>() where T : class
    {
        List<Type> types = new List<Type>();
        foreach (Type type in
            Assembly.GetAssembly(typeof(T)).GetTypes()
            .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T))))
        {
            types.Add(type);
        }
        return types;
    }
}
