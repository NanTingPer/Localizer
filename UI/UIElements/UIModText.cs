using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;

namespace Localizer.UI.UIElements;

public class UIModText : UIMyText
{
    public Mod Mod { get; set; }
    public UIModText(string text, Mod mod, float textScale = 1, bool large = false) : base(text, textScale, large)
    {
        Mod = mod;
    }
}
