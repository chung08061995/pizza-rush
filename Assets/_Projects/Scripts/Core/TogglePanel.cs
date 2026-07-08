using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Component base: 1 button toggle mở/đóng panel.
/// </summary>
public class TogglePanel : MonoBehaviour
{
    [SerializeField] private Button toggleButton;
    [SerializeField] private GameObject panel;

    private void Start()
    {
        panel.SetActive(false);
        toggleButton.onClick.AddListener(Toggle);
    }

    private void Toggle()
    {
        panel.SetActive(!panel.activeSelf);
    }
}
