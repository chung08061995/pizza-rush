using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(
    fileName = "GameplayVisualConfig",
    menuName = "Pizza Rush/Gameplay Visual Config")]
internal sealed class GameplayVisualConfigSO : ScriptableObject
{
    private const string BaseColorProperty = "_BaseColor";
    private const string LegacyColorProperty = "_Color";

    [Header("Camera framing")]
    [SerializeField, Range(45f, 80f)] private float cameraPitch = 80f;
    [SerializeField] private Vector3 cameraPosition = new(4.5f, 25f, 0f);
    [SerializeField, Min(0.1f)] private float fixedOrthographicSize = 8.563546f;
    [SerializeField] private bool lockCameraTransform;
    [SerializeField, Min(0f)] private float framingPaddingCells = 0f;
    [SerializeField, Min(0f)] private float powerupReserveCells = 1f;
    [SerializeField, Range(0f, 0.25f)] private float safeLeft;
    [SerializeField, Range(0f, 0.25f)] private float safeRight;
    [SerializeField, Range(0f, 0.35f)] private float safeTop = 0.13f;
    [SerializeField, Range(0f, 0.35f)] private float safeBottom = 0.18f;
    [SerializeField, Min(0.1f)] private float minimumOrthographicSize = 0.1f;
    [SerializeField, Min(0.1f)] private float maximumOrthographicSize = 18f;

    [Header("Kitchen background")]
    [SerializeField] private string kitchenBackgroundObjectName = "PR3D_KitchenBackground_Preview";
    [SerializeField] private string kitchenBackgroundAnchorPath = "KitchenFixtures/Counter";
    [SerializeField, Min(0f)] private float kitchenBackgroundGapCells = 1.22f;

    [Header("Directional light")]
    [SerializeField] private Color mainLightColor = Color.white;
    [SerializeField, Min(0f)] private float mainLightIntensity = 1f;
    [SerializeField] private Vector3 mainLightRotation = new(50f, -30f, 0f);
    [SerializeField, Range(0f, 1f)] private float hardShadowStrength = 1f;

    [Header("Ambient gradient")]
    [SerializeField] private Color ambientSky = Rgb(0x1C, 0x4B, 0xCF);
    [SerializeField] private Color ambientEquator = Rgb(0xA7, 0x89, 0xA8);
    [SerializeField] private Color ambientGround = new(0.047f, 0.043f, 0.035f, 1f);
    [SerializeField, Range(0f, 2f)] private float ambientIntensity = 1f;

    [Header("Environment palette")]
    [SerializeField] private Color cameraBackground = Rgb(0x31, 0x4D, 0x79);
    [SerializeField] private Color floor = new(0.66760784f, 0.7199137f, 0.74509805f, 1f);
    [SerializeField] private Color boardTile = new(0.88699996f, 0.88699996f, 0.88699996f, 1f);
    [SerializeField] private Color boardGround = new(0.88699996f, 0.88699996f, 0.88699996f, 1f);
    [SerializeField] private Color boardBorder = new(0.88699996f, 0.88699996f, 0.88699996f, 1f);
    [SerializeField] private Color conveyorRoad = new(0.7294118f, 0.7294118f, 0.8235295f, 1f);
    [SerializeField] private Color conveyorBorder = new(0.007843138f, 0.8313726f, 0.86666673f, 1f);

    [Header("Board grid - Option F")]
    [SerializeField] private Color boardGrid = new(0.58f, 0.31f, 0.14f, 1f);
    [SerializeField, Range(0f, 0.45f)] private float boardGridInsetCells = 0.075f;
    [SerializeField, Range(0.005f, 0.25f)] private float boardGridLineWidthCells = 0.02f;
    [SerializeField, Min(0f)] private float boardGridHeightOffset = 0.21f;

    [Header("Authoritative assets")]
    [SerializeField] private VolumeProfile gameplayVolumeProfile;
    [SerializeField] private Material floorMaterial;
    [SerializeField] private Material[] boardTileMaterials;
    [SerializeField] private Material[] boardGroundMaterials;
    [SerializeField] private Material[] boardBorderMaterials;
    [SerializeField] private Material conveyorRoadMaterial;
    [SerializeField] private Material conveyorBorderMaterial;

    internal float FramingPaddingCells => framingPaddingCells;
    internal float PowerupReserveCells => powerupReserveCells;

    internal void ApplyBoardGrid(LevelObjectSpawner levelObjectSpawner)
    {
        if (levelObjectSpawner == null)
        {
            return;
        }

        levelObjectSpawner.ApplyBoardGridVisual(
            boardGrid,
            boardGridInsetCells,
            boardGridLineWidthCells,
            boardGridHeightOffset);
    }

    internal void AlignKitchenBackground(Bounds boardBounds, float cellSize)
    {
        if (string.IsNullOrWhiteSpace(kitchenBackgroundObjectName))
        {
            return;
        }

        GameObject background = GameObject.Find(kitchenBackgroundObjectName);
        if (background == null)
        {
            return;
        }

        Transform anchor = background.transform.Find(kitchenBackgroundAnchorPath);
        Renderer anchorRenderer = anchor != null ? anchor.GetComponent<Renderer>() : null;
        if (anchorRenderer == null)
        {
            return;
        }

        float targetBackgroundZ = boardBounds.max.z +
                                  kitchenBackgroundGapCells * Mathf.Max(0.01f, cellSize);
        Vector3 position = background.transform.position;
        position.z += targetBackgroundZ - anchorRenderer.bounds.min.z;
        background.transform.position = position;
    }

    internal void ApplyRenderRig(Camera gameplayCamera, Light directionalLight, Volume globalVolume)
    {
        if (gameplayCamera != null)
        {
            gameplayCamera.orthographic = true;
            gameplayCamera.backgroundColor = cameraBackground;
            gameplayCamera.transform.rotation = Quaternion.Euler(cameraPitch, 0f, 0f);
            gameplayCamera.transform.position = cameraPosition;
            if (lockCameraTransform)
            {
                gameplayCamera.orthographicSize = fixedOrthographicSize;
            }
        }

        if (directionalLight != null)
        {
            directionalLight.type = LightType.Directional;
            directionalLight.color = mainLightColor;
            directionalLight.intensity = mainLightIntensity;
            directionalLight.shadows = LightShadows.Soft;
            directionalLight.shadowStrength = hardShadowStrength;
            directionalLight.transform.rotation = Quaternion.Euler(mainLightRotation);
            RenderSettings.sun = directionalLight;
        }

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = ambientSky;
        RenderSettings.ambientEquatorColor = ambientEquator;
        RenderSettings.ambientGroundColor = ambientGround;
        RenderSettings.ambientIntensity = ambientIntensity;

        if (globalVolume != null && gameplayVolumeProfile != null)
        {
            globalVolume.isGlobal = true;
            globalVolume.sharedProfile = gameplayVolumeProfile;
        }

        ApplyPalette();
    }

    internal void FrameCamera(Camera gameplayCamera, Bounds worldBounds, float cellSize)
    {
        if (gameplayCamera == null)
        {
            return;
        }

        gameplayCamera.orthographic = true;
        gameplayCamera.transform.rotation = Quaternion.Euler(cameraPitch, 0f, 0f);
        gameplayCamera.transform.position = cameraPosition;
        if (lockCameraTransform)
        {
            gameplayCamera.orthographicSize = fixedOrthographicSize;
            return;
        }

        float padding = Mathf.Max(0f, framingPaddingCells * Mathf.Max(0.01f, cellSize));
        worldBounds.Expand(new Vector3(padding * 2f, 0f, padding * 2f));
        Vector3 target = new(worldBounds.center.x, 0f, worldBounds.center.z);

        Vector3 forward = gameplayCamera.transform.forward;
        float verticalForward = Mathf.Max(0.01f, -forward.y);
        float distanceToGround = Mathf.Max(1f, (gameplayCamera.transform.position.y - target.y) / verticalForward);
        gameplayCamera.transform.position = target - forward * distanceToGround;

        Vector3 right = gameplayCamera.transform.right;
        Vector3 up = gameplayCamera.transform.up;
        float horizontalHalfExtent = 0f;
        float verticalHalfExtent = 0f;
        foreach (Vector3 corner in GetGroundCorners(worldBounds))
        {
            Vector3 offset = corner - target;
            horizontalHalfExtent = Mathf.Max(horizontalHalfExtent, Mathf.Abs(Vector3.Dot(offset, right)));
            verticalHalfExtent = Mathf.Max(verticalHalfExtent, Mathf.Abs(Vector3.Dot(offset, up)));
        }

        float safeWidth = Mathf.Max(0.2f, 1f - safeLeft - safeRight);
        float safeHeight = Mathf.Max(0.2f, 1f - safeTop - safeBottom);
        float aspect = Mathf.Max(0.1f, gameplayCamera.aspect);
        float sizeForHeight = verticalHalfExtent / safeHeight;
        float sizeForWidth = horizontalHalfExtent / (aspect * safeWidth);
        gameplayCamera.orthographicSize = Mathf.Clamp(
            Mathf.Max(sizeForHeight, sizeForWidth),
            minimumOrthographicSize,
            maximumOrthographicSize);

        ShiftFramingIntoSafeArea(gameplayCamera, target);
    }

    private void ShiftFramingIntoSafeArea(Camera gameplayCamera, Vector3 target)
    {
        float safeCenterX = (safeLeft + (1f - safeRight)) * 0.5f;
        float safeCenterY = (safeBottom + (1f - safeTop)) * 0.5f;
        float viewportOffsetX = safeCenterX - 0.5f;
        float viewportOffsetY = safeCenterY - 0.5f;
        float halfHeight = gameplayCamera.orthographicSize;
        float halfWidth = halfHeight * gameplayCamera.aspect;

        Vector3 groundRight = Vector3.ProjectOnPlane(gameplayCamera.transform.right, Vector3.up).normalized;
        Vector3 groundUp = Vector3.ProjectOnPlane(gameplayCamera.transform.up, Vector3.up).normalized;
        float groundUpProjection = Mathf.Max(
            0.01f,
            Vector3.Dot(groundUp, gameplayCamera.transform.up));
        Vector3 groundShift =
            -groundRight * (viewportOffsetX * halfWidth * 2f) -
            groundUp * (viewportOffsetY * halfHeight * 2f / groundUpProjection);

        Vector3 shiftedTarget = target + groundShift;
        Vector3 forward = gameplayCamera.transform.forward;
        float verticalForward = Mathf.Max(0.01f, -forward.y);
        float distanceToGround = Mathf.Max(
            1f,
            (gameplayCamera.transform.position.y - shiftedTarget.y) / verticalForward);
        gameplayCamera.transform.position = shiftedTarget - forward * distanceToGround;
    }

    private static Vector3[] GetGroundCorners(Bounds bounds)
    {
        return new[]
        {
            new Vector3(bounds.min.x, 0f, bounds.min.z),
            new Vector3(bounds.min.x, 0f, bounds.max.z),
            new Vector3(bounds.max.x, 0f, bounds.min.z),
            new Vector3(bounds.max.x, 0f, bounds.max.z)
        };
    }

    private void ApplyPalette()
    {
        SetMaterialColor(floorMaterial, floor);
        SetMaterialColors(boardTileMaterials, boardTile);
        SetMaterialColors(boardGroundMaterials, boardGround);
        SetMaterialColors(boardBorderMaterials, boardBorder);
        SetMaterialColor(conveyorRoadMaterial, conveyorRoad);
        SetMaterialColor(conveyorBorderMaterial, conveyorBorder);
    }

    private static void SetMaterialColors(Material[] materials, Color color)
    {
        if (materials == null)
        {
            return;
        }

        foreach (Material material in materials)
        {
            SetMaterialColor(material, color);
        }
    }

    private static void SetMaterialColor(Material material, Color color)
    {
        if (material == null)
        {
            return;
        }

        if (material.HasProperty(BaseColorProperty))
        {
            material.SetColor(BaseColorProperty, color);
        }
        else if (material.HasProperty(LegacyColorProperty))
        {
            material.SetColor(LegacyColorProperty, color);
        }
    }

    private static Color Rgb(byte red, byte green, byte blue)
    {
        return new Color32(red, green, blue, 0xFF);
    }
}
