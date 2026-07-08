using UnityEngine;

[CreateAssetMenu(menuName = GameConstain.ScriptableObjectsPath.ParametterGameConfig + nameof(ParametterGameConfigSO))]
public class ParametterGameConfigSO : ScriptableObject
{
    [Header("Gameplay Configs")]
    /// <summary>
    /// Used in ContainerFactory.cs for container movement speed.
    /// </summary>
    [Tooltip("Used in ContainerFactory.cs for container movement speed.")]
    public float ContainerMoveSpeed = 8f;

    /// <summary>
    /// Used in ContainerFlyAwayState.cs for container fly away animation duration.
    /// </summary>
    [Tooltip("Used in ContainerFlyAwayState.cs for container fly away animation duration.")]
    public float ContainerFlyAwayDuration = 1.5f;

    /// <summary>
    /// Used in DragContainerState.cs for drag container animation duration.
    /// </summary>
    [Tooltip("Used in DragContainerState.cs for drag container animation duration.")]
    public float DragContainerDuration = 0.5f;

    /// <summary>
    /// Vertical offset applied to containers while dragging (Y world coordinate).
    /// </summary>
    [Tooltip("Vertical offset applied to containers while dragging (Y world coordinate).")]
    public float DragContainerBonusY = 0f;

    /// <summary>
    /// Used in LevelRunner.cs as default time limit for a level.
    /// </summary>
    [Tooltip("Used in LevelRunner.cs as default time limit for a level.")]
    public float DefaultLevelTime = 20f;

    /// <summary>
    /// Used in WinState.cs for the delay before showing the win popup.
    /// </summary>
    [Tooltip("Used in WinState.cs for the delay before showing the win popup.")]
    public float WinDelay = 2f;

    [Header("UI & System Configs")]
    /// <summary>
    /// Used in InitProgress.cs for the delay before loading the main scene.
    /// </summary>
    [Tooltip("Used in InitProgress.cs for the delay before loading the main scene.")]
    public float InitDelay = 0.2f;

    /// <summary>
    /// Used in PopupMain.cs for setting the follower UI speed.
    /// </summary>
    [Tooltip("Used in PopupMain.cs for setting the follower UI speed.")]
    public float MainPopupFollowerSpeed = 2000f;

    /// <summary>
    /// Maximum number of productions visible on a production line at once.
    /// </summary>
    [Tooltip("Maximum number of productions visible on a production line at once.")]
    public int MaxProductionOnLine = 5;

    /// <summary>
    /// Total wait time for the animation sequence when transferring products to container.
    /// </summary>
    [Tooltip("Total wait time for the animation sequence when transferring products to container.")]
    public float AnimationWait = 3f;

    /// <summary>
    /// Ease used when animating products into container.
    /// </summary>
    [Tooltip("Ease used when animating products into container.")]
    public DG.Tweening.Ease ProductionEase = DG.Tweening.Ease.Linear;

    /// <summary>
    /// Ease used when animating container flying up.
    /// </summary>
    [Tooltip("Ease used when animating container flying up.")]
    public DG.Tweening.Ease ContainerFlyAwayUpEase = DG.Tweening.Ease.OutCubic;

    /// <summary>
    /// Ease used when animating container flying off screen.
    /// </summary>
    [Tooltip("Ease used when animating container flying off screen.")]
    public DG.Tweening.Ease ContainerFlyAwayOffScreenEase = DG.Tweening.Ease.InOutBack;
}
