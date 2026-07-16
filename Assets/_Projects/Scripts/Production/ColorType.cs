using UnityEngine;

public enum ColorType
{
    None = 1,
    Red = 100,
    Green = 200,
    Blue = 300,
    White = 400,
    Orange = 500,
    Yellow = 600,
    Brown = 700,
    Cyan = 800,
    DarkPurple = 900,
    Pink = 1000,
    Violet = 1100,
    Lime = 1200,
    Navy = 1300,
    Gray = 1400,

}

public static class ColorTypeUtils
{
    public static Color ToColor(this ColorType colorType)
    {
        return colorType switch
        {
            ColorType.Red => Color.red,
            ColorType.Green => Color.green,
            ColorType.Blue => Color.blue,
            _ => Color.white,
        };
    }
}
