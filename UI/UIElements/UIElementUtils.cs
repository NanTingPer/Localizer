using Microsoft.Xna.Framework;
using Terraria.UI;

namespace Localizer.UI.UIElements;

public static class UIElementUtils
{
    public static Rectangle GetRectangle(this UIElement uie)
    {
        var dims = uie.GetDimensions();
        var pos = dims.Position();
        var width = (int)dims.Width;
        var height = (int)dims.Height;
        return new Rectangle((int)pos.X, (int)pos.Y, width, height);
    }
}
