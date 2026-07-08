/// <summary>
/// Int version: parse string sang int.
/// </summary>
public class InputListenerIntPersistentValue : InputListenerPersistentValue<int>
{
    protected override bool TryParse(string text, out int value)
    {
        return int.TryParse(text, out value);
    }
}
