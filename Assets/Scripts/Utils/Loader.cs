using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public static class Loader
{
    public enum Scene {
        MainScene,
        SinglePlayerLoadScene,
        LobbyScene,
        CharSelectScene,
        GamePreloadingScene,
        GameScene,
        StarshipScene
    }

    public static Scene targetScene;

    public static void Load(Scene targetScene)
    {
        Loader.targetScene = targetScene;
        SceneManager.LoadScene(targetScene.ToString());
    }

    public static void LoadNetwork(Scene targetScene)
    {
        Loader.targetScene = targetScene;
        NetworkManager.Singleton.SceneManager.LoadScene(targetScene.ToString(), LoadSceneMode.Single);
    }
}
