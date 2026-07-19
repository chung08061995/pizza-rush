using UnityEngine;


public class HomeContentsController : DraftUtils.DraftMonoBehaviour
{
    [SerializeField] private DraftUtils.ComponentReference<PopupShopContent> popupShopContentReference = new();

    [SerializeField] private DraftUtils.ComponentReference<PopupRankingContent> popupRankingContentReference = new();

    [SerializeField] private DraftUtils.ComponentReference<PopupHomeContent> popupHomeContentReference = new();
    [SerializeField] private DraftUtils.ComponentReference<PopupLevelUpContent> popupLevelUpContentReference = new();
    [SerializeField] private DraftUtils.ComponentReference<PopupSettingContent> popupSettingContentReference = new();
    private DraftUtils.PopupFactory _popupFactory => PopupManager.Instance.PopupFactory;

    public PopupShopContent ShowPopupShopContent(Transform root)
    {
        var popup = _popupFactory.DestroyCurrentAndCreate(popupShopContentReference, root);
        return popup;
    }
    public PopupRankingContent ShowPopupRankingContent(Transform root)
    {
        var popup = _popupFactory.DestroyCurrentAndCreate(popupRankingContentReference, root);
        return popup;
    }
    public PopupHomeContent ShowPopupHomeContent(Transform root)
    {
        if (popupHomeContentReference.instance == null)
        {
            popupHomeContentReference.instance = root.GetComponentInChildren<PopupHomeContent>(true);
        }

        var popup = _popupFactory.GetOrCreate(popupHomeContentReference, root);
        popup.gameObject.SetActive(true);
        return popup;
    }

    public PopupLevelUpContent ShowPopupLevelUpContent(Transform root)
    {
        var popup = _popupFactory.DestroyCurrentAndCreate(popupLevelUpContentReference, root);
        popup.SetData();
        return popup;
    }
    public PopupSettingContent ShowPopupSettingContent(Transform root)
    {
        var popup = _popupFactory.DestroyCurrentAndCreate(popupSettingContentReference, root);
        return popup;
    }

}
