using UnityEngine;
using UnityEngine.SceneManagement;

public class InitProgress : DraftUtils.DraftMonoBehaviour
{
    void Start()
    {
        GameAnalytics.Log(GameAnalytics.AppStart);
        StartCoroutine(DraftUtils.Utils.CoroutineUtils.DelayBySeconds(DataManager.Instance.ParametterGameConfigSO.InitDelay, SceneControllerExtensions.LoadMain));
        PopupManager.Instance.GetPopupLoading(1);

#if UNITY_EDITOR
        PopupManager.Instance.GetPopupCheatLevel();
#endif
    }
}
