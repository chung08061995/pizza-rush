using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class WinState : DraftUtils.IState
{
    private LevelRunner _levelRunner;

    public void SetLevelRunner(LevelRunner levelRunner)
    {
        _levelRunner = levelRunner;
    }

    public void FixedUpdate()
    {

    }

    private bool _isWinTriggered = false;

    public void OnEnter()
    {
        _levelRunner.Timer.Pause();
        _isWinTriggered = false;
        GameAnalytics.LogLevelEvent(GameAnalytics.LevelWin);

        if (PopupManager.Instance != null)
        {
            PopupManager.Instance.GetPopupBlockUser(5f);
        }


        DataManager.Instance.Level.Value += 1;
        DataManager.Instance.Level.Save();
    }
    private void Win()
    {
        var goldReward = _levelRunner.LevelData != null ? _levelRunner.LevelData.goldReward : 0;
        if (goldReward > 0)
        {
            DataManager.Instance.Reward(new() { new RewardData { itemType = ItemType.Gold, amount = goldReward } });
        }

        void ShowResult() => PopupManager.Instance.ShowPopupWin(goldReward);
        if (DraftUtils.Ads.AdsManager.Instance != null)
            DraftUtils.Ads.AdsManager.Instance.ShowLevelEndInterstitial(true, ShowResult);
        else
            ShowResult();
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayWin(_levelRunner.transform);
        }
        VibrationManager.Vibrate(VibrationType.Success);
    }
    public void OnExit()
    {
    }

    private int _frameCount = 0;

    public void Update()
    {
        if (_isWinTriggered) return;

        _frameCount++;
        if (_frameCount % 5 != 0) return;

        bool anyAnimating = _levelRunner.GetComponentsInChildren<Container>().Any(c => c.isAnimating);
        if (!anyAnimating)
        {
            _isWinTriggered = true;
            Win();
        }
    }
}
