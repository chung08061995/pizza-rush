using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

public class LevelObjectSpawner : DraftUtils.DraftMonoBehaviour
{
    private DraftUtils.FormattedLogger _logger = new DraftUtils.FormattedLogger(nameof(LevelObjectSpawner));
    [SerializeField] private DraftUtils.TileMap3D tileMap3D;
    [SerializeField] private DraftUtils.ObjectCreator<Container> containerPooler = new();
    [SerializeField] private ContainerFactory containerFactory;
    [SerializeField] private DraftUtils.Pooler<ProductionLine> productionLinePooler = new();
    [SerializeField] private ProductionLineFactory productionLineFactory;
    [SerializeField] private DraftUtils.Pooler<AnchorPoint> anchorPointPooler = new();
    [SerializeField] private CoverFactory coverFactory;

    private DraftUtils.GridXZ grid = new();
    private readonly List<Vector2Int> _initialAddTilePositions = new();

    public DraftUtils.GridXZ Grid => grid;
    public DraftUtils.Pooler<ProductionLine> ProductionLinePooler => productionLinePooler;
    public DraftUtils.ObjectCreator<Container> ContainerPooler => containerPooler;
    public CoverFactory CoverFactory => coverFactory;
    private LevelData _levelData;
    private Transform _root;
    public void SetLevelData(LevelData newLevelData)
    {
        _levelData = newLevelData;
        _initialAddTilePositions.Clear();
    }
    public void SetRoot(Transform root)
    {
        _root = root;

        containerPooler.EnsureParentExists(root);
        productionLinePooler.EnsureParentExists(root);
        tileMap3D.TilePooler.EnsureParentExists(root);
        anchorPointPooler.EnsureParentExists(root);
    }
    public void SetGrid(DraftUtils.GridXZ newGrid)
    {
        grid = newGrid;
    }
    public void SetData(LevelData levelData, Transform root)
    {
        _levelData = levelData;

        _root = root;

        tileMap3D.TilePooler.EnsureParentExists(root);
        
        tileMap3D.Generate(levelData.gridPositions.ConvertAll(pos => pos.ToVector2Int()));

        containerPooler.EnsureParentExists(root);

        productionLinePooler.EnsureParentExists(root);
        productionLinePooler.Factory = new DraftUtils.ComponentInstantiatePoolFactory<ProductionLine>();

        anchorPointPooler.Factory = new DraftUtils.ComponentInstantiatePoolFactory<AnchorPoint>();

        containerFactory.SpawnFromLevelData(levelData, containerPooler, grid);
        productionLineFactory.SpawnFromLevelData(levelData, productionLinePooler, grid);
        CacheInitialAddTilePositions();
    }
    public List<Vector2Int> GetAvailableGridPositions()
    {
        List<Vector2Int> availableGridPositions = _levelData.gridPositions.ConvertAll(pos => pos.ToVector2Int());

        var occupied = new HashSet<Vector2Int>();
        foreach (var container in containerPooler.ActiveItems)
        {
            var containerCell = grid.WorldToCell(container.transform.position);
            foreach (var partPos in container.GetPartPositions())
            {
                occupied.Add(containerCell + partPos);
            }
        }
        availableGridPositions.RemoveAll(p => occupied.Contains(p));
        return availableGridPositions;
    }
    public List<Vector2Int> GetAvailableGridPositionsIgnore(Container containerToIgnore)
    {
        List<Vector2Int> availableGridPositions = _levelData.gridPositions.ConvertAll(pos => pos.ToVector2Int());

        var occupied = new HashSet<Vector2Int>();
        foreach (var container in containerPooler.ActiveItems)
        {
            if (container == containerToIgnore)
                continue;

            var containerCell = grid.WorldToCell(container.transform.position);
            foreach (var partPos in container.GetPartPositions())
            {
                occupied.Add(containerCell + partPos);
            }
        }
        availableGridPositions.RemoveAll(p => occupied.Contains(p));
        return availableGridPositions;
    }

    public void ReplaceContainer(Container oldContainer, ContainerData newData)
    {
        var cell = grid.WorldToCell(oldContainer.transform.position);

        containerPooler.Despawn(oldContainer);
        oldContainer.gameObject.SetActive(false);

        var newSaveData = new ContainerSaveData
        {
            position = new SerializableVector2Int(cell),
            rotationType = oldContainer.Data.rotationType,
            flipX = oldContainer.Data.flipX,
            containerData = newData
        };

        containerFactory.SpawnSingleContainer(newSaveData, containerPooler, grid);
    }

    private Sequence _showAddTileSequence;

    public void ShowAddTileAnchors()
    {
        HideAddTileAnchors();

        if (_initialAddTilePositions.Count == 0)
        {
            CacheInitialAddTilePositions();
        }

        var existingPositions = new HashSet<Vector2Int>(_levelData.gridPositions.ConvertAll(pos => pos.ToVector2Int()));
        
        var sortedOuterEdges = _initialAddTilePositions
            .Where(pos => !existingPositions.Contains(pos))
            .OrderBy(p => p.x)
            .ThenBy(p => p.y)
            .ToList();

        _showAddTileSequence = DOTween.Sequence();
        _showAddTileSequence.AppendInterval(0.2f); // đợi 1 ít

        foreach (var edgePos in sortedOuterEdges)
        {
            var anchor = anchorPointPooler.Spawn();
            anchor.transform.position = grid.CellToWorld(edgePos);
            anchor.SetIndex(new SerializableVector2Int(edgePos));

            anchor.transform.localScale = Vector3.zero;
            anchor.transform.localRotation = Quaternion.Euler(0, 0, 180);

            _showAddTileSequence.AppendCallback(() =>
            {
                if (anchor != null && anchor.gameObject.activeInHierarchy)
                {
                    anchor.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
                    anchor.transform.DORotate(Vector3.zero, 0.3f).SetEase(Ease.OutBack);
                }
            });
            _showAddTileSequence.AppendInterval(0.05f); // lật lần lượt
        }
    }

    private void CacheInitialAddTilePositions()
    {
        _initialAddTilePositions.Clear();

        if (_levelData == null)
        {
            return;
        }

        var existingPositions = new HashSet<Vector2Int>(_levelData.gridPositions.ConvertAll(pos => pos.ToVector2Int()));
        var productionLineCells = new HashSet<Vector2Int>();

        foreach (var line in productionLinePooler.ActiveItems)
        {
            if (line == null) continue;

            productionLineCells.Add(grid.WorldToCell(line.transform.position));
            if (line.Places == null) continue;

            foreach (var place in line.Places)
            {
                if (place != null)
                {
                    productionLineCells.Add(grid.WorldToCell(place.transform.position));
                }
            }
        }

        var outerEdges = new HashSet<Vector2Int>();
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (var pos in existingPositions)
        {
            foreach (var dir in directions)
            {
                var neighbor = pos + dir;
                if (!existingPositions.Contains(neighbor) && !productionLineCells.Contains(neighbor))
                {
                    outerEdges.Add(neighbor);
                }
            }
        }

        _initialAddTilePositions.AddRange(outerEdges);
    }

    public void HideAddTileAnchors()
    {
        _showAddTileSequence?.Kill();
        anchorPointPooler.DespawnAll();
    }

    public void AddNewTile(Vector2Int gridPosition)
    {
        if (_initialAddTilePositions.Count == 0)
        {
            CacheInitialAddTilePositions();
        }

        if (!_initialAddTilePositions.Contains(gridPosition))
        {
            return;
        }

        _levelData.AddPosition(new SerializableVector2Int(gridPosition));
        ClearTilePool();
        tileMap3D.Generate(_levelData.gridPositions.ConvertAll(pos => pos.ToVector2Int()));
    }

    public void DestroyContainer(Container container)
    {
        if (container == null)
        {
            return;
        }

        RemoveProductionsForDestroyedContainer(container);

        containerPooler.Despawn(container);
        container.gameObject.SetActive(false);
    }

    private void RemoveProductionsForDestroyedContainer(Container container)
    {
        var colorType = container.Data.containerData.containerColorData.colorType;
        var emptyPlaces = container.GetEmptyPlacesSortLeftToRightTopToBottom();
        int remainingToRemove = emptyPlaces.Count;

        if (remainingToRemove <= 0)
        {
            return;
        }

        foreach (var line in productionLinePooler.ActiveItems)
        {
            if (line == null || remainingToRemove <= 0) continue;

            remainingToRemove = RemoveActiveProductionsByColor(line, colorType, remainingToRemove);
            remainingToRemove = RemoveQueuedProductionDataByColor(line, colorType, remainingToRemove);
            ReindexProductionLine(line);
            line.Creat(DataManager.Instance.ParametterGameConfigSO.MaxProductionOnLine);
            line.ChangeProductionLineColor();
        }
    }

    private int RemoveActiveProductionsByColor(ProductionLine line, ColorType colorType, int remainingToRemove)
    {
        for (int i = line.ProductionPooler.ActiveItems.Count - 1; i >= 0 && remainingToRemove > 0; i--)
        {
            var production = line.ProductionPooler.ActiveItems[i];
            if (production == null || production.ColorType != colorType)
            {
                continue;
            }

            production.transform.DOKill();
            line.ProductionPooler.Despawn(production);
            production.gameObject.SetActive(false);

            if (line.Data.productionColors.Count > 0)
            {
                int dataIndex = Mathf.Clamp(production.CurrentIndex, 0, line.Data.productionColors.Count - 1);
                if (line.Data.productionColors[dataIndex] == colorType)
                {
                    line.Data.productionColors.RemoveAt(dataIndex);
                }
                else
                {
                    RemoveQueuedProductionDataByColor(line, colorType, 1);
                }
            }

            remainingToRemove--;
        }

        return remainingToRemove;
    }

    private int RemoveQueuedProductionDataByColor(ProductionLine line, ColorType colorType, int remainingToRemove)
    {
        for (int i = line.Data.productionColors.Count - 1; i >= 0 && remainingToRemove > 0; i--)
        {
            if (line.Data.productionColors[i] != colorType)
            {
                continue;
            }

            line.Data.productionColors.RemoveAt(i);
            remainingToRemove--;
        }

        return remainingToRemove;
    }

    private void ReindexProductionLine(ProductionLine line)
    {
        for (int i = 0; i < line.ProductionPooler.ActiveItems.Count; i++)
        {
            var production = line.ProductionPooler.ActiveItems[i];
            if (production == null) continue;

            production.CurrentIndex = i;
            production.transform.DOKill();
            line.SetupProductionTransform(production, i);
        }
    }

    private void ClearTilePool()
    {
        tileMap3D.TilePooler.DespawnAll();
        while (tileMap3D.TilePooler.InactiveItems.Count > 0)
        {
            tileMap3D.TilePooler.Spawn();
        }
        foreach (var item in tileMap3D.TilePooler.PublicActiveItems)
        {
            if (item != null)
            {
                Destroy(item.gameObject);
            }
        }
        tileMap3D.TilePooler.PublicActiveItems.Clear();
    }

    public bool TryGetProductionNearAndSamleColor(Container container, Vector2Int containerIndex, out ProductionLine productionLine)
    {
        productionLine = null;

        var containerColor = container.Data.containerData.containerColorData.colorType;
        Debug.Log($"[TryGetProduction] Container color: {containerColor}, containerIndex: {containerIndex}");

        foreach (var line in productionLinePooler.ActiveItems)
        {
            // Check trực tiếp production đầu tiên trên line có cùng màu container không
            bool hasColor = false;
            if (line.ProductionPooler.ActiveItems.Count > 0)
            {
                hasColor = line.ProductionPooler.ActiveItems[0].ColorType == containerColor;
            }
            var productionLineIndex = grid.WorldToCell(line.transform.position);
            Debug.Log($"[TryGetProduction] Line at {productionLineIndex}, activeCount: {line.ProductionPooler.ActiveItems.Count}, firstActiveColor: {(line.ProductionPooler.ActiveItems.Count > 0 ? line.ProductionPooler.ActiveItems[0].ColorType.ToString() : "EMPTY")}, hasFirstColor: {hasColor}");

            if (!hasColor) continue;

            var baseDirection = new Vector2Int(0, -1);
            var rotationType = line.Data.productionLineSaveData.rotationType;
            var rotatedDirection = RotateDirection(baseDirection, rotationType);
            var targetIndex = productionLineIndex + rotatedDirection;

            Debug.Log($"[TryGetProduction] rotationType: {rotationType}, baseDir: {baseDirection}, rotatedDir: {rotatedDirection}, targetIndex: {targetIndex}");

            foreach (var partPos in container.GetPartPositions())
            {
                var partIndex = containerIndex + partPos;
                Debug.Log($"[TryGetProduction] partPos: {partPos}, partIndex: {partIndex}, targetIndex: {targetIndex}, match: {partIndex == targetIndex}");
                if (partIndex != targetIndex)
                {
                    continue;
                }
                productionLine = line;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Xoay direction vector theo RotationType (dùng Quaternion Unity quay quanh trục Y trên mặt phẳng XZ).
    /// </summary>
    private Vector2Int RotateDirection(Vector2Int direction, RotationType rotationType)
    {
        float angle = RotationTypeExtensions.ConvertToAngle(rotationType);
        if (Mathf.Approximately(angle, 0f))
        {
            return direction;
        }

        // Dùng Quaternion giống cách Container rotate part positions (XZ plane, Y-axis rotation)
        Quaternion rotation = Quaternion.Euler(0, angle, 0);
        Vector3 rotated = rotation * new Vector3(direction.x, 0, direction.y);
        return new Vector2Int(Mathf.RoundToInt(rotated.x), Mathf.RoundToInt(rotated.z));
    }

    public void SplitAndReplaceContainer(Container oldContainer, List<SplitContainerData> splitDatas)
    {
        var cell = grid.WorldToCell(oldContainer.transform.position);
        var productionsByCell = DetachProductionsByCell(oldContainer);

        containerPooler.Despawn(oldContainer);
        oldContainer.gameObject.SetActive(false);

        float delay = 0f;

        foreach (var splitData in splitDatas)
        {
            Vector2Int offset = ContainerSaveDataExtensions.TransformLocalPosition(oldContainer.Data, splitData.Position);

            RotationType finalRotation = RotationTypeExtensions.Add(oldContainer.Data.rotationType, splitData.rotationType); 

            var newContainerData = new ContainerData();
            newContainerData.containerShapeType = splitData.containerShapeType;
            newContainerData.containerMaterialType = oldContainer.Data.containerData.containerMaterialType;
            newContainerData.containerMovementType = oldContainer.Data.containerData.containerMovementType;
            
            newContainerData.containerColorData = new ContainerColorData() { colorType = oldContainer.Data.containerData.containerColorData.colorType };
            newContainerData.containerIceData = new ContainerIceData() { iceAmount = oldContainer.Data.containerData.containerIceData.iceAmount };
            newContainerData.containerBoombData = new ContainerBoombData() { boombAmount = oldContainer.Data.containerData.containerBoombData.boombAmount };
            newContainerData.containerKeyData = new ContainerKeyData() { keyAmount = oldContainer.Data.containerData.containerKeyData.keyAmount };

            var newSaveData = new ContainerSaveData
            {
                position = new SerializableVector2Int(cell + offset),
                rotationType = finalRotation,
                flipX = oldContainer.Data.flipX,
                containerData = newContainerData
            };

            var newContainer = containerFactory.SpawnSingleContainer(newSaveData, containerPooler, grid);
            ReattachProductionsByCell(newContainer, productionsByCell, ref delay);
        }
    }

    private Dictionary<Vector2Int, List<Production>> DetachProductionsByCell(Container container)
    {
        var productionsByCell = new Dictionary<Vector2Int, List<Production>>();
        if (container == null || container.ContainerPlacesList == null)
        {
            return productionsByCell;
        }

        var cell = grid.WorldToCell(container.transform.position);

        foreach (var cp in container.ContainerPlacesList)
        {
            if (cp == null) continue;

            var partCell = cell + ContainerSaveDataExtensions.TransformLocalPosition(container.Data, cp.PartPosition);
            var productions = new List<Production>();
            foreach (var place in cp.Places)
            {
                if (place != null && place.Production != null)
                {
                    productions.Add(place.DetachProduction());
                }
            }
            if (productions.Count > 0)
            {
                productionsByCell[partCell] = productions;
            }
        }

        return productionsByCell;
    }

    private void ReattachProductionsByCell(Container container, Dictionary<Vector2Int, List<Production>> productionsByCell, ref float delay)
    {
        if (container == null || container.ContainerPlacesList == null || productionsByCell.Count == 0)
        {
            return;
        }

        var cell = grid.WorldToCell(container.transform.position);

        // Đếm tổng số lượng animation sẽ chạy trên container này
        int totalAnimations = 0;
        foreach (var cp in container.ContainerPlacesList)
        {
            if (cp == null) continue;
            var partCell = cell + ContainerSaveDataExtensions.TransformLocalPosition(container.Data, cp.PartPosition);
            if (productionsByCell.TryGetValue(partCell, out var productions) && productions != null)
            {
                totalAnimations += Mathf.Min(cp.Places.Count, productions.Count);
            }
        }

        int landedCount = 0;

        foreach (var cp in container.ContainerPlacesList)
        {
            if (cp == null) continue;

            var partCell = cell + ContainerSaveDataExtensions.TransformLocalPosition(container.Data, cp.PartPosition);
            if (!productionsByCell.TryGetValue(partCell, out var productions) || productions == null)
            {
                continue;
            }

            int count = Mathf.Min(cp.Places.Count, productions.Count);
            for (int i = 0; i < count; i++)
            {
                var production = productions[i];
                var place = cp.Places[i];
                if (production == null || place == null) continue;

                production.transform.DOKill();
                production.gameObject.SetActive(true);
                
                // Giữ nguyên vị trí thế giới khi đổi parent để làm mốc bắt đầu bay
                production.transform.SetParent(place.Pizza, true);

                // Bay nhảy cầu vồng sang vị trí mới trong container phân tách
                var currentContainer = container;
                production.transform.DOLocalJump(Vector3.zero, jumpPower: 1.5f, numJumps: 1, duration: 0.4f)
                    .SetDelay(delay)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() =>
                    {
                        landedCount++;
                        if (landedCount >= totalAnimations)
                        {
                            if (currentContainer != null && currentContainer.IsFull() && !currentContainer.IsFlyingAway)
                            {
                                currentContainer.StateMachine.ChangeToFlyAwayState();
                            }
                        }
                    });
                production.transform.DOLocalRotate(Vector3.zero, 0.4f)
                    .SetDelay(delay)
                    .SetEase(Ease.OutQuad);
                production.transform.DOScale(Vector3.one, 0.4f)
                    .SetDelay(delay)
                    .SetEase(Ease.OutQuad);

                if (place.Pin != null)
                {
                    place.Pin.DOKill();
                    place.Pin.localScale = Vector3.zero;
                }

                place.SetProduction(production);
                delay += 0.15f;
            }
        }
    }

}
