using TMPro;
using UnityEngine;

public class AnchorPoint : DraftUtils.DraftMonoBehaviour
{
    [SerializeField] private TMP_Text indexText;
    [SerializeField] private Renderer m_Renderer;
    public Vector2Int CellPosition { get; set; }
    public void SetColor(Color color)
    {
        m_Renderer.material.color = color;
    }
    public void SetIndex(SerializableVector2Int index)
    {
        CellPosition = index;
        indexText.text = $"({index.x}, {index.y})";
    }
}