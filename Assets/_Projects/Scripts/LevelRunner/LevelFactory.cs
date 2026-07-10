using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

public class LevelFactory : DraftUtils.SceneSingletonMonoBehaviour<LevelFactory>
{
    private DraftUtils.FormattedLogger _logger = new DraftUtils.FormattedLogger(nameof(LevelFactory));
    [SerializeField] private DraftUtils.ComponentReference<LevelRunner> levelRunnerReference;
    private DraftUtils.PopupFactory _factory = new();
    private LevelRunner _levelRunner;
    private DraftUtils.Ads.AdsManager _ads;
    public LevelRunner LevelRunner => _levelRunner;

    private void Start()
    {
        LoadCurrentLevelData();
        _ads = DraftUtils.Ads.AdsManager.Instance;
        _ads.ShowBanner(DraftUtils.Ads.AdBannerPosition.Bottom);
    }
    private void OnDestroy()
    {
        // Không gọi AdsManager.Instance ở đây: getter singleton sẽ hồi sinh GameObject
        // khi thoát play mode, gây log "objects were not cleaned up". Dùng ref đã cache
        // (Unity trả "null giả" nếu AdsManager đã bị hủy) để chuyển cảnh runtime vẫn
        // ẩn banner còn lúc teardown thì bỏ qua.
        if (_ads != null)
        {
            _ads.HideBanner();
        }
    }
    public void LoadCurrentLevelData()
    {
        PopupManager.Instance.HidePopupGameplay();
        PopupManager.Instance.GetPopupGameplay();
        PopupManager.Instance.GetPopupSkillGameplay();
        _levelRunner = LoadLevelAtIndex(DataManager.Instance.Level.Value);
        TryUseCoffeeTimeBooster();
        CheckAndShowNewSkillPopups();
    }
    private LevelRunner LoadLevelAtIndex(int index)
    {
        LevelRunner levelRunner = null;
        var maxIndex = GetMaxLevel();

        if (index > maxIndex)
        {
            index = index % maxIndex + 1;
        }

        var path = string.Format(GameConstain.StringFormats.LevelDataFileNameFormat, index);
        if (!DraftUtils.Utils.ResourcesUtils.TryLoad<LevelData>(path, out var levelData))
        {
            _logger.Log("Failed to load level data for index: {0}", index);
            return null;
        }

        PrepareLevelDataForRun(levelData, index);
        levelRunner = _factory.DestroyCurrentAndCreate(levelRunnerReference, transform);
        levelRunner.SetData(levelData);

        return levelRunner;
    }

    private void PrepareLevelDataForRun(LevelData levelData, int index)
    {
        levelData.SetLevelIndex(index);
        levelData.ShuffleColors();
    }

    private int GetMaxLevel()
    {
        var levelAssets = Resources.LoadAll<TextAsset>(GameConstain.StringFormats.LevelDataPath);
        if (levelAssets == null || levelAssets.Length == 0)
        {
            return 1;
        }

        return levelAssets.Length;
    }

    private void TryUseCoffeeTimeBooster()
    {
        if (_levelRunner == null || DataManager.Instance == null || RuntimeStorage.Instance == null)
        {
            return;
        }

        if (!RuntimeStorage.Instance.TryGet(GameConstain.RuntimeStorage.StartBooterItems, out List<ItemType> booters) || booters == null)
        {
            return;
        }

        if (booters.Contains(ItemType.Booter_CoffeeTime))
        {
            DataManager.Instance.Using(ItemType.Booter_CoffeeTime, -1);
            booters.Remove(ItemType.Booter_CoffeeTime);
            RuntimeStorage.Instance.Set(GameConstain.RuntimeStorage.StartBooterItems, booters);

            int bonusTime = 60;
            PopupManager.Instance.ShowPopupUsingBooter(ItemType.Booter_CoffeeTime, .5f, null);
            PopupManager.Instance.ShowPopupCoffeeTime(bonusTime, PopupManager.Instance.popupGameplayReference.instance.TimeText.value.values[0].Text.rectTransform, () => _levelRunner.AddTime(bonusTime));

        }

        if (booters.Contains(ItemType.Booter_Magic))
        {
            DataManager.Instance.Using(ItemType.Booter_Magic, -1);
            booters.Remove(ItemType.Booter_Magic);
            RuntimeStorage.Instance.Set(GameConstain.RuntimeStorage.StartBooterItems, booters);
            PopupManager.Instance.ShowPopupUsingBooter(ItemType.Booter_Magic, .5f, () =>
            {
                if (_levelRunner == null || _levelRunner.LevelObjectSpawner == null)
                {
                    return;
                }

                var containers = _levelRunner.LevelObjectSpawner.ContainerPooler.ActiveItems;
                if (containers == null || containers.Count < 2)
                {
                    return;
                }

                for (int i = 0; i < 2 && i < containers.Count; i++)
                {
                    var container = containers[i];
                    if (container != null)
                    {
                        _levelRunner.StartCoroutine(PlayDestroyAnimation(container));
                    }
                }
            });
        }

    }

    private System.Collections.IEnumerator PlayDestroyAnimation(Container container)
    {
        if (container == null)
        {
            yield break;
        }

        var colliders = container.GetComponentsInChildren<Collider>();
        foreach (var col in colliders)
        {
            if (col != null)
            {
                col.enabled = false;
            }
        }

        yield return new WaitForSeconds(1f);

        var transform = container.transform;
        var originalScale = transform.localScale;

        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOScale(originalScale * 1.2f, 0.15f));
        seq.Append(transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack));
        seq.OnComplete(() =>
        {
            transform.localScale = originalScale;
            foreach (var col in colliders)
            {
                if (col != null)
                {
                    col.enabled = true;
                }
            }

            if (_levelRunner != null && _levelRunner.LevelObjectSpawner != null)
            {
                _levelRunner.LevelObjectSpawner.DestroyContainer(container);
            }
        });
    }

    private void CheckAndShowNewSkillPopups()
    {
        int currentLevel = DataManager.Instance.Level.Value;
        foreach (var pair in DataManager.Instance.levelUnlockItems)
        {
            ItemType skillType = pair.Key;
            int unlockLevel = pair.Value;

            if (currentLevel >= unlockLevel)
            {
                string prefsKey = "HasShownNewSkill_" + skillType.ToString();
                if (PlayerPrefs.GetInt(prefsKey, 0) == 0)
                {
                    PlayerPrefs.SetInt(prefsKey, 1);
                    PlayerPrefs.Save();

                    PopupManager.Instance.ShowPopupNewSkillItem(skillType);
                }
            }
        }
    }
}
