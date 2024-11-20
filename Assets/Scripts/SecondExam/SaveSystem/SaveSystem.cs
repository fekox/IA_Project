using System;
using RojoinSaveSystem;
using UnityEngine;

public class SaveSystem : MonoBehaviour
{
    public RojoinSaveSystem.SaveSystem _saveSystem;
    public GameSettings _gameSettings;

    public string pathToSave = "/Saves/Genomes";
    public string extension = "genome";

    private void OnEnable()
    {
        _saveSystem = new RojoinSaveSystem.SaveSystem();
        var currentPath = Application.dataPath + pathToSave + "." + extension;
        _saveSystem.savePath = currentPath;
        _saveSystem.StartSaveSystem(DebugLogger);
        _saveSystem.AddObjectToSave(_gameSettings);
    }

    public void DebugLogger(string text) => Debug.Log(text);

    [ContextMenu("Save Game")]
    public void Save()
    {
        _saveSystem.CreateSaveFile();
    }

    [ContextMenu("Load")]
    public void Load()
    {
        _saveSystem.LoadSaveFile();
    }
}