using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public class PreloadMainScene
{
    private static readonly string sceneBeforePlayKey = "SceneBeforePlayPath";

    static PreloadMainScene()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        switch (state)
        {
            case PlayModeStateChange.ExitingEditMode:
                // Store the current scene's path using SessionState
                SessionState.SetString(sceneBeforePlayKey, SceneManager.GetActiveScene().path);
                LoadMainSceneIfNeeded();
                break;

            case PlayModeStateChange.EnteredEditMode:
                // Retrieve the scene path from SessionState
                string sceneToReturnTo = SessionState.GetString(sceneBeforePlayKey, "");

                if (!string.IsNullOrEmpty(sceneToReturnTo) && sceneToReturnTo != SceneManager.GetActiveScene().path)
                {
                    EditorSceneManager.OpenScene(sceneToReturnTo);
                }
                break;
        }
    }

    private static void LoadMainSceneIfNeeded()
    {
        string mainScenePath = "Assets/Scenes/LoadingScene.unity";

        if (SceneManager.GetActiveScene().path != mainScenePath)
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            EditorSceneManager.OpenScene(mainScenePath);
        }
    }
}
