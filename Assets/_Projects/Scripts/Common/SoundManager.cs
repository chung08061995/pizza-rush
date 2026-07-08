using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

public class SoundManager : DraftUtils.SingletonDontDestroyOnLoadMonoBehaviour<SoundManager>
{
    [SerializeField] private DraftUtils.SoundChanel background;
    [SerializeField] private DraftUtils.SoundChanel sfxSound;

    private DraftUtils.PersistentValue<bool> backgroundVolume => DataManager.Instance.musicVolume;
    private DraftUtils.PersistentValue<bool> sfxVolume => DataManager.Instance.sfxVolume;

    private void Start()
    {
        DOVirtual.DelayedCall(0.25f, () =>
        {

            background.SetFactory();
            sfxSound.SetFactory();

            backgroundVolume.Notifier.AddListener(ChangeBackgroundVolume);
            sfxVolume.Notifier.AddListener(ChangeSfxVolume);

            ChangeBackgroundVolume(backgroundVolume.Value);
            ChangeSfxVolume(sfxVolume.Value);
        });
    }

    private void OnDestroy()
    {
        if (DataManager.Instance == null)
        {
            return;
        }

        backgroundVolume.Notifier.RemoveListener(ChangeBackgroundVolume);
        sfxVolume.Notifier.RemoveListener(ChangeSfxVolume);
    }

    private void ChangeBackgroundVolume(bool volume)
    {
        background.ChangeConfig(new()
        {
            loop = true,
            volume = DraftUtils.Utils.ConvertUtils.ConvertBoolToFloat(volume)
        });
    }

    private void ChangeSfxVolume(bool volume)
    {
        sfxSound.ChangeConfig(new()
        {
            loop = false,
            volume = DraftUtils.Utils.ConvertUtils.ConvertBoolToFloat(volume)
        });
    }

    public void PlayButtonPress(Button button)
    {
        if (button == null)
        {
            return;
        }

        PlayButtonPress(button.transform);
    }

    public void PlayButtonPress(Transform button)
    {
        if (button == null || DataManager.Instance.AudioClipDataCreator == null)
        {
            return;
        }

        PlaySfx(ItemType.Sound_PressButton, button);
    }

    public void PlayWin(Transform target = null)
    {
        PlaySfx(ItemType.Sound_Win, target != null ? target : transform);
    }

    public void PlayLose(Transform target = null)
    {
        PlaySfx(ItemType.Sound_Lose, target != null ? target : transform);
    }

    public void PlayBackgroundLobby(Transform parent)
    {
        PlayBackground(ItemType.Sound_BackgroundLobby, parent);
    }

    public void PlayBackgroundGame(Transform parent)
    {
        PlayBackground(ItemType.Sound_BackgroundGame, parent);
    }

    private void PlaySfx(ItemType itemType, Transform target)
    {
        try
        {
            if (target == null || DataManager.Instance.AudioClipDataCreator == null)
            {
                return;
            }

            if (DataManager.Instance.AudioClipDataCreator.clips.TryGetValue(itemType, out var clip) && clip.Clip != null)
            {
                sfxSound.Play(Guid.NewGuid(), clip.Clip, target);
            }
        }
        catch { }
    }

    private void PlayBackground(ItemType itemType, Transform parent)
    {
        try
        {
            if (parent == null || DataManager.Instance.AudioClipDataCreator == null)
            {
                return;
            }

            if (DataManager.Instance.AudioClipDataCreator.clips.TryGetValue(itemType, out var clip) && clip.Clip != null)
            {
                background.Play(itemType, clip.Clip, parent);
            }
        }
        catch { }
    }
}
