using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneControllerExtensions
{
    public static void LoadMain()
    {
        DraftUtils.SceneControllerSingleton.Instance.LoadScene(GameConstain.SenceName.Main, LoadSceneMode.Single);
    }
    public static void LoadGameplay()
    {
        DraftUtils.SceneControllerSingleton.Instance.LoadSceneAsync(GameConstain.SenceName.LevelRunner, LoadSceneMode.Single, DraftUtils.SceneControllerSingleton.Instance.SetActiveFirstAdditionalScene);
    }
}