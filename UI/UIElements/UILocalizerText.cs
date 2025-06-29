#nullable enable
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;


namespace Localizer.UI.UIElements;

public class UILocalizerText : UIMyText
{
    public static RenderTarget2D RT2D = new RenderTarget2D(Main.graphics.GraphicsDevice, 10, 10, false, SurfaceFormat.Color, DepthFormat.None, 1, usage: RenderTargetUsage.PreserveContents);

    public required Mod Mod { get; set; }
    public string? LocalPath { get; set; }
    public string? NetPath { get; set; }
    public UILocalizerText(string text, float textScale = 1, bool large = false) : base(text, textScale, large)
    {
    }
}
