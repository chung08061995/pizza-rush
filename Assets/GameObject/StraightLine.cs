using Sirenix.OdinInspector;
using UnityEngine;

public class StraightLine : MonoBehaviour
{
    public Renderer rend;

    [Button]
    void SetToonColor(Color tint)
    {
        Material mat = rend.material; // tạo instance riêng, không ảnh hưởng material gốc
        mat.SetColor("_Color", tint);
    }
}
