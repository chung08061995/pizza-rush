using UnityEngine;

public class LockContentComponent : DraftUtils.DraftMonoBehaviour
{
    [SerializeField] private DraftUtils.OptionalGameObjectGroup lockObject = new();
    [SerializeField] private DraftUtils.OptionalGameObjectGroup unlockObject = new();
    [SerializeField] private DraftUtils.OptionalTMPTextGroup unlockAtLevelText = new();

    public void SetData(int unlockAtLevel)
    {
        bool isUnlocked = DataManager.Instance.Level.Value >= unlockAtLevel;
        SetLockObject(isUnlocked);
        SetUnlockObject(isUnlocked);
        SetUnlockAtLevelText(isUnlocked, unlockAtLevel);
    }

    private void SetLockObject(bool isUnlocked)
    {
        lockObject.SetActive(!isUnlocked);
    }

    private void SetUnlockObject(bool isUnlocked)
    {
        unlockObject.SetActive(isUnlocked);
    }

    private void SetUnlockAtLevelText(bool isUnlocked, int unlockAtLevel)
    {
        unlockAtLevelText.SetText(string.Format("Unlock At Level {0}", unlockAtLevel));
    }
}
