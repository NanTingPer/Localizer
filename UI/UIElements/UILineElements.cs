using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.UI;

namespace Localizer.UI.UIElements;

public class UILineElements : UIElement
{
    public static Texture2D White = new Texture2D(Main.graphics.GraphicsDevice, 1, 1);
    public override void Draw(SpriteBatch spriteBatch)
    {
        White.SetData([Color.White]);
        base.Draw(spriteBatch);
        var dims = GetDimensions();
        var pos = dims.Position();
        var width = (int)dims.Width;
        var height = (int)dims.Height;

        spriteBatch.Draw(White, new Rectangle((int)pos.X, (int)pos.Y, width, 1), Color.White * 0.9f);
        spriteBatch.Draw(White, new Rectangle((int)pos.X, (int)(pos.Y + height - 1), width, 1), Color.White * 0.9f);
        spriteBatch.Draw(White, new Rectangle((int)pos.X, (int)pos.Y, 1, height), Color.White);
        spriteBatch.Draw(White, new Rectangle((int)(pos.X + width - 1), (int)pos.Y, 1, height), Color.White);

    }
}
