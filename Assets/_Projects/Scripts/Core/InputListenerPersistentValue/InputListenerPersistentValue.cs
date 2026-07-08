using TMPro;
using UnityEngine;

/// <summary>
/// Base generic class: lắng nghe TMP_InputField, khi input thay đổi thì parse sang T
/// và gắn vào PersistentValue đã đăng ký.
/// </summary>
public abstract class InputListenerPersistentValue<T> : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;

    private DraftUtils.PersistentValue<T> _persistentValue;

    public void SetPersistentValue(DraftUtils.PersistentValue<T> persistentValue)
    {
        _persistentValue = persistentValue;
        inputField.text = _persistentValue.Value.ToString();

        inputField.onValueChanged.RemoveAllListeners();
        inputField.onValueChanged.AddListener(OnInputChanged);
    }

    private void OnInputChanged(string text)
    {
        if (TryParse(text, out T value))
        {
            _persistentValue.SetValue(value);
            _persistentValue.Save();
            _persistentValue.Notify();
        }
    }

    protected abstract bool TryParse(string text, out T value);
}
