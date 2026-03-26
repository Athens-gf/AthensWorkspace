using System.Drawing;
using AthensWorkspace.Utility;

namespace Utility;

[AttributeUsage(AttributeTargets.Field)]
public class AColor(Color color) : Attribute
{
    public Color Color { get; } = color;

    public AColor(string color) : this(ColorTranslator.FromHtml(color))
    {
    }
}

public static class ExAColor
{
    private static readonly Dictionary<Enum, Color> ColorCache = new();
    public static Color GetColor(this Enum instance) => ExAttribute.GetValue<AColor, Color>(instance, ColorCache, icon => icon.Color, _ => new Color(), _ => new Color());
    public static string ToHtml(this Color color) => ColorTranslator.ToHtml(color);
    public static string ToHtmlWithAlpha(this Color color, double alpha = 0.5) => $"rgba({color.R}, {color.G}, {color.B}, {alpha})";
}