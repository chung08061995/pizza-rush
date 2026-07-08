using UnityEngine;
using System.IO;
using Sirenix.OdinInspector;

public class LevelCreator : DraftUtils.DraftMonoBehaviour
{
    private const string LevelDataDirectory = "Assets/_Projects/Resources/LevelData";

    [SerializeField] private DraftUtils.Pooler<AnchorPoint> anchorPointPooler = new();
    [SerializeField] private LevelObjectSpawner levelObjectSpawner;
    [SerializeField, Min(1)] private int levelIndex = 100;
    [ShowInInspector] private LevelData levelData = new();
    [ShowInInspector] private LevelCreatorStateMachine levelCreatorStateMachine = new();
    public DraftUtils.Pooler<AnchorPoint> AnchorPointPooler => anchorPointPooler;
    public LevelObjectSpawner LevelObjectSpawner => levelObjectSpawner;
    public LevelData LevelData => levelData;
    public int LevelIndex => levelIndex;
    public string LevelDataPath => Path.Combine(LevelDataDirectory, $"{levelIndex:D4}.json").Replace("\\", "/");

    private void Start()
    {

        anchorPointPooler.EnsureParentExists(transform);
        /*
        Camera.main.transform.position = new Vector3(
            levelCreatorStateMachine.DrawBackgroundState.gridSize / 2,
            Camera.main.transform.position.y,
            levelCreatorStateMachine.DrawBackgroundState.gridSize / 2);
        */

        LoadLevelData();
        levelCreatorStateMachine.SetData(this);
        levelObjectSpawner.SetData(levelData, transform);
    }

    [Button]
    private void LoadLevelData()
    {
        levelIndex = Mathf.Max(1, levelIndex);
        Directory.CreateDirectory(LevelDataDirectory);
        levelData = LevelData.Load(LevelDataPath);
        levelData.SetLevelIndex(levelIndex);
    }

    private void Update()
    {
        levelCreatorStateMachine.StateMachine.Update();

    }
}
