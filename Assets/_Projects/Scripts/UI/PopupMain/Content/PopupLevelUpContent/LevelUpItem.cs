using UnityEngine;
using UnityEngine.UI;

public class LevelUpItem : EnhancedUI.EnhancedScroller.EnhancedScrollerCellView, ICellView2<LevelUpCellData>
{
    [SerializeField] private DraftUtils.OptionalGameObjectGroup passedObject = new();
    [SerializeField] private DraftUtils.OptionalGameObjectGroup notReachedObject = new();
    [SerializeField] private DraftUtils.OptionalGameObjectGroup noRewardedObject = new();

    [SerializeField] private DraftUtils.OptionalGameObjectGroup rewardedObject = new();
    [SerializeField] private Button rewardButton;

    [SerializeField] private DraftUtils.OptionalGameObjectGroup currentObject = new();
    [SerializeField] private LevelUpRewardSOView rewardView;

    private int _currentLevel;
    private bool _isLast;
    private LevelUpRewardSO _data;

    private void Awake()
    {
        if (rewardButton != null)
        {
            rewardButton.onClick.AddListener(OnRewardButtonClicked);
        }
    }

    public void SetData(LevelUpCellData data, int index)
    {
        SetData(data.currentLevel, data.isLast, data.rewardSO);
    }

    public void SetData(int currentLevel, bool isLast, LevelUpRewardSO data)
    {
        _currentLevel = currentLevel;
        _isLast = isLast;
        _data = data;
        rewardView.SetData(data);

        SetPassedObject();
        SetNotReachedObject();
        SetCurrentObject();
        SetRewardState();
    }

    private void SetPassedObject()
    {
        passedObject.SetActive(!_isLast && _currentLevel > _data.level);
    }

    private void SetNotReachedObject()
    {
        notReachedObject.SetActive(_isLast || _currentLevel < _data.level);
    }

    private void SetCurrentObject()
    {
        currentObject.SetActive(_isLast);
    }

    private void SetRewardState()
    {
        if (_data == null)
        {
            return;
        }

        bool isClaimed = IsClaimed();
        noRewardedObject.SetActive(!isClaimed && (_currentLevel < _data.level || _currentLevel >= _data.level));
        rewardedObject.SetActive(isClaimed);

        if (rewardButton != null)
        {
            rewardButton.interactable = !isClaimed && _currentLevel >= _data.level;
            //rewardButton.gameObject.SetActive(!isClaimed);
        }
    }

    private bool IsClaimed()
    {
        if (_data == null)
        {
            return false;
        }

        return PlayerPrefs.GetInt(GetClaimedKey(), 0) == 1;
    }

    private string GetClaimedKey()
    {
        return GameConstain.PlayerPrefsKey.LevelUpRewardClaimedPrefix + _data.level;
    }

    private void OnRewardButtonClicked()
    {
        if (_data == null || !CanClaim())
        {
            return;
        }

        if (DataManager.Instance != null)
        {
            DataManager.Instance.Reward(_data.rewardDatas);
        }

        PlayerPrefs.SetInt(GetClaimedKey(), 1);
        PlayerPrefs.Save();

        SetRewardState();

        if (_data.rewardDatas == null || _data.rewardDatas.Count == 0)
        {
            return;
        }

        if (_data.rewardDatas.Count == 1)
        {
            PopupManager.Instance.ShowPopupRewardSingle(_data.rewardDatas[0]);
        }
        else
        {
            PopupManager.Instance.ShowPopupRewardMultiple(_data.rewardDatas);
        }
    }

    private bool CanClaim()
    {
        return _data != null && _currentLevel >= _data.level && !IsClaimed();
    }
}
