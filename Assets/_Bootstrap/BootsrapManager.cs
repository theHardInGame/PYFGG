using UnityEngine;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine.SceneManagement;
using System;
using UnityEditor.Build.Reporting;

public class BootstrapManager : MonoBehaviour
{
    [SerializeField] BootConfig config;

    private async void Awake()
    {
        await CleanUpScenes();

        if (config.showLoadingScreen)
        {
            Debug.Log("Bootstrap Loading...");
        }

        await InitializeManagers();
        await LoadFirstScene();

        Debug.Log("Loading Done!");

        UnloadBootLoader();
    }

    /// <summary>
    /// Unloads scenes preloaded at start except bootstrap scene
    /// </summary>
    /// <returns></returns>
    private async Task CleanUpScenes()
    {
        int currentSceneCount = SceneManager.sceneCount;

        if (currentSceneCount > 1)
        {
            Scene btScene = gameObject.scene;

            for (int i = 0; i < currentSceneCount; i++)
            {
                Scene indexedScene = SceneManager.GetSceneAt(i);

                if (indexedScene == btScene) continue;

                await SceneManager.UnloadSceneAsync(indexedScene.name);
            }
        }
    }

    /// <summary>
    /// Initialize singleton managers
    /// </summary>
    /// <returns></returns>
    private async Task InitializeManagers()
    {
        foreach (GameObject manager in config.managersToInitiate)
        {
            GameObject managerGO = Instantiate(manager);

            if(managerGO.TryGetComponent(out IInitializable init))
            {
                await Awaitable.MainThreadAsync();
                Task task = init.InitializeAsync();

                await Awaitable.BackgroundThreadAsync();
                Task timeoutTask = Task.Run(async () => await Task.Delay((int)(config.managersInitTimeout * 1000)));
                
                Task completed = await Task.WhenAny(task, timeoutTask);

                await Awaitable.MainThreadAsync();
                if(completed == timeoutTask)
                {
                    Destroy(managerGO);
                    Debug.LogWarning($"{manager.name} timed out and could not be initialized");
                }
                else
                {
                    Debug.Log($"Singleton: {manager.name} successfully initialized");
                }
            }
            else
            {
                Debug.LogError($"Failied to initialize {manager.name} + \n + {manager.name} does not inherit IInitializable interface");
            }
        }
    }

    private async Task LoadFirstScene()
    {
        AsyncOperation[] ops = new AsyncOperation[config.scenesToLoad.Length];
        for (int i = 0; i < config.scenesToLoad.Length; i++)
        {
            ops[i] = SceneManager.LoadSceneAsync(config.scenesToLoad[i], LoadSceneMode.Additive);
        }

        while (!IsDoneHelper(ops))
        {
            await Awaitable.NextFrameAsync();
        }
    }

    private bool IsDoneHelper(AsyncOperation[] ops)
    {
        for (int i = 0; i < ops.Length; i++)
        {
            if (!ops[i].isDone) return false;
        }

        return true;    
    }

    private void UnloadBootLoader()
    {
        SceneManager.UnloadSceneAsync("Bootstrap");
    }
}
