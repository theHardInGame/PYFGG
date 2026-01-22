using UnityEngine;
using System.Collections.Generic;
using UnityEditor.EditorTools;


[CreateAssetMenu(fileName = "BootConfig", menuName = "Bootstrap/BootConfig")]
public class BootConfig : ScriptableObject
{

    [Header("Scenes"), Tooltip("Will load MainMenu if First Scene is empty")]
    public string[] scenesToLoad;
    public bool skipMainMenu = false;



    [Header("Managers/Singletons"), Tooltip("Managers will load sequentially")]
    public List<GameObject> managersToInitiate;

    [Header("Settings")]
    public bool showLoadingScreen = true;
    public float managersInitTimeout = 5.0f; 
}
