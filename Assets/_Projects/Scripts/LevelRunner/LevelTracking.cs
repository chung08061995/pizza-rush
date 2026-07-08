using System;
[System.Serializable]
public class LevelTracking
{
    public DraftUtils.PersistentValue<int> dragContainerTimes { get; set; } = new();

    public DraftUtils.PersistentValue<int> resolvedContainer { get; set; } = new();
}