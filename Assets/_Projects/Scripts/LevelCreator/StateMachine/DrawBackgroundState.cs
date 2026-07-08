using System.Linq;
using UnityEngine;

public class DrawBackgroundState : DraftUtils.IState
{
    private LevelCreator _levelCreator;
    public int gridSize = 10;
    public void SetLevelCreator(LevelCreator levelCreator)
    {
        _levelCreator = levelCreator;
    }
    public void FixedUpdate()
    {

    }

    public void OnEnter()
    {
        _levelCreator.AnchorPointPooler.DespawnAll();

        GenerateAnchorPointGrid(_levelCreator.AnchorPointPooler, _levelCreator.LevelObjectSpawner.Grid, gridSize);

        foreach (var pos in _levelCreator.LevelData.gridPositions)
        {
            var anchorPoint = _levelCreator.AnchorPointPooler.ActiveItems.FirstOrDefault(ap => ap.CellPosition == pos.ToVector2Int());
            if (anchorPoint != null)
            {
                anchorPoint.SetColor(Color.yellow);
            }
        }
    }

    /// <summary>
    /// Sinh lưới anchor point 10x10
    /// </summary>
    /// <param name="spacing">Khoảng cách giữa các anchor point</param>
    /// <param name="startPosition">Vị trí bắt đầu của lưới</param>
    public void GenerateAnchorPointGrid(DraftUtils.Pooler<AnchorPoint> anchorPointPooler, DraftUtils.GridXZ grid, int gridSize)
    {
        anchorPointPooler.Factory = new DraftUtils.ComponentInstantiatePoolFactory<AnchorPoint>();

        for (int x = 0; x < gridSize; x++)
        {
            for (int z = 0; z < gridSize; z++)
            {
                // Lấy anchor point từ pooler
                var anchorPoint = anchorPointPooler.Spawn();
                var anchorPointIndex = new Vector2Int(x, z);
                anchorPoint.transform.position = grid.CellToWorld(anchorPointIndex);
                anchorPoint.CellPosition = anchorPointIndex;
                anchorPoint.SetIndex(anchorPointIndex);
            }
        }
    }
    public void OnExit()
    {

        _levelCreator.AnchorPointPooler.DespawnAll();
    }

    public void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseClick();
        }
    }
    private void HandleMouseClick()
    {


        if (DraftUtils.Utils.Physic3DUtils.TryGetComponentUnderMouse(
            mouseScreenPosition: Input.mousePosition,
            camera: Camera.main,
            getComponentFunc: hit => hit.collider.GetComponentInParent<AnchorPoint>(),
            out AnchorPoint anchorPointUnderMouse
        ))
        {
            _levelCreator.LevelData.AddPosition(new SerializableVector2Int(anchorPointUnderMouse.CellPosition));
            _levelCreator.LevelData.Save(_levelCreator.LevelDataPath);
            anchorPointUnderMouse.SetColor(Color.yellow);
        }
        //_levelCreator.LevelObjectSpawner.SetData(_levelCreator.LevelData, _levelCreator.transform);
    }
}
