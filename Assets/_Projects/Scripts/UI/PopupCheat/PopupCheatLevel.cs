using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Cheat level: nhập số level rồi bấm Go để load level đó.
/// Đặt trong panel của PopupCheat.
/// </summary>
public class PopupCheatLevel : DraftUtils.DraftMonoBehaviour
{
    [SerializeField] private InputListenerIntPersistentValue levelInput;

    private void Start()
    {
        levelInput.SetPersistentValue(DataManager.Instance.Level);
    }

}
