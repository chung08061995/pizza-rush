using Sirenix.OdinInspector;
using UnityEngine;


public class HomeNavigationButton : MonoBehaviour
{
    [SerializeField] private DraftUtils.OptionalButtonGroup button;

    [SerializeField] private DraftUtils.AnimationPopup selectAnimation;
    [SerializeField] private DraftUtils.AnimationPopup deselectAnimation;


    public DraftUtils.OptionalButtonGroup Button => button;

    void Start()
    {
        button.RegisterClickEvents();
    }
    [Button]
    public void Select()
    {
        selectAnimation.In(null);
    }
    [Button]
    public void Deselect()
    {
        deselectAnimation.In(null);
    }
}
