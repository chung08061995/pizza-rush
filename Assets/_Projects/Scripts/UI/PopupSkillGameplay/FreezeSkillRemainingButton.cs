using UnityEngine;
using UnityEngine.UI;

public class FreezeSkillRemainingButton : DraftUtils.DraftMonoBehaviour
{
    [SerializeField] private SkillRemainingDataView skillRemainingDataView;
    [SerializeField] private Image fillImgae;


    public SkillRemainingDataView SkillRemainingDataView => skillRemainingDataView;
    public void SetFill(float ratio)
    {
        fillImgae.fillAmount = ratio;
    }
}
