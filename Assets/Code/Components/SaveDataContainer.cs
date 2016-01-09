using UnityEngine;
using System.Collections;

using FinalFrontier.Serialization;

public enum SaveDataState
{
    New,
    Load
}

public class SaveDataContainer : MonoBehaviour {

    public Savegame saveGame;
    public SaveDataState state;
    public int newGameSeed = 0;

	private void Start () {
        DontDestroyOnLoad(this);
	}
}
