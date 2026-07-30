public class InitProgress : DraftUtils.DraftMonoBehaviour
{
    void Start()
    {
        GameAnalytics.Log(GameAnalytics.AppStart);
        float loadingDuration = DataManager.Instance.ParametterGameConfigSO.InitDelay;
        PopupManager.Instance.GetPopupLoading(loadingDuration, SceneControllerExtensions.LoadMain);
    }
}
