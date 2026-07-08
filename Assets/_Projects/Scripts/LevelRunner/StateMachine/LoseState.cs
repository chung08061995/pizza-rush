using DG.Tweening;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class LoseState : DraftUtils.IState
{
    private LevelRunner _levelRunner;
    private bool _isLoseTriggered = false;
    private int _frameCount = 0;

    public void SetLevelRunner(LevelRunner levelRunner)
    {
        _levelRunner = levelRunner;
    }

    public void FixedUpdate()
    {
    }

    public void OnEnter()
    {
        _levelRunner.Timer.Pause();
        _isLoseTriggered = false;
        
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayLose(_levelRunner.transform);
        }
        
        if (PopupManager.Instance != null)
        {
            PopupManager.Instance.GetPopupBlockUser(5f);
        }
    }

    private void Lose()
    {
        if (PopupManager.Instance != null)
        {
            PopupManager.Instance.GetPopupLose();
        }
    }

    public void OnExit()
    {
    }

    public void Update()
    {
        if (_isLoseTriggered) return;

        _frameCount++;
        if (_frameCount % 5 != 0) return;

        bool anyAnimating = _levelRunner.GetComponentsInChildren<Container>().Any(c => c.isAnimating);
        if (!anyAnimating)
        {
            _isLoseTriggered = true;
            Lose();
        }
    }
}
