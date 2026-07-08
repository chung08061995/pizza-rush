using UnityEngine;
using UnityEngine.SceneManagement;

public class InitProgress : DraftUtils.DraftMonoBehaviour
{
    void Start()
    {
        StartCoroutine(DraftUtils.Utils.CoroutineUtils.DelayBySeconds(DataManager.Instance.ParametterGameConfigSO.InitDelay, SceneControllerExtensions.LoadMain));
        PopupManager.Instance.GetPopupLoading(1);
        
        PopupManager.Instance.GetPopupCheatLevel();
    }
}
