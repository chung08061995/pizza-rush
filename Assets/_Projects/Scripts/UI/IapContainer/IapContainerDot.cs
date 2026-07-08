using UnityEngine;

public class IapContainerDot : MonoBehaviour
{
    [SerializeField] private DraftUtils.AnimationPopup selectAnimation;
    [SerializeField] private DraftUtils.AnimationPopup deselectAnimation;

    public void SetActiveState(bool isActive)
    {
        if (isActive)
        {
            selectAnimation.In(null);
        }
        else
        {
            deselectAnimation.In(null);
        }
    }
}
