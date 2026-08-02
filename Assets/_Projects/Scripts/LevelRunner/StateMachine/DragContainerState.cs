using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// Lưu trữ dữ liệu tiến trình kéo container.
/// Dùng để theo dõi trạng thái kéo container từ lúc bắt đầu cho đến khi kết thúc.
/// </summary>
public class ProgressDragContainerData
{
    /// <summary>
    /// Container hiện đang được chọn/kéo (null nếu không có container nào được chọn).
    /// </summary>
    public Container selectedContainer;

    /// <summary>
    /// Offset từ vị trí chuột đến tâm của container khi bắt đầu kéo.
    /// Dùng để giữ điểm kéo ở vị trí ban đầu khi di chuột.
    /// </summary>
    public Vector3 dragOffset = Vector3.zero;

    /// <summary>
    /// Vị trí grid ban đầu của container trước khi bắt đầu kéo (dùng để kiểm tra xem có di chuyển hay không).
    /// </summary>
    public Vector3 startGridPos = Vector3.zero;

    /// <summary>
    /// Vị trí target hiện tại mà container đang di chuyển tới (dùng để snap khi thả chuột).
    /// </summary>
    public Vector3 targetMoverPos = Vector3.zero;

    /// <summary>
    /// Lưu trữ các ô grid hợp lệ có thể đi tới để tránh tính toán lại mỗi frame.
    /// </summary>
    public HashSet<Vector2Int> cachedConnectedCells = new();
}

/// <summary>
/// Quản lý trạng thái kéo container trong trò chơi.
/// Xử lý nhập chuột để phát hiện khi bắt đầu kéo, di chuyển container mượt mà trên grid,
/// kiểm tra va chạm góc, và kết thúc kéo với snap về vị trí grid gần nhất hợp lệ.
/// 
/// Tính năng chính:
/// - Phát hiện và bắt đầu kéo container khi nhấn chuột
/// - Cho phép di chuyển mượt mà (smooth drag) với ràng buộc grid
/// - Chống đi chéo cắt góc (corner cutting prevention)
/// - Tính toán các ô grid hợp lệ dựa trên loại di chuyển (ngang, dọc, tự do, khóa)
/// - Xử lý snapping về ô grid gần nhất khi kết thúc kéo
/// - Tự động chuyển products từ production line sang container
/// </summary>
[System.Serializable]
public class DragContainerState : DraftUtils.IState
{
    /// <summary>
    /// Tham chiếu đến LevelRunner để truy cập các thành phần game khác.
    /// </summary>
    private LevelRunner _levelRunner;

    /// <summary>
    /// Dữ liệu tiến trình kéo container hiện tại (chọn, offset, vị trí bắt đầu).
    /// </summary>
    private ProgressDragContainerData progressDragContainerData = new();

    /// <summary>
    /// Thiết lập tham chiếu LevelRunner cho state này.
    /// </summary>
    /// <param name="levelRunner">LevelRunner chứa dữ liệu game chính</param>
    public void SetLevelRunner(LevelRunner levelRunner)
    {
        _levelRunner = levelRunner;
    }

    /// <summary>
    /// Gọi mỗi frame physics. Không sử dụng trong state kéo container này.
    /// </summary>
    public void FixedUpdate()
    {
    }

    /// <summary>
    /// Được gọi khi vào state kéo container. Khởi tạo dữ liệu tiến trình kéo.
    /// </summary>
    public void OnEnter()
    {
        progressDragContainerData.selectedContainer = null;
    }

    /// <summary>
    /// Được gọi khi rời khỏi state kéo container. Kết thúc kéo nếu container đang được kéo.
    /// </summary>
    public void OnExit()
    {
        if (progressDragContainerData.selectedContainer != null)
        {
            EndDrag(progressDragContainerData, _levelRunner);
        }
    }

    /// <summary>
    /// Gọi mỗi frame để xử lý input chuột và quản lý trạng thái kéo.
    /// 
    /// Quy trình:
    /// 1. GetMouseButtonDown(0): Phát hiện container dưới chuột, bắt đầu kéo nếu có thể di chuyển
    /// 2. GetMouseButton(0): Cập nhật vị trí container theo chuột (smooth drag)
    /// 3. GetMouseButtonUp(0): Kết thúc kéo, snap về vị trí grid hợp lệ gần nhất
    /// </summary>
    public void Update()
    {
        // Phát hiện bắt đầu kéo: nhấn chuột trái
        if (Input.GetMouseButtonDown(0))
        {
            if (TryBeginDragAtScreenPoint(Input.mousePosition, Camera.main))
            {
                return;
            }
        }

        // Xử lý kéo container: giữ nút chuột
        if (Input.GetMouseButton(0))
        {
            Dragging(progressDragContainerData);
        }

        // Kết thúc kéo: thả nút chuột
        if (Input.GetMouseButtonUp(0))
        {
            EndDrag(progressDragContainerData, _levelRunner);
        }
    }

    /// <summary>
    /// Finds the nearest container under a screen point without depending on
    /// which overlapping gameplay collider Physics.Raycast returns first.
    /// Tile colliders are rebuilt by the Add Tile skill and can otherwise
    /// cover container colliders on some devices.
    /// </summary>
    internal bool TryFindDraggableContainerAtScreenPoint(
        Vector3 screenPosition,
        Camera camera,
        out Container container)
    {
        if (!TryGetContainerUnderScreenPoint(screenPosition, camera, out container) ||
            container == null ||
            !ContainerDataUtils.CanMoving(container.Data.containerData) ||
            container.IsFull())
        {
            container = null;
            return false;
        }

        return true;
    }

    internal bool TryBeginDragAtScreenPoint(Vector3 screenPosition, Camera camera)
    {
        if (!TryFindDraggableContainerAtScreenPoint(screenPosition, camera, out var container))
        {
            return false;
        }

        BeginDrag(progressDragContainerData, container);
        return true;
    }

    internal void BeginDrag(Container container)
    {
        BeginDrag(progressDragContainerData, container);
    }

    private static bool TryGetContainerUnderScreenPoint(
        Vector3 screenPosition,
        Camera camera,
        out Container container)
    {
        container = null;
        if (camera == null)
        {
            return false;
        }

        var ray = camera.ScreenPointToRay(screenPosition);
        var hits = Physics.RaycastAll(
            ray,
            camera.farClipPlane,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore);

        foreach (var hit in hits.OrderBy(hit => hit.distance))
        {
            var candidate = hit.collider.GetComponentInParent<Container>();
            if (candidate == null)
            {
                continue;
            }

            container = candidate;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Xử lý di chuyển container mượt mà theo chuột trong khi kéo.
    /// 
    /// Quy trình:
    /// 1. Lấy vị trí thế giới (world position) của chuột
    /// 2. Trừ dragOffset để tìm vị trí mục tiêu của container
    /// 3. Tính toán các ô grid hợp lệ có thể di chuyển tới (dựa trên loại di chuyển)
    /// 4. Ép vị trí mục tiêu vào phạm vi ô grid hợp lệ (clamping)
    /// 5. Di chuyển container mượt mà đến vị trí mục tiêu
    /// 
    /// Chống corner cutting: Nếu container cố di chuyển chéo qua góc bị chặn,
    /// buộc trượt sát vào một bức tường thay vì đi xiên.
    /// </summary>
    /// <param name="progressData">Dữ liệu tiến trình kéo chứa container được chọn và offset</param>
    private void Dragging(ProgressDragContainerData progressData)
    {
        if (progressData.selectedContainer == null)
        {
            return;
        }

        var worldPoint = DraftUtils.Utils.CameraInput.GetMouseWorldPositionAtY(Input.mousePosition, 0);
        var targetPos = worldPoint - progressData.dragOffset;
        targetPos.y = DataManager.Instance.ParametterGameConfigSO.DragContainerBonusY;

        var spawner = _levelRunner.LevelObjectSpawner;
        var grid = spawner.Grid;
        var container = progressData.selectedContainer;

        // Use cached connected cells instead of recalculating everything every frame
        targetPos = ClampTargetPosition(container, targetPos, grid, progressData.cachedConnectedCells, progressData.startGridPos, progressData.targetMoverPos);

        container.StateMachine.MoveToPositionState.SmoothMover.SetTargetPosition(targetPos);
        container.StateMachine.MoveToPositionState.SmoothMover.StartMoving();
        progressData.targetMoverPos = targetPos;
    }



    /// <summary>
    /// Thực thi thuật toán loang (BFS - Breadth-First Search) để tìm toàn bộ các ô grid hợp lệ
    /// có thể đi tới được từ ô bắt đầu, chỉ di chuyển theo các hướng cho phép và không đâm xuyên container khác.
    /// 
    /// Thuật toán:
    /// 1. Khởi tạo hàng đợi với ô bắt đầu
    /// 2. Với mỗi ô trong hàng đợi, kiểm tra 4 ô lân cận (theo hướng được phép)
    /// 3. Nếu ô lân cận chưa được khám phá và hợp lệ (không chứa vật khác), thêm vào hàng đợi
    /// 4. Trả về tập hợp tất cả các ô hợp lệ
    /// </summary>
    /// <param name="startCell">Ô grid bắt đầu loang</param>
    /// <param name="directions">Danh sách hướng di chuyển được phép (up, down, left, right)</param>
    /// <param name="partPositions">Các vị trí các phần của container (các ô bị chiếm bởi container)</param>
    /// <param name="availableGridSet">Tập hợp các ô trống hiện có trên bàn chơi</param>
    /// <returns>Tập hợp các ô grid hợp lệ có thể đi tới được</returns>
    private HashSet<Vector2Int> ExecuteBFSForValidCells(Vector2Int startCell, List<Vector2Int> directions, IEnumerable<Vector2Int> partPositions, HashSet<Vector2Int> availableGridSet)
    {
        var connectedCells = new HashSet<Vector2Int>();
        var queue = new Queue<Vector2Int>();

        queue.Enqueue(startCell);
        connectedCells.Add(startCell);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var dir in directions)
            {
                var neighbor = current + dir;
                if (!connectedCells.Contains(neighbor) && IsPlacementValid(neighbor, partPositions, availableGridSet))
                {
                    connectedCells.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }
        return connectedCells;
    }

    /// <summary>
    /// Tìm tất cả các ô grid hợp lệ có thể đi tới được từ vị trí bắt đầu mà không bị cản bởi vật khác,
    /// dựa trên loại di chuyển của container (Horizontal/Vertical/Free/Blocked).
    /// 
    /// Sử dụng BFS để khám phá tất cả các ô kết nối từ ô bắt đầu.
    /// </summary>
    /// <param name="container">Container đang được xét (chứa dữ liệu loại di chuyển và vị trí các phần)</param>
    /// <param name="grid">Grid của game để chuyển đổi giữa tọa độ thế giới và tọa độ grid</param>
    /// <param name="startGridPos">Vị trí thế giới ban đầu của container lúc bắt đầu kéo</param>
    /// <param name="availableGridSet">Tập hợp các ô trống hiện có trên bàn chơi</param>
    /// <returns>Tập hợp các ô tọa độ grid mà container có thể trượt tới được (kết nối)</returns>
    private HashSet<Vector2Int> GetConnectedValidBaseCells(Container container, DraftUtils.GridXZ grid, Vector3 startGridPos, HashSet<Vector2Int> availableGridSet)
    {
        var startCell = grid.WorldToCell(startGridPos);

        if (!IsPlacementValid(startCell, container.GetPartPositions(), availableGridSet))
        {
            return new HashSet<Vector2Int>();
        }

        var directions = ContainerMovementTypeExtensions.GetAllowedDirections(container.Data.containerData.containerMovementType);
        return ExecuteBFSForValidCells(startCell, directions, container.GetPartPositions(), availableGridSet);
    }

    /// <summary>
    /// Tính toán giới hạn của vùng không gian hợp lệ xung quanh ô grid gần nhất,
    /// và ép (clamp) vị trí mục tiêu để container trượt mượt mà nhưng KHÔNG đi chéo hay đâm xuyên các container khác.
    /// 
    /// Quy trình:
    /// 1. Tìm ô grid gần nhất với vị trí container hiện tại
    /// 2. Xác định các ô lân cận hợp lệ (có thể di chuyển tới)
    /// 3. Tính toán phạm vi X và Z hợp lệ dựa trên các ô lân cận
    /// 4. Ép vị trí mục tiêu vào phạm vi này
    /// 5. Chống corner cutting: Nếu container cố đi chéo qua góc bị chặn, buộc trượt sát vào tường
    /// </summary>
    /// <param name="container">Container đang được kéo</param>
    /// <param name="targetPos">Vị trí mục tiêu cần ép (dựa trên chuột)</param>
    /// <param name="grid">Grid để chuyển đổi tọa độ</param>
    /// <param name="connectedCells">Tập hợp các ô grid hợp lệ có thể di chuyển tới</param>
    /// <param name="startGridPos">Vị trí bắt đầu kéo (dùng để fallback nếu không có ô hợp lệ)</param>
    /// <returns>Vị trí mục tiêu sau khi ép vào phạm vi hợp lệ</returns>
    private Vector3 ClampTargetPosition(Container container, Vector3 targetPos, DraftUtils.GridXZ grid, HashSet<Vector2Int> connectedCells, Vector3 startGridPos, Vector3 referencePos)
    {
        if (connectedCells.Count == 0)
        {
            var fallback = grid.CellToWorld(grid.WorldToCell(startGridPos));
            fallback.y = targetPos.y;
            return fallback;
        }

        var currentWorldPos = referencePos;
        var nearestCell = FindNearestBaseCell(currentWorldPos, grid, connectedCells);
        var nearestWorld = grid.CellToWorld(nearestCell);

        float minX = nearestWorld.x, maxX = nearestWorld.x;
        float minZ = nearestWorld.z, maxZ = nearestWorld.z;

        if (connectedCells.Contains(nearestCell + Vector2Int.left)) minX = grid.CellToWorld(nearestCell + Vector2Int.left).x;
        if (connectedCells.Contains(nearestCell + Vector2Int.right)) maxX = grid.CellToWorld(nearestCell + Vector2Int.right).x;
        if (connectedCells.Contains(nearestCell + Vector2Int.down)) minZ = grid.CellToWorld(nearestCell + Vector2Int.down).z;
        if (connectedCells.Contains(nearestCell + Vector2Int.up)) maxZ = grid.CellToWorld(nearestCell + Vector2Int.up).z;

        // Trượt tự do (smooth drag): không tự động khóa cứng trục nếu không cần thiết
        targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
        targetPos.z = Mathf.Clamp(targetPos.z, minZ, maxZ);

        // Chống cắt góc (corner cutting) cho container Walkable đi 2D:
        // Nếu container đang bị kéo chéo (cả X và Z đều di chuyển khỏi ô trung tâm)
        float dx = targetPos.x - nearestWorld.x;
        float dz = targetPos.z - nearestWorld.z;

        if (Mathf.Abs(dx) > 0.01f && Mathf.Abs(dz) > 0.01f)
        {
            int signX = (int)Mathf.Sign(dx);
            int signZ = (int)Mathf.Sign(dz);
            Vector2Int diagonalCell = nearestCell + new Vector2Int(signX, signZ);

            // Nếu ô góc chéo bị chặn, ta không thể đi chéo qua góc đó (sẽ bị chờm lên container ở góc)
            // Do đó phải ép xe trượt sát vào 1 trong 2 bức tường thay vì đi xiên.
            if (!connectedCells.Contains(diagonalCell))
            {
                if (Mathf.Abs(dx) > Mathf.Abs(dz))
                {
                    targetPos.z = nearestWorld.z; // Trượt theo trục X
                }
                else
                {
                    targetPos.x = nearestWorld.x; // Trượt theo trục Z
                }
            }
        }

        return targetPos;
    }

    /// <summary>
    /// Tìm vị trí ô grid gần nhất với vị trí mục tiêu trong tập hợp các ô hợp lệ.
    /// Dùng để xác định ô "trung tâm" của container cho mục đích ép vị trí.
    /// </summary>
    /// <param name="targetPos">Vị trí thế giới mục tiêu (chuột hoặc container)</param>
    /// <param name="grid">Grid để chuyển đổi tọa độ</param>
    /// <param name="connectedCells">Tập hợp các ô grid hợp lệ để tìm kiếm</param>
    /// <returns>Tọa độ grid của ô gần nhất</returns>
    private Vector2Int FindNearestBaseCell(Vector3 targetPos, DraftUtils.GridXZ grid, HashSet<Vector2Int> connectedCells)
    {
        Vector2Int nearestBaseCell = default;
        float nearestDistance = float.MaxValue;

        foreach (var baseCell in connectedCells)
        {
            var baseCellWorldPos = grid.CellToWorld(baseCell);
            var distance = (baseCellWorldPos - targetPos).sqrMagnitude;
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestBaseCell = baseCell;
            }
        }
        return nearestBaseCell;
    }

    /// <summary>
    /// Kiểm tra xem vị trí một ô grid có hợp lệ không (tất cả các phần của container có chỗ trống).
    /// 
    /// Một vị trí là hợp lệ nếu tất cả các ô chiếm bởi các phần của container đều trong tập hợp ô trống.
    /// </summary>
    /// <param name="baseCell">Ô grid cơ sở (vị trí của container)</param>
    /// <param name="partPositions">Các vị trí tương đối của các phần container</param>
    /// <param name="availableSet">Tập hợp các ô trống hiện có</param>
    /// <returns>true nếu tất cả các phần có chỗ trống, false nếu có phần bị chặn</returns>
    private bool IsPlacementValid(Vector2Int baseCell, IEnumerable<Vector2Int> partPositions, HashSet<Vector2Int> availableSet)
    {
        foreach (var partPos in partPositions)
        {
            if (!availableSet.Contains(baseCell + partPos))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Bắt đầu kéo container. Khởi tạo dữ liệu tiến trình kéo và nâng container lên theo bonus Y.
    /// 
    /// Quy trình:
    /// 1. Lưu lại container được chọn và vị trí bắt đầu
    /// 2. Nâng container lên theo giá trị DragContainerBonusY
    /// 3. Tính toán offset từ chuột đến tâm container (để giữ điểm kéo)
    /// </summary>
    /// <param name="progresData">Dữ liệu tiến trình kéo cần khởi tạo</param>
    /// <param name="container">Container bắt đầu kéo</param>
    internal void BeginDrag(ProgressDragContainerData progresData, Container container)
    {
        progresData.selectedContainer = container;
        progresData.startGridPos = container.transform.position;

        var newPos = progresData.selectedContainer.transform.position;
        // Giữ vị trí hiện tại khi bắt đầu kéo (không snap); chỉ nâng lên theo bonus Y
        newPos.y = DataManager.Instance.ParametterGameConfigSO.DragContainerBonusY;

        progresData.selectedContainer.StateMachine.MoveToPositionState.SmoothMover.SetTargetPosition(newPos);
        progresData.selectedContainer.StateMachine.MoveToPositionState.SmoothMover.StartMoving();

        var worldPoint = DraftUtils.Utils.CameraInput.GetMouseWorldPositionAtY(Input.mousePosition, 0);
        progresData.dragOffset = worldPoint - progresData.selectedContainer.transform.position;
        progresData.targetMoverPos = newPos;

        // Calculate and cache connected cells once here
        var spawner = _levelRunner.LevelObjectSpawner;
        var availableGridPositions = spawner.GetAvailableGridPositionsIgnore(container);
        var availableGridSet = new HashSet<Vector2Int>(availableGridPositions ?? new List<Vector2Int>());
        progresData.cachedConnectedCells = GetConnectedValidBaseCells(container, spawner.Grid, progresData.startGridPos, availableGridSet);
    }

    /// <summary>
    /// Kết thúc kéo container. Snap về ô grid gần nhất hợp lệ, cập nhật số lần kéo,
    /// và tự động chuyển products từ production line sang container nếu có.
    /// 
    /// Quy trình:
    /// 1. Tìm ô grid hợp lệ gần nhất với vị trí container hiện tại
    /// 2. Di chuyển container mượt mà đến ô grid đó
    /// 3. Cập nhật số lần kéo nếu container thực sự di chuyển
    /// 4. Tìm production line gần container và chuyển products có cùng màu
    /// 5. Chạy animation chuyển động products
    /// </summary>
    /// <param name="progressData">Dữ liệu tiến trình kéo chứa container được chọn</param>
    /// <param name="levelRunner">LevelRunner để truy cập spawner, grid, tracking...</param>
    internal void EndDrag(ProgressDragContainerData progressData, LevelRunner levelRunner)
    {
        if (progressData.selectedContainer == null)
        {
            return;
        }

        var connectedCells = progressData.cachedConnectedCells;

        // Chống đâm xuyên đường chéo: Snap về vị trí target gần nhất của container thay vì vị trí visual đang di chuyển dở dang
        var currentWorldPos = progressData.targetMoverPos;
        var nearestCell = FindNearestBaseCell(currentWorldPos, levelRunner.LevelObjectSpawner.Grid, connectedCells);

        var worldPos = levelRunner.LevelObjectSpawner.Grid.CellToWorld(nearestCell);
        worldPos.y = 0;

        var smoothMover = progressData.selectedContainer.StateMachine.MoveToPositionState.SmoothMover;
        smoothMover.SetTargetPosition(worldPos);
        smoothMover.StartMoving();

        // Chỉ tăng số lần kéo khi container thực sự được di chuyển sang một ô grid mới
        if (Vector3.Distance(worldPos, progressData.startGridPos) > 0.01f)
        {
            levelRunner.LevelTracking.dragContainerTimes.SetValue(levelRunner.LevelTracking.dragContainerTimes.Value + 1);
            levelRunner.LevelTracking.dragContainerTimes.Notifier.Notify();
        }

        if (_levelRunner.LevelObjectSpawner.TryGetProductionNearAndSamleColor(progressData.selectedContainer, nearestCell, out var productionLine))
        {
            var color = productionLine.ProductionPooler.ActiveItems[0].ColorType;

            // Double-check: container phải cùng màu với production đầu tiên trên line
            if (!ProductionLineRuntimeDataExensions.HasFirstProductionColor(productionLine.Data, color))
            {
                progressData.selectedContainer = null;
                return;
            }

            var sortedEmptyPlaces = progressData.selectedContainer.GetEmptyPlacesForColor(color);

            var firstColors = ProductionLineRuntimeDataExensions.GetFirstColors(productionLine.Data, color);

            var numberToRelease = Mathf.Min(sortedEmptyPlaces.Count, firstColors.Count());
            Debug.Log($"Number to release: {numberToRelease}, Max Production On Line: {DataManager.Instance.ParametterGameConfigSO.MaxProductionOnLine}");

            // Spawn thêm production ra line trước khi move
            productionLine.Creat(numberToRelease + DataManager.Instance.ParametterGameConfigSO.MaxProductionOnLine);

            // Lấy chính xác số lượng production cần move
            var allProductionInLineColorAsContainer = productionLine.GetAllProductionInLineSampleColorAsContainer(color, productionLine)
                .GetRange(0, numberToRelease);

            MoveProductionsToContainerLogic(productionLine, progressData.selectedContainer, color);
            _levelRunner.StartCoroutine(AnimationMove(progressData.selectedContainer, productionLine, allProductionInLineColorAsContainer, sortedEmptyPlaces));
        }
        progressData.selectedContainer = null;
    }

    /// <summary>
    /// Chuyển các products có cùng màu từ production line sang container khi container được đặt gần nó.
    /// 
    /// Quy trình:
    /// 1. Lọc các products từ production line có cùng màu với container
    /// 2. Lấy các vị trí trống trong container (sắp xếp xa nhất từ production line trước)
    /// 3. Di chuyển mỗi product vào một vị trí trống
    /// 4. Thực hiện animation động để hiển thị quá trình chuyển động
    /// 5. Cập nhật màu sắc production line
    /// 6. Kiểm tra nếu container đầy sau animation, gửi container bay đi
    /// </summary>
    /// <param name="productionLine">Production line là nguồn của products</param>
    /// <param name="container">Container là đích đến của products</param>
    private void MoveProductionsToContainerLogic(ProductionLine productionLine, Container container, ColorType color)
    {
        if (container.Places == null)
        {
            return;
        }
        if (container.Places.Count == 0)
        {
            return;
        }

        List<Production> productions = productionLine.GetAllProductionInLineSampleColorAsContainer(color, productionLine);
        if (productions.Count() == 0)
        {
            return;
        }

        // Sắp xếp các vị trí trống theo thứ tự: trái → phải, trên → dưới
        var sortedPlaces = container.GetEmptyPlacesForColor(color);

        if (sortedPlaces.Count == 0)
        {
            return;
        }

        int numberProductionsToMove = Mathf.Min(productions.Count, sortedPlaces.Count);

        for (int i = 0; i < numberProductionsToMove; i++)
        {
            var production = productions[i];
            var place = sortedPlaces[i];
            place.SetProduction(production);

            // Remove from ActiveItems instead of Despawn so the pool doesn't steal it while animating
            productionLine.ProductionPooler.Despawn(production);

            // Xóa màu đã dùng khỏi dữ liệu để đảm bảo mapping đúng cho các lần Spawn kế tiếp
            if (productionLine.Data.productionColors.Count > 0)
            {
                productionLine.Data.productionColors.RemoveAt(0);
            }
        }
        productionLine.ChangeProductionLineColor();
        if (container.IsFull() && !container.HasNextColorLayer())
        {
            container.ContainerView.HideAll();
            _levelRunner.LevelObjectSpawner.ContainerPooler.Despawn(container);
        }
    }


    /// <summary>
    /// Chạy animation để hiển thị quá trình chuyển động của products từ production line sang container.
    /// 
    /// Chi tiết animation:
    /// - Mỗi product nhảy vào vị trí của nó với arc (cung tròn) để tạo hiệu ứng nhảy 3D
    /// - Các products khác trong production line dịch chuyển sang bên (shift) theo thứ tự
    /// - Tất cả animations chạy với độ trễ (stagger) để tạo hiệu ứng tuần tự
    /// - Sau cùng, kiểm tra nếu container đầy thì gửi container bay đi (FlyAwayState)
    /// 
    /// Parameters cho animation:
    /// - duration: Thời gian mỗi product nhảy vào (từ DragContainerDuration)
    /// - stepWait: Độ trễ giữa mỗi product (wait / 3)
    /// </summary>
    /// <param name="productionLine">Production line để thực hiện animation dịch chuyển products còn lại</param>
    /// <param name="productions">Danh sách products được di chuyển sang container</param>
    /// <param name="places">Danh sách vị trí (Place) trong container để đặt products</param>
    /// <returns>IEnumerator cho coroutine</returns>
    private IEnumerator AnimationMove(Container container, ProductionLine productionLine, List<Production> productions, List<ContainerPlace> places)
    {
        container.isAnimating = true;
        yield return container.StateMachine.MoveToPositionState.WaitMoveCompletedCouroutin(20f);
        var duration = DataManager.Instance.ParametterGameConfigSO.DragContainerDuration;
        var wait = DataManager.Instance.ParametterGameConfigSO.AnimationWait;
        var stepWait = wait / 3f;
        int numberProductionsToMove = Mathf.Min(productions.Count, places.Count);

        for (int i = 0; i < numberProductionsToMove; i++)
        {
            var production = productions[i];
            var targetPlace = places[i];

            // Dừng bất kỳ animation nào đang chạy trên product này
            production.transform.DOKill();
            production.transform.SetParent(targetPlace.Pizza, true);

            // Scale pin về 0 khi production gần tiếp đất
            if (targetPlace.Pin != null)
            {
                targetPlace.Pin.DOKill();
                targetPlace.Pin.DOScale(Vector3.zero, duration * 0.3f)
                    .SetDelay(duration * 0.7f)
                    .SetEase(Ease.OutQuad);
            }

            var startLocal = production.transform.localPosition;

            // Tạo animation nhảy: production nhảy lên cao với arc parabola rồi rơi xuống vị trí đích
            var capturedStartLocal = startLocal;
            var capturedProduction = production;
            var capturedTargetPlace = targetPlace;
            var startRotation = capturedProduction.transform.localRotation;

            DOVirtual.Float(0, 1, duration, t =>
            {
                if (capturedProduction != null && capturedProduction.transform != null)
                {
                    // Interpolate local position từ vị trí ban đầu đến Vector3.zero (vị trí cuối)
                    var linearPos = Vector3.Lerp(capturedStartLocal, Vector3.zero, t);
                    capturedProduction.transform.localPosition = linearPos;

                    // Cộng arc vào world position (trục Y world) để không bị ảnh hưởng bởi rotation của parent
                    var jumpArc = Mathf.Sin(t * Mathf.PI) * 3f;
                    capturedProduction.transform.position += new Vector3(0, jumpArc, 0);

                    // Scale lên khi ở đỉnh arc (gần camera hơn), rồi về lại 1
                    float scaleBonus = Mathf.Sin(t * Mathf.PI) * 1f;
                    capturedProduction.transform.localScale = Vector3.one * (1f + scaleBonus);

                    // Rotate local về identity (0,0,0) chỉ khi gần tiếp đất (t > 0.8)
                    float threshold = 0.8f;
                    float tRotation = t > threshold ? (t - threshold) / (1f - threshold) : 0f;
                    capturedProduction.transform.localRotation = Quaternion.Slerp(startRotation, Quaternion.identity, tRotation);

                    // Set blend shape weight for Pizza_Expand
                    float tBlendThreshold = 0.6f;
                    float tBlend = t > tBlendThreshold ? (t - tBlendThreshold) / (1f - tBlendThreshold) : 0f;
                    capturedProduction.SetBlendShapeWeight("Pizza_Expand", 100 - tBlend * 100f);
                }
            }).SetTarget(capturedProduction.transform).SetEase(DataManager.Instance.ParametterGameConfigSO.ProductionEase)
            .OnComplete(() =>
            {
                VibrationManager.Vibrate(VibrationType.ItemPlaced);

                if (container != null && container.transform != null)
                {
                    if (container.IsFlyingAway)
                        return;

                    container.transform.DOKill(true);
                    container.transform.DOScale(Vector3.one * 1.02f, 0.01f)
                        .SetEase(Ease.OutQuad)
                        .OnComplete(() =>
                        {
                            container.transform.DOScale(Vector3.one, 0.12f).SetEase(Ease.OutQuad);
                        });
                }
            });

            // Chờ stepWait trước khi di chuyển product tiếp theo
            yield return new WaitForSeconds(stepWait);

            // Dịch chuyển (shift) các products chưa bay vào container dần dần về đầu line
            for (int k = i + 1; k < numberProductionsToMove; k++)
            {
                var futureProduction = productions[k];
                futureProduction.transform.DOKill();
                futureProduction.CurrentIndex--;
                productionLine.AnimateProductionToShift(futureProduction, futureProduction.CurrentIndex, stepWait);
            }

            // Dịch chuyển (shift) tất cả các products còn lại trong production line sang bên dần dần
            for (int j = 0; j < productionLine.ProductionPooler.ActiveItems.Count; j++)
            {
                var otherProduction = productionLine.ProductionPooler.ActiveItems[j];
                otherProduction.transform.DOKill();
                otherProduction.CurrentIndex--;
                productionLine.AnimateProductionToShift(otherProduction, otherProduction.CurrentIndex, stepWait);
            }
        }

        // Chờ item cuối cùng nhảy xong hẳn rồi mới bay container
        float remainingTime = duration - stepWait;
        if (remainingTime > 0)
        {
            yield return new WaitForSeconds(remainingTime);
        }

        // Đợi thêm một chút cho sản phẩm cuối hoàn toàn tiếp đất và ổn định
        yield return new WaitForSeconds(0.2f);

        if (container.IsFull() && container.TryAdvanceColorLayer())
        {
            VibrationManager.Vibrate(VibrationType.Completion);

            // Coffee Run counts a completed LayerBox layer as one resolved box for
            // Ice unlock thresholds, even though the physical container remains.
            _levelRunner.LevelTracking.resolvedContainer.SetValue(
                _levelRunner.LevelTracking.resolvedContainer.Value + 1);
            _levelRunner.LevelTracking.resolvedContainer.Notifier.Notify();
            container.isAnimating = false;
        }
        else if (container.IsFull())
        {
            container.StateMachine.ChangeToFlyAwayState();
        }
        else
        {
            container.isAnimating = false;
        }
    }
}
